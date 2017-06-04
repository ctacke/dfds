using OpenNETCF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace OpenNETCF.DFDS.Test
{
    public class RestBasedStore : IDfdsRemoteStore
    {
        public T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue)
        {
            throw new NotImplementedException();
        }
    }
}
