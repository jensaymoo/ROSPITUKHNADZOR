using Newtonsoft.Json.Linq;
using RestSharp;
using Serilog;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [MessageHandler("/add", ChatType.Group)]
    class AddCommandGroupHandler : IMessageHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;
        ILogger loggerProvider;

        ConfigurationBot config;
        public AddCommandGroupHandler(ILogger logger, IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;
            loggerProvider = logger;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            var admins = await bot.GetChatAdministratorsAsync(current_message.Chat.Id);

            if (admins.Any(adm => adm.User.Id == current_user.Id))
            {
                var words = (current_message.Text?.Split(' ') ?? current_message.Caption?.Split(' ') ?? Array.Empty<string>())
                    .Where(str => !str.StartsWith("/add") && str != string.Empty)
                    .Select(str => new string(str.Where(x => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x)).ToArray()).ToLower())
                    .Except(config.BeerTokens!);

                if (words.Any())
                {
                    loggerProvider.Information("Request add {@BanWords:l} to banlist in {ChatID} on {TimeSatmp}", words, current_message.Chat.Id, DateTime.Now);

                    var baned_words = await AddBanWords(words, current_message.Chat.Id);

                    if (baned_words.Any())
                        await bot.SendTextMessageAsync(current_message.Chat.Id, $"питух @{current_user.Username} добавил в словарь: {"«" + string.Join("», «", baned_words) + "»"}");
                    else
                        await bot.SendTextMessageAsync(current_message.Chat.Id, $"питух @{current_user.Username} попытался добавить в словарь: {"«" + string.Join("», «", words) + "»"}, но не добавил, пушто они уже и так есть");
                }
                else
                {
                    await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} че добавлять то будем?");
                }
            }
            else
            {
                await bot.SendTextMessageAsync(current_message.Chat.Id, $"@{current_user.Username} иди отсюда, ты не овнер", replyToMessageId: current_message.MessageId);
            }
        }
        private async Task<IEnumerable<string>> AddBanWords(IEnumerable<string> words, long chat)
        {
            var added_ban_words = new List<string>();
            var declensions_words = await GetDeclensions(words);
            foreach (var word in declensions_words)
            {
                if (!storageProvider.GetBanWords(w => w.ChatID == chat && w.Word == word).Any())
                {
                    await storageProvider.AddBanWordAsync(new BanWord(chat, word));
                    added_ban_words.Add(word);
                }
            }
            return added_ban_words;
        }
        private async Task<IEnumerable<string>> GetDeclensions(IEnumerable<string> words)
        {
            var baned_words = new List<string>();

            foreach (var word in words)
            {
                var options = new RestClientOptions("https://ws3.morpher.ru/russian");

                using (var client = new RestClient(options))
                {
                    var request = new RestRequest("declension");
                    request.AddParameter("s", word);
                    request.AddParameter("format", "json");

                    baned_words.Add(word);

                    try
                    {
                        var response = await client.GetAsync(request);

                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            using var doc = JsonDocument.Parse(response.Content!);
                            {
                                var responsed_values = EnumerateJsonPaths(doc.RootElement)
                                    .Select(x => JObject.Parse(response.Content!).SelectToken(x)?.Value<string>())
                                    .Where(x => !string.IsNullOrEmpty(x));

                                baned_words.AddRange(responsed_values!);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        loggerProvider.Error(ex, "Exception when loading declensions for ban words. {mes:l}", ex.Message);
                        break;
                    }
                }
            }
            loggerProvider.Information("Declension service returned {@BanWords:l} on {TimeSatmp}", baned_words, DateTime.Now);
            return baned_words.Distinct();
        }

        static IEnumerable<string> EnumerateJsonPaths(JsonElement doc)
        {
            var queu = new Queue<(string ParentPath, JsonElement element)>();
            queu.Enqueue(("", doc));
            while (queu.Any())
            {
                var (parentPath, element) = queu.Dequeue();
                switch (element.ValueKind)
                {
                    case JsonValueKind.Object:
                        parentPath = parentPath == ""
                            ? parentPath
                            : parentPath + ".";
                        foreach (var nextEl in element.EnumerateObject())
                        {
                            queu.Enqueue(($"{parentPath}{nextEl.Name}", nextEl.Value));
                        }
                        break;
                    case JsonValueKind.Array:
                        foreach (var (nextEl, i) in element.EnumerateArray().Select((jsonElement, i) => (jsonElement, i)))
                        {
                            queu.Enqueue(($"{parentPath}[{i}]", nextEl));
                        }
                        break;
                    case JsonValueKind.Undefined:
                    case JsonValueKind.String:
                    case JsonValueKind.Number:
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                    case JsonValueKind.Null:
                        yield return parentPath;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

    }
}