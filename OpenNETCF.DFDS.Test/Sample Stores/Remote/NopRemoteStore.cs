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
        public object[] GetMultiple(Type entityType, DateTime? lastRequest)
        {
            return null;
        }

        public T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue)
        {
            return default(T);
        }
    }
}
