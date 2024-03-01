using FluentValidation;

namespace RosPitukhNadzor
{
    internal class ConfigurationBot
    {
        public string? TelegramBotToken { get; set; } = null;

        public int? BanTimeSpan { get; set; } = 15;
        public int? AutoBanTimeSpan { get; set; } = 5;
        public int? BanWarningsCount { get; set; } = 2;

        public int? WarningExpirationTimeSpan { get; set; } = 5;

        //public Dictionary<string, string[]>? BanTokenWords { get; set; } = new();

        public string[]? BeerTokens { get; set; } = { "пива", "пиву", "пиво", "пивом", "пиве",
                                                      "пивка", "пивку", "пивко", "пивком", "пивке",
                                                      "пиваса", "пивасу", "пивас", "пивасом", "пивасе",
                                                      "пивчанского", "пивчанскому", "пивчанского", "пивчанским",
                                                      "пивчанском", "пивчанский", "пиго", "пивасика",
                                                      "пивасику", "пивасик", "пивасиком", "пивасике", "пивчик",
                                                      "пивчика", "пивчику", "пивчика", "пивчиком", "пивчике" };

    }
    internal class ConfigurationBotValidator : AbstractValidator<ConfigurationBot>
    {
        public ConfigurationBotValidator()
        {
            RuleFor(opt => opt.TelegramBotToken)
                .NotNull()
                .NotEmpty()
                .MinimumLength(43)
                .MaximumLength(46)
                .Matches(@"^[0-9]{8,10}:[a-zA-Z0-9_-]{35}$");

            RuleFor(opt => opt.BanTimeSpan)
                .NotNull()
                .InclusiveBetween(1, 300);

            RuleFor(opt => opt.AutoBanTimeSpan)
                .NotNull()
                .InclusiveBetween(1, 300);

            RuleFor(opt => opt.BanWarningsCount)
                .NotNull()
                .InclusiveBetween(0, 10);

            RuleFor(opt => opt.WarningExpirationTimeSpan)
                .NotNull()
                .InclusiveBetween(1, 300);

            RuleFor(opt => opt.BeerTokens)
                .NotNull();
        }
    }
}
