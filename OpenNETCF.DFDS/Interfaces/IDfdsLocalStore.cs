using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.Data
{
    public interface IDfdsRemoteStore
    {
        T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue);
    }

    public interface IDfdsLocalStore
    {
        T Get<T>(object identifier) where T : class, new();
        T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue);
        void Remove<T>(PropertyInfo identifierProperty, object identifierValue);
        int Count<T>();
    }
}
