using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNETCF.Data
{
    public interface IDfdsRemoteStore
    {
        T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue);
    }

    public interface IDfdsLocalStore
    {
        T GetSingle<T>(object identifier) where T : class, new();
        Task<T> GetSingleAsync<T>(object identifier) where T : class, new();
        T[] GetMultiple<T>() where T : class, new();
        Task<T[]> GetMultipleAsync<T>() where T : class, new();
        T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue);
        Task<T> StoreAsync<T>(T item, PropertyInfo identifierProperty, object identifierValue);
        void Remove<T>(PropertyInfo identifierProperty, object identifierValue);
        Task RemoveAsync<T>(PropertyInfo identifierProperty, object identifierValue);
        void RemoveAll<T>() where T : class;
        Task RemoveAllAsync<T>() where T : class;
        int Count<T>();
    }
}
