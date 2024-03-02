using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler(null, ChatType.Private, ChatType.Channel, ChatType.Sender)]
    class MessageProcessingHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;
        public MessageProcessingHandler(IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            await bot.SendTextMessageAsync(current_message.Chat.Id, $"этот чат приватный, добавь меня в группой чат, сделай одменом и тогда я наведу там порядок", replyToMessageId: current_message.MessageId);
        }
    }
}