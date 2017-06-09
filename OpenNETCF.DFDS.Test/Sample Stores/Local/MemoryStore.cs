using OpenNETCF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNETCF.DFDS.Test
{
    public class MemoryStore : IDfdsLocalStore
    {
        private Dictionary<Type, Dictionary<object, object>> m_store;
        private DeviceFirstDataService m_dfds;

        public MemoryStore()
        {
            m_store = new Dictionary<Type, Dictionary<object, object>>();
        }

        public DeviceFirstDataService Parent
        {
            get { return m_dfds; }
            set
            {
                Validate
                    .Begin()
                    .ParameterIsNotNull(value, "value")
                    .Check();

                m_dfds = value;
            }
        }

        public T GetSingle<T>(object identifier)
            where T : class, new()
        {
            var t = typeof(T);
            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    return null;
                }
                if (!m_store[t].ContainsKey(identifier))
                {
                    return null;
                }
                return m_store[t][identifier] as T;
            }
        }

        public T[] GetMultiple<T>()
            where T : class, new()
        {
            var t = typeof(T);

            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    return null;
                }

                return m_store[t].Values.Select(e => e as T).ToArray();
            }
        }

        public T[] StoreMultiple<T>(T[] items)
            where T : class
        {
            foreach (var item in items)
            {
                Store(item);
            }

            return items;
        }

        public T Store<T>(T item)
            where T : class
        {
            var t = item.GetType();

            Validate
                .Begin()
                .ParameterIsNotNull(item, "item")
                .IsTrue(Parent.TypeIdentifierRegistrations.ContainsKey(t))
                .Check();

            var identifierProperty = Parent.TypeIdentifierRegistrations[t];

            var identifierValue = identifierProperty.GetValue(item);

            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    m_store.Add(t, new Dictionary<object, object>());
                }

                if (m_store[t].ContainsKey(identifierValue))
                {
                    // this is an update
                    m_store[t][identifierValue] = item;
                }
                else
                {
                    // this is an insert

                    var ti = identifierValue.GetType();

                    // if the identifier is an integer, and is <= 0, we'll treat it as a "auto incrementing key" to be friendly
                    if (ti.Equals(typeof(int)))
                    {
                        var currentID = Convert.ToInt32(identifierValue);
                        if (currentID <= 0)
                        {
                            int nextID;
                            if (m_store[t].Count == 0)
                            {
                                nextID = 1;
                            }
                            else
                            {
                                var currentMaxID = m_store[t].Max(v => Convert.ToInt32(identifierProperty.GetValue(v.Value)));
                                nextID = currentMaxID + 1;
                            }
                            identifierProperty.SetValue(item, nextID);
                            identifierValue = nextID;
                        }
                    }

                    m_store[t].Add(identifierValue, item);
                }
            }

            return item;   
        }

        public void Remove<T>(PropertyInfo identifierProperty, object identifierValue)
        {
            var t = typeof(T);
            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    return;
                }
                if (!m_store[t].ContainsKey(identifierValue))
                {
                    return;
                }
                var existing = m_store[t][identifierValue];
                m_store[t].Remove(identifierValue);
            }
        }

        public void RemoveAll<T>()
            where T : class
        {
            var t = typeof(T);
            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    return;
                }
                m_store[t].Clear();
            }
        }

        public int Count<T>()
        {
            var t = typeof(T);
            lock (m_store)
            {
                if (!m_store.ContainsKey(t))
                {
                    return 0;
                }
                return m_store[t].Count;
            }
        }

        public async Task<T> GetSingleAsync<T>(object identifier)
            where T : class, new()
        {
            return await Task.Run(() => { return GetSingle<T>(identifier); });
        }

        public async Task<T[]> GetMultipleAsync<T>() where T : class, new()
        {
            return await Task.Run(() => { return GetMultiple<T>(); });
        }

        public async Task<T> StoreAsync<T>(T item)
            where T : class
        {
            return await Task.Run(() => { return Store(item); });
        }

        public async Task RemoveAsync<T>(PropertyInfo identifierProperty, object identifierValue)
        {
            await Task.Run(() => { Remove<T>(identifierProperty, identifierValue); });
        }

        public async Task RemoveAllAsync<T>()
            where T : class
        {
            await Task.Run(() => { RemoveAll<T>(); });
        }
    }
}
