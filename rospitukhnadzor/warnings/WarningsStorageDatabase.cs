using FluentValidation;

namespace RosPitukhNadzor
{
    internal class WarningsStorageDatabase : IWarningsStorage
    {
        private ConfigurationDatabase config;

        public WarningsStorageDatabase(IConfigurationProvider configurationProvider)
        {
            config = configurationProvider.GetConfiguration<ConfigurationDatabase>();

            var validator = new ConfigurationDatabaseValidator();
            validator.ValidateAndThrow(config);
        }

        public void AddWarning(Warning warning)
        {
            throw new NotImplementedException();
        }
        public int RemoveWarning(Predicate<Warning> warning)
        {
            throw new NotImplementedException();
        }

        public int ClearExpiredWarnings()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Warning> GetWarnings()
        {
            throw new NotImplementedException();
        }
    }
}
