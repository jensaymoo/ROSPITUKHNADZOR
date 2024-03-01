using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [CommandHandler("unknown", ChatType.Group)]
    class UnknownCommandGroupHandler : ICommandHandler
    {
        public UnknownCommandGroupHandler(IConfigurationProvider configurationProvider, IStorageProvider storageBase)
        {

        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            Console.WriteLine("unknown");

            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            var problem_message = current_message.ReplyToMessage;
            var problem_user = problem_message?.From!;

            await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} ты ебанутый? че тебе от меня надо?", replyToMessageId: current_message.MessageId);

        }
    }
}