using Telegram.Bot;

namespace RosPitukhNadzor
{
    public interface ITelegramBotInstance : ITelegramBotClient
    {
        public Task Run();
    }
}
