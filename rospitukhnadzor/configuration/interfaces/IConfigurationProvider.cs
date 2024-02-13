using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosPitukhNadzor
{
    internal interface IConfigurationProvider
    {
        public Configuration GetConfiguration();
        public bool SaveConfiguration();
    }
}
