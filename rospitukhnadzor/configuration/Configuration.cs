namespace RosPitukhNadzor
{
    internal class Configuration
    {
        public string? TelegramBotToken { get; set; } = null;

        public int? BanTimeSpan { get; set; } = 15;
        public int? AutoBanTimeSpan { get; set; } = 5;
        public int? BanWarningsCount { get; set; } = 2;

        public int? WarningExpirationTimeSpan { get; set; } = 5;

        public Dictionary<string, string[]>? BanTokenWords { get; set; } = new();

        public string[]? BeerTokens { get; set; } = { "пива", "пиву", "пиво", "пивом", "пиве",
                                                      "пивка", "пивку", "пивко", "пивком", "пивке",
                                                      "пиваса", "пивасу", "пивас", "пивасом", "пивасе",
                                                      "пивчанского", "пивчанскому", "пивчанского", "пивчанским",
                                                      "пивчанском", "пивчанский", "пиго", "пивасика",
                                                      "пивасику", "пивасик", "пивасиком", "пивасике", "пивчик",
                                                      "пивчика", "пивчику", "пивчика", "пивчиком", "пивчике" };

    }
}
