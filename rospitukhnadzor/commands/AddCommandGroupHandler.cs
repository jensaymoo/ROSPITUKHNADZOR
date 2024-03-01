using Newtonsoft.Json.Linq;
using RestSharp;
using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RosPitukhNadzor.Commands
{
    [CommandHandler("/add", ChatType.Group)]
    class AddCommandGroupHandler : ICommandHandler
    {
        IStorageProvider storageProvider;
        IConfigurationProvider configProvider;

        ConfigurationBot config;
        public AddCommandGroupHandler(IConfigurationProvider configuration, IStorageProvider storage)
        {
            storageProvider = storage;
            configProvider = configuration;

            config = configProvider.GetConfiguration(new ConfigurationBotValidator());
        }

        public async Task RunAsync(ITelegramBotClient bot, Update update)
        {
            Console.WriteLine("/add");

            var current_message = (update.Message ?? update.EditedMessage)!;
            var current_user = current_message.From!;

            var problem_message = current_message.ReplyToMessage;
            var problem_user = problem_message?.From!;

            var admins = await bot.GetChatAdministratorsAsync(current_message.Chat.Id);

            if (admins.Any(adm => adm.User.Id == current_user.Id))
            {
                var words = (current_message.Text?.Split(' ') ?? current_message.Caption?.Split(' ') ?? Array.Empty<string>())
                    .Where(str => !str.StartsWith("/add") && str != string.Empty)
                    .Select(str => new string(str.Where(x => char.IsWhiteSpace(x) || char.IsLetterOrDigit(x)).ToArray()).ToLower())
                    .Except(config.BeerTokens!);

                if (words.Any())
                {
                    var baned_words = await AddBanWords(words, current_message.Chat.Id);
                    await bot.SendTextMessageAsync(current_message.Chat.Id, $"питух @{current_user.Username} добавил в словарь: {string.Join(", ", baned_words)}");
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
            var chat_hash_string = (chat).GetHashCode().ToBytesString();

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
                        continue;
                    }
                }
            }

            var added_ban_words = new List<string>();
            foreach (var word in baned_words.Distinct())
            {
                if (!storageProvider.GetBanWords(w => w.ChatID == chat && w.Word == word).Any())
                {
                    await storageProvider.AddBanWordAsync(new BanWord(chat, word));
                    added_ban_words.Add(word);
                }
            }

            return added_ban_words;
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