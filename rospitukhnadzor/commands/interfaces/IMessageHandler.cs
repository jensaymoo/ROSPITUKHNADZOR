using Telegram.Bot;
using Telegram.Bot.Types;

namespace RosPitukhNadzor.Commands
{
    interface IMessageHandler
    {
        Task RunAsync(ITelegramBotClient bot, Update update);
    }
}