using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    interface IStorageProvider
    {
        Task AddWarningAsync(Warning warning);
        Task<int> RemoveWarningAsync(Expression<Func<Warning, bool>> expression);
        Task<int> ClearExpiredWarningsAsync();
        IEnumerable<Warning> GetWarnings(Expression<Func<Warning, bool>> expression);

        Task AddBanWordAsync(BanWord word);
        Task<int> RemoveBanWordAsync(Expression<Func<BanWord, bool>> expression);
        IEnumerable<BanWord> GetBanWords(Expression<Func<BanWord, bool>> expression);

    }
}
