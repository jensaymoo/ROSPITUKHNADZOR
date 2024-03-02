using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler("/mute", [ChatType.Group])]
    class MuteCommandGroupHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;

        public MuteCommandGroupHandler(IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            Console.WriteLine("/mute");

            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            var problem_message = current_message.ReplyToMessage;
            var problem_user = problem_message?.From!;

            var admins = await bot.GetChatAdministratorsAsync(current_message.Chat.Id);

            if (problem_message == null)
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"ты ебобо штоле? кого пломбировать то?", replyToMessageId: current_message.MessageId);

                return;
            }
            if (problem_user.Id == bot.BotId)
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"анус себе запломбировать не хочешь?", replyToMessageId: current_message.MessageId);

                return;
            }
            if (admins.Any(x => x.User.Id == problem_user.Id))
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"он одмен, ты че", replyToMessageId: current_message.MessageId);

                return;
            }

            var votes_in_this_chat = storageProvider.GetWarnings(warn => warn.ChatID == current_message.Chat.Id && warn.ToUserID == problem_user.Id);
            var current_user_voted = votes_in_this_chat.Where(warn => warn.FromUserID == current_user.Id).Any();

            if (current_user_voted)
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"ты уже голосовал против @{problem_user.Username}", replyToMessageId: current_message.MessageId);
                return;
            }
            else
            {
                await storageProvider.AddWarningAsync(new Warning(current_message.Chat.Id, current_user.Id, problem_user.Id, DateTime.Now + TimeSpan.FromMinutes(config.WarningExpirationTimeSpan!.Value)));
            }

            //чекаем коллво выданных предупредений
            var count_votes_to = votes_in_this_chat
                .Where(warn => warn.ToUserID == problem_user.Id)
                .Count();

            if (count_votes_to > config.BanWarningsCount!.Value || admins.Any(adm => adm.User.Id == current_user.Id))
            {
                //сообщаем что питух запломбирован и баним его
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"питуху @{problem_user.Username} выписан мьют " +
                    $"на {config.BanTimeSpan!.Value} мин");

                var restrictions = new ChatPermissions()
                {
                    CanSendVoiceNotes = false,
                    CanSendVideos = false,
                    CanSendVideoNotes = false,
                    CanSendPolls = false,
                    CanSendPhotos = false,
                    CanSendOtherMessages = false,
                    CanSendMessages = false,
                    CanSendDocuments = false,
                    CanAddWebPagePreviews = false,
                    CanChangeInfo = false,
                    CanManageTopics = false,
                    CanPinMessages = false,
                    CanSendAudios = false,
                };
                await bot.RestrictChatMemberAsync(current_message.Chat.Id, problem_user.Id, restrictions, untilDate: DateTime.Now + TimeSpan.FromMinutes(config.BanTimeSpan!.Value));
                await storageProvider.RemoveWarningAsync(warn => warn.ChatID == current_message.Chat.Id && warn.ToUserID == problem_user.Id);

            }
            else
            {
                //если колва необходимого для бана не набралось и заявивший юзер не одмен
                //то сообщаем что питуху выдано предупреждение
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"питуху @{problem_user.Username} выдано пердупреждение на " +
                    $"{config.WarningExpirationTimeSpan!.Value} мин, для мьюта еше нужно {config.BanWarningsCount!.Value - count_votes_to + 1} " +
                    $"голосов", replyToMessageId: problem_message.MessageId);

            }
        }
    }
}