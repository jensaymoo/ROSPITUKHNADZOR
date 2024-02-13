using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    internal class WarningsStorageInMemory : IWarningsStorage
    {
        private List<Warning> _warnings = new();
        public void AddWarning(Warning warning)
        {
            _warnings.Add(warning);
        }
        public int RemoveWarning(Predicate<Warning> warning)
        {
            return _warnings.RemoveAll(warning);
        }

        public int ClearMuteExpiredUsers()
        {
            return _warnings.RemoveAll(warn => warn.MuteExpiries < DateTime.Now);
        }

        public IEnumerable<Warning> GetWarnings()
        {
            return _warnings;
        }
    }
}
