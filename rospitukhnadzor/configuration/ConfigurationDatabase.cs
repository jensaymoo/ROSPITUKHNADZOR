using FluentValidation;
using LinqToDB;

namespace RosPitukhNadzor
{
    internal class ConfigurationDatabase
    {
        public string? DatabaseProvider {  get; set; }
        public string? DatabaseConfig { get; set; }
    }

    internal class ConfigurationDatabaseValidator : AbstractValidator<ConfigurationDatabase>
    {
        public ConfigurationDatabaseValidator()
        {
            RuleFor(opt => opt.DatabaseProvider)
                .NotNull()
                .NotEmpty();

            RuleFor(opt => opt.DatabaseConfig)
                .NotNull()
                .NotEmpty();
        }
    }
}
