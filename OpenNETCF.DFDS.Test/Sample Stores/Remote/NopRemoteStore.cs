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
        public T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue)
        {
            return default(T);
        }
    }
}
