using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler(null, ChatType.Group)]
    class AutoBanGroupHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;
        public AutoBanGroupHandler (IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            var current_message_text = (current_message!.Text ?? current_message!.Caption ?? string.Empty).ToLower();
            var admins = await bot.GetChatAdministratorsAsync(current_message.Chat.Id);

            var founded_ban_words = await CheckMessageFromBanWords(current_message_text, current_message.Chat.Id);
            if (founded_ban_words.Any() && !admins.Any(x => x.User.Id == current_user.Id))
            {
                //чекаем на наличие банслов и от кого сообщение (игнорим сообщения от админов)
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"за свой гнилой базар ({"«" + string.Join("», «", founded_ban_words) + "»"}), " +
                    $"питух @{current_user.Username} отхватил автобан на {config.AutoBanTimeSpan!.Value} мин");

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
                await bot.RestrictChatMemberAsync(current_message.Chat.Id, current_user.Id, restrictions, untilDate: DateTime.Now + TimeSpan.FromMinutes(config.AutoBanTimeSpan!.Value));
            }
        }

        private async Task<IEnumerable<string>> CheckMessageFromBanWords(string inputMessage, long chat)
        {
            return await Task.Run(() =>
            {
                var input_string = string.Concat(inputMessage
                    .Where(c => char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)).ToArray());

                var found_ban_words = storageProvider.GetBanWords(f => f.ChatID == chat)
                    .Where(x => input_string.Contains(x.Word))
                    .Select(x => x.Word);

                return found_ban_words!;
            });
        }
    }
}