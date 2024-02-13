using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    internal class ConfigurationValidator : AbstractValidator<Configuration>
    {
        public ConfigurationValidator()
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
