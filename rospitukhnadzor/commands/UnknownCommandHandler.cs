using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler("unknown", ChatType.Private, ChatType.Channel, ChatType.Sender)]
    class UnknownCommandHandler : IMessageHandler
    {
        public UnknownCommandHandler(ILogger loggerProvider, IConfigurationProvider configuration, IStorageProvider storage)
        {

        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            await bot.SendTextMessageAsync(current_message.Chat.Id, $"этот чат приватный, добавь меня в группой чат, сделай одменом и тогда я наведу там порядок", replyToMessageId: current_message.MessageId);
        }
    }
}