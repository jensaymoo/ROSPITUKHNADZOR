using FluentValidation;
using RosPitukhNadzor.Commands;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Serilog;

namespace RosPitukhNadzor
{
    internal class TelegramBot : TelegramBotClient, ITelegramBotInstance
    {
        public ConfigurationBot config;

        ILogger loggerProvider;
        private IStorageProvider storage;
        private IEnumerable<IMessageHandler> handlers;


        private UpdateType[] allowedUpdates = new[] { UpdateType.Message, UpdateType.EditedMessage };

        public TelegramBot(ILogger logger, IConfigurationProvider configProvider, IStorageProvider storageBase, IEnumerable<IMessageHandler> messageHandlers) : 
            base(configProvider.GetConfiguration(new ConfigurationBotValidator())!.TelegramBotToken!)
        {
            loggerProvider = logger;
            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
            storage = storageBase;
            this.handlers = messageHandlers;
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
                var chat_type = current_message.Chat.Type;

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

                    //если сообщение команда и в групповом чате и не адресовано боту то инорим
                    if ((chat_type == ChatType.Group || chat_type == ChatType.Supergroup) && !command_splited.Where(a => a == me_user.Username).Any())
                    {
                        return;
                    }

                    foreach (var comm in command_splited.Where(a => a.StartsWith('/')))
                    {
                        
                        var command_handlers = handlers.SelectMessageHandlers(x => x.CommandName == comm && x.ChatTypes.Any(f => f.Equals(chat_type)));
                        if (command_handlers.Any())
                        {
                            command_handlers.ForEach(async x => await x.RunAsync(bot, update));
                        }
                        else
                        {
                            command_handlers = handlers.SelectMessageHandlers(x => x.CommandName == "unknown" && x.ChatTypes.Any(f => f.Equals(chat_type)));
                            command_handlers.ForEach(async x => await x.RunAsync(bot, update));
                        }
                    }
                }
                else
                {
                    var message_handlers = handlers.SelectMessageHandlers(x => x.CommandName == null && x.ChatTypes.Any(f => f.Equals(chat_type)));
                    if (message_handlers.Any())
                    {
                        message_handlers.ForEach(async x => await x.RunAsync(bot, update));
                    }
                }
            }
        }
        private Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
        {
            var ErrorMessage = error switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => error.ToString()

            };

            loggerProvider.Fatal(error, "Fatal error in {Bot}" , nameof(TelegramBot));
            return Task.CompletedTask;
        }


    }

}
