using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler(null, [ChatType.Group])]
    class BeerTimeGroupHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;
        public BeerTimeGroupHandler(IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            var input_message = (current_message!.Text ?? current_message!.Caption ?? string.Empty).ToLower();

            //проверяем на есть ли пивко в пятничку
            if (await CheckMessagFromBeerTime(input_message, current_message.Chat.Id))
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"🍻", replyToMessageId: current_message.MessageId);
            }
        }

        private async Task<bool> CheckMessagFromBeerTime(string inputMessage, long chat)
        {
            return await Task.Run(() =>
            {
                if (DateTime.Now.DayOfWeek == DayOfWeek.Friday || DateTime.Now.DayOfWeek == DayOfWeek.Saturday)
                {
                    var input_string = string.Concat(inputMessage.Where(c => char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c)).ToArray());
                    var found_beer_words = config.BeerTokens!.Where(x => input_string.Contains(x));

                    return found_beer_words.Any();
                }

                return false;
            });
        }
    }
}