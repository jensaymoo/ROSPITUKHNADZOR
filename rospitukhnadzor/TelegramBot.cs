using FluentValidation;
using RosPitukhNadzor.Commands;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor
{
    internal class TelegramBot : TelegramBotClient, ITelegramBotInstance
    {
        public ConfigurationBot config;

        private IStorageProvider storage;
        private IEnumerable<ICommandHandler> commandHandlers;

        private UpdateType[] allowedUpdates = new[] { UpdateType.Message, UpdateType.EditedMessage };

        public TelegramBot(IConfigurationProvider configProvider, IStorageProvider storageBase, IEnumerable<ICommandHandler> commands) : 
            base(configProvider.GetConfiguration(new ConfigurationBotValidator())!.TelegramBotToken!)
        {
            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
            storage = storageBase;
            commandHandlers = commands;
        }

        async public Task Run()
        {
            using var cts = new CancellationTokenSource();
            var _receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = allowedUpdates,
                ThrowPendingUpdates = true,
            };

            this.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token);

            PeriodicTimer timer = new(TimeSpan.FromMinutes(1));
            while (await timer.WaitForNextTickAsync())
            {

                await storage.ClearExpiredWarningsAsync();
            }
        }

        private async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            
            bool message_instance_is_not_null = update.Message != null || update.EditedMessage != null;
            bool message_text_is_not_null = (update.Message?.Text != null || update.EditedMessage?.Text != null || update.Message?.Caption != null || update.EditedMessage?.Caption != null);

            if (allowedUpdates.Any(type => type.Equals(update.Type)) && message_instance_is_not_null && message_text_is_not_null)
            {
                var current_message = (update.Message ?? update.EditedMessage)!;
                var current_user = current_message.From!;

                var me_user = await bot.GetMeAsync();

                //игнорим сообещния от себя самого
                if (current_user.Id == me_user.Id)
                {
                    return;
                }

                //чекаем команда ли это
                var command_entity = current_message.Entities?
                    .Where(e => e.Type == MessageEntityType.BotCommand)?.SingleOrDefault();

                if (command_entity != null)
                {
                    var command_line = current_message.Text!.Substring(command_entity!.Offset, command_entity!.Length);
                    var command_splited = command_line.Split('@');

                    foreach (var comm in command_splited.Where(a => a.StartsWith('/')))
                    {
                        var command_handlers = commandHandlers.Where(x => x.GetType().GetCustomAttribute<CommandHandlerAttribute>()?.CommandName == comm);
                        if (command_handlers.Any())
                        {
                            command_handlers.ForEach(async x => await x.RunAsync(bot, update));
                        }
                        else
                        {
                            command_handlers = commandHandlers.Where(x => x.GetType().GetCustomAttribute<CommandHandlerAttribute>()?.CommandName == "unknown");
                            command_handlers.ForEach(async x => await x.RunAsync(bot, update));
                        }
                    }
                }

                //чекаем чат на приватность
                if (current_message.Chat.Type == ChatType.Private)
                {
                    await bot.SendTextMessageAsync(current_message.Chat.Id, $"этот чат приватный, добавь меня в группой чат, сделай одменом и тогда я наведу там порядок", replyToMessageId: current_message.MessageId);
                    return;
                }

                var admins = await bot.GetChatAdministratorsAsync(current_message.Chat.Id);


                if (command_entity != null)
                {
                    //var problem_message = current_message.ReplyToMessage;
                    //var problem_user = problem_message?.From!;

                    var command_line = current_message.Text!.Substring(command_entity!.Offset, command_entity!.Length);
                    var command_splited = command_line.Split('@');

                    //чекаем сюда ли это сообщение адресовано
                    if (command_splited.Where(a => a == me_user.Username).Any())
                    {
                        //foreach (var comm in command_splited.Where(a => a.StartsWith('/')))
                        //{
                        //    switch (comm)
                        //    {
                        //        case "/mute":

                        //            //if (problem_message == null)
                        //            //{
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"ты ебобо штоле? кого пломбировать то?", replyToMessageId: current_message.MessageId);

                        //            //    return;
                        //            //}
                        //            //if (problem_user.Id == bot.BotId)
                        //            //{
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"анус себе запломбировать не хочешь?", replyToMessageId: current_message.MessageId);

                        //            //    return;
                        //            //}
                        //            //if (admins.Any(x => x.User.Id == problem_user.Id))
                        //            //{
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"он одмен, ты че", replyToMessageId: current_message.MessageId);

                        //            //    return;
                        //            //}

                        //            //var votes_in_this_chat = storage.GetWarnings(warn => warn.ChatID == current_message.Chat.Id && warn.ToUserID == problem_user.Id);
                                    
                        //            //var current_user_voted = votes_in_this_chat
                        //            //    .Where(warn => warn.FromUserID == current_user.Id).Any();

                        //            //if (current_user_voted)
                        //            //{
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"ты уже голосовал против @{problem_user.Username}", replyToMessageId: current_message.MessageId);
                        //            //    return;
                        //            //}
                        //            //else
                        //            //{
                        //            //    await storage.AddWarningAsync(new Warning(current_message.Chat.Id, current_user.Id, problem_user.Id, DateTime.Now + TimeSpan.FromMinutes(config.WarningExpirationTimeSpan!.Value)));
                        //            //}

                        //            ////чекаем коллво выданных предупредений
                        //            //var count_votes_to = votes_in_this_chat
                        //            //    .Where(warn => warn.ToUserID == problem_user.Id)
                        //            //    .Count();

                        //            //if (count_votes_to > config.BanWarningsCount!.Value || admins.Any(adm => adm.User.Id == current_user.Id))
                        //            //{
                        //            //    //сообщаем что питух запломбирован и баним его
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"питуху @{problem_user.Username} выписан мьют " +
                        //            //        $"на {config.BanTimeSpan!.Value} мин");

                        //            //    var restrictions = new ChatPermissions()
                        //            //    {
                        //            //        CanSendVoiceNotes = false,
                        //            //        CanSendVideos = false,
                        //            //        CanSendVideoNotes = false,
                        //            //        CanSendPolls = false,
                        //            //        CanSendPhotos = false,
                        //            //        CanSendOtherMessages = false,
                        //            //        CanSendMessages = false,
                        //            //        CanSendDocuments = false,
                        //            //        CanAddWebPagePreviews = false,
                        //            //        CanChangeInfo = false,
                        //            //        CanManageTopics = false,
                        //            //        CanPinMessages = false,
                        //            //        CanSendAudios = false,
                        //            //    };
                        //            //    await bot.RestrictChatMemberAsync(current_message.Chat.Id, problem_user.Id, restrictions, untilDate: DateTime.Now + TimeSpan.FromMinutes(config.BanTimeSpan!.Value));
                        //            //    await storage.RemoveWarningAsync(warn => warn.ChatID == current_message.Chat.Id && warn.ToUserID == problem_user.Id);

                        //            //}
                        //            //else
                        //            //{
                        //            //    //если колва необходимого для бана не набралось и заявивший юзер не одмен
                        //            //    //то сообщаем что питуху выдано предупреждение
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"питуху @{problem_user.Username} выдано пердупреждение на " +
                        //            //        $"{config.WarningExpirationTimeSpan!.Value} мин, для мьюта еше нужно {config.BanWarningsCount!.Value - count_votes_to + 1} " +
                        //            //        $"голосов", replyToMessageId: problem_message.MessageId);

                        //            //}

                        //            return;

                        //        case "/add":
                        //            //if (admins.Any(adm => adm.User.Id ==  current_user.Id))
                        //            //{
                        //            //    var words = (current_message.Text?.Split(' ') ?? current_message.Caption?.Split(' ') ?? Array.Empty<string>())
                        //            //        .Where(str => !str.StartsWith("/add"))
                        //            //        .Where(str => str != string.Empty)
                        //            //        .Select(str => new string(str.Where(x => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x)).ToArray()).ToLower())
                        //            //        .Except(config.BeerTokens!);

                        //            //    if (words.Any())
                        //            //    {
                        //            //        var baned_words = await AddBanWords(words, current_message.Chat.Id);
                        //            //        await bot.SendTextMessageAsync(current_message.Chat.Id, $"питух @{current_user.Username} добавил в словарь: {string.Join(", ", baned_words)}");
                        //            //    }
                        //            //    else
                        //            //    {
                        //            //        await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} че добавлять то будем?");
                        //            //    }
                        //            //}
                        //            //else
                        //            //{
                        //            //    await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} иди отсюда, ты не овнер", replyToMessageId: current_message.MessageId);
                        //            //}
                        //            break;

                        //        default:
                        //            //await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} ты ебанутый? че тебе от меня надо?", replyToMessageId: current_message.MessageId);
                        //            break;
                        //    }
                        //}
                    }
                }
                else
                {
                    //ипмпровизация
                    if (current_message.Text != null)
                    {
                        switch (string.Concat(current_message.Text.Where(c => char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)).ToArray()).ToLower())
                        {
                            case "да":
                                await bot.SendTextMessageAsync(current_message.Chat.Id, $"ПИЗДА", replyToMessageId: current_message.MessageId);
                                break;
                            case "нет":
                                await bot.SendTextMessageAsync(current_message.Chat.Id, $"ПИДОРА ОТВЕТ", replyToMessageId: current_message.MessageId);
                                break;
                        }
                    }

                    var input_message = (current_message!.Text ?? current_message!.Caption ?? string.Empty).ToLower();
                    //проверяем на есть ли пивко в пятничку
                    if (await CheckMessagFromBeerTime(input_message, current_message.Chat.Id))
                    {
                        await bot.SendTextMessageAsync(current_message.Chat.Id, $"🍻", replyToMessageId: current_message.MessageId);
                    }

                    //проверяем на есть ли запрещенные слова
                    ;
                    if (storage.GetBanWords(f => f.ChatID == current_message.Chat.Id).Any() && current_message != null)
                    {
                        var founded_ban_words = await CheckMessageFromBanWords(input_message, current_message.Chat.Id);
                        if (founded_ban_words.Any() && !admins.Any(x => x.User.Id == current_user.Id))
                        {
                            await bot.SendTextMessageAsync(current_message.Chat.Id, $"за свой гнилой базар («{string.Join("», «", founded_ban_words)}»), " +
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
                            return;
                        }
                    }
                }
            }
        }
        private async Task<bool> CheckMessagFromBeerTime(string inputMessage, long chat)
        {
            if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
            {
                var input_string = string.Concat(inputMessage.Where(c => char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)).ToArray());
                var found_beer_words = config.BeerTokens!.Where(x => input_string.Contains(x));

                return found_beer_words.Any();
            }

            return false;
        }
        private async Task<IEnumerable<string>> CheckMessageFromBanWords(string inputMessage, long chat)
        {
            var input_string = string.Concat(inputMessage
                .Where(c => char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)).ToArray());
            var found_ban_words = storage.GetBanWords(f => f.ChatID == chat)
                .Where(x => input_string.Contains(x.Word))
                .Select(x => x.Word);

            return found_ban_words!;
        }

        //private async Task<IEnumerable<string>> AddBanWords(IEnumerable<string> words, long chat)
        //{
        //    var chat_hash_string = (chat).GetHashCode().ToBytesString();

        //    var baned_words = new List<string>();
        //    foreach (var word in words)
        //    {
        //        var options = new RestClientOptions("https://ws3.morpher.ru/russian");
        //        using (var client = new RestClient(options))
        //        {

        //            var request = new RestRequest("declension");
        //            request.AddParameter("s", word);
        //            request.AddParameter("format", "json");

        //            baned_words.Add(word);

        //            try
        //            {
        //                var response = await client.GetAsync(request);

        //                if (response.StatusCode == System.Net.HttpStatusCode.OK)
        //                {
        //                    using var doc = JsonDocument.Parse(response.Content!);
        //                    {
        //                        var responsed_values = EnumerateJsonPaths(doc.RootElement)
        //                            .Select(x => JObject.Parse(response.Content!).SelectToken(x)?.Value<string>())
        //                            .Where(x => !string.IsNullOrEmpty(x));

        //                        baned_words.AddRange(responsed_values!);
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                continue;
        //            }
        //        }
        //    }

        //    var added_ban_words = new List<string>();
        //    foreach (var word in baned_words.Distinct())
        //    {
        //        if (!storage.GetBanWords(w => w.ChatID == chat && w.Word == word).Any())
        //        {
        //            await storage.AddBanWordAsync(new BanWord(chat, word));
        //            added_ban_words.Add(word);
        //        }
        //    }

        //    return added_ban_words;
        //}
        //static IEnumerable<string> EnumerateJsonPaths(JsonElement doc)

        //{
        //    var queu = new Queue<(string ParentPath, JsonElement element)>();
        //    queu.Enqueue(("", doc));
        //    while (queu.Any())
        //    {
        //        var (parentPath, element) = queu.Dequeue();
        //        switch (element.ValueKind)
        //        {
        //            case JsonValueKind.Object:
        //                parentPath = parentPath == ""
        //                    ? parentPath
        //                    : parentPath + ".";
        //                foreach (var nextEl in element.EnumerateObject())
        //                {
        //                    queu.Enqueue(($"{parentPath}{nextEl.Name}", nextEl.Value));
        //                }
        //                break;
        //            case JsonValueKind.Array:
        //                foreach (var (nextEl, i) in element.EnumerateArray().Select((jsonElement, i) => (jsonElement, i)))
        //                {
        //                    queu.Enqueue(($"{parentPath}[{i}]", nextEl));
        //                }
        //                break;
        //            case JsonValueKind.Undefined:
        //            case JsonValueKind.String:
        //            case JsonValueKind.Number:
        //            case JsonValueKind.True:
        //            case JsonValueKind.False:
        //            case JsonValueKind.Null:
        //                yield return parentPath;
        //                break;
        //            default:
        //                throw new ArgumentOutOfRangeException();
        //        }
        //    }
        //}
        private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()
            };
            return Task.CompletedTask;
        }


    }

}
