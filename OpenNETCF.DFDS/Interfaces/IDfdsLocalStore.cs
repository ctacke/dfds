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
        DeviceFirstDataService Parent { get; set; }

        /// <summary>
        /// Requests an array of Entities from the store to be *appended* to the DFDS local store
        /// </summary>
        /// <param name="lastRequest">The last time DFDS made this request (to assist the Store implementation in caching)</param>
        /// <param name="entityType">The type of the Entities to be retrieved</param>
        /// <returns>An array of entities or null if the query returns no result</returns>
        object[] GetMultiple(Type entityType, DateTime? lastRequest);

        void Insert<T>(T item);
        void Update<T>(T item);
    }

    public interface IDfdsLocalStore
    {
        DeviceFirstDataService Parent { get; set; }

        T GetSingle<T>(object identifier) where T : class, new();
        Task<T> GetSingleAsync<T>(object identifier) where T : class, new();
        T[] GetMultiple<T>() where T : class, new();
        Task<T[]> GetMultipleAsync<T>() where T : class, new();
        /// <summary>
        /// Stores an instance of and entity (either insert or update)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns><b>true</b> if the operation inserted a new instance, otherwise <b>false</b></returns>
        bool Store<T>(T item) where T : class;
        /// <summary>
        /// Stores an instance of and entity (either insert or update)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns><b>true</b> if the operation inserted a new instance, otherwise <b>false</b></returns>
        Task<bool> StoreAsync<T>(T item) where T : class;
        T[] StoreMultiple<T>(T[] items) where T : class;
        void Remove<T>(PropertyInfo identifierProperty, object identifierValue);
        Task RemoveAsync<T>(PropertyInfo identifierProperty, object identifierValue);
        void RemoveAll<T>() where T : class;
        Task RemoveAllAsync<T>() where T : class;
        int Count<T>();
    }

    //public abstract class LocalStoreBase : IDfdsLocalStore
    //{
    //}
}
