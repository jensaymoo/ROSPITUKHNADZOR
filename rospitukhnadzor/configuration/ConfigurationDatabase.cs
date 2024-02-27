using FluentValidation;

namespace RosPitukhNadzor
{
    internal class ConfigurationDatabase
    {
        public string? DatabaseProvider {  get; set; } = "test";
        public string? DatabaseConfig { get; set; } = "test";
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
