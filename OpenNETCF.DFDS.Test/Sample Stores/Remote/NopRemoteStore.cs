using OpenNETCF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OpenNETCF.DFDS.Test
{
    public class NopRemoteStore : IDfdsRemoteStore
    {
        public DeviceFirstDataService Parent { get; set; }

        public void Delete<T>(T item)
        {
        }

        public object[] GetMultiple(Type entityType, DateTime? lastRequest)
        {
            return null;
        }

        public void Insert<T>(T item)
        {
        }

        public void Update<T>(T item)
        {
        }
    }
}
