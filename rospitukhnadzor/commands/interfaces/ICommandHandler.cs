using Telegram.Bot;
using Telegram.Bot.Types;

namespace RosPitukhNadzor.Commands
{
    interface ICommandHandler
    {
        Task RunAsync(ITelegramBotClient bot, Update update);
    }
}