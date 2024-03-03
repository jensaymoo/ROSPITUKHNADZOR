using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler("unknown", [ChatType.Group])]
    class UnknownCommandGroupHandler : IMessageHandler
    {
        public UnknownCommandGroupHandler(ILogger logger, IConfigurationProvider configuration, IStorageProvider storage)
        {

        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} ты ебанутый? че тебе от меня надо?", replyToMessageId: current_message.MessageId);
        }
    }
}