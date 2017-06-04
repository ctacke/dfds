﻿using OpenNETCF.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.DFDS.Test
{
    public class MemoryStore : IDfdsLocalStore
    {
        private Dictionary<Type, Dictionary<object, object>> m_store;

        public MemoryStore()
        {
            m_store = new Dictionary<Type, Dictionary<object, object>>();
        }

        public T Get<T>(object identifier)
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

        public T Store<T>(T item, PropertyInfo identifierProperty, object identifierValue)
        {
            var t = item.GetType();
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
    }
}