using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNETCF.Data
{
    public static class SyncPeriod
    {
        /// <summary>
        /// Sync only when explicitly called for
        /// </summary>
        public const int OnDemand = 0;
        /// <summary>
        /// Use the service-wide default sync period
        /// </summary>
        public const int UseServiceDefault = -1;
    }
}
