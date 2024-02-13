using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    interface IWarningsStorage
    {
        void AddWarning(Warning warning);
        int RemoveWarning(Predicate<Warning> warning);
        int ClearMuteExpiredUsers();
        IEnumerable<Warning> GetWarnings();
    }
}
