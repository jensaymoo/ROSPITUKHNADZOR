using FluentValidation;

namespace RosPitukhNadzor
{
    internal class WarningsStorageInMemory : IWarningsStorage
    {
        private List<Warning> _warnings = new();

        public WarningsStorageInMemory (IConfigurationProvider configurationProvider)
        {

        }

        public void AddWarning(Warning warning)
        {
            _warnings.Add(warning);
        }
        public int RemoveWarning(Predicate<Warning> warning)
        {
            return _warnings.RemoveAll(warning);
        }

        public int ClearExpiredWarnings()
        {
            return RemoveWarning(warn => warn.MuteExpiries < DateTime.Now);
        }

        public IEnumerable<Warning> GetWarnings()
        {
            return _warnings;
        }
    }
}
