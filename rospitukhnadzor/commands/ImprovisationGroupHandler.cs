using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler(null, [ChatType.Group])]
    class ImprovisationGroupHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;
        public ImprovisationGroupHandler(IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            //немного ипмпровизация
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
        }
    }
}