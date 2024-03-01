using FluentValidation;

namespace RosPitukhNadzor
{
    internal interface IConfigurationProvider
    {
        public T GetConfiguration<T>(AbstractValidator<T>? validator = null);
    }
}
