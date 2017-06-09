using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNETCF.Data
{
    public partial class DeviceFirstDataService : DisposableBase
	{
        private Dictionary<Type, CircularBuffer<object>> m_transmitBuffers = new Dictionary<Type, CircularBuffer<object>>();
        private AutoResetEvent m_txDataRead = new AutoResetEvent(false);

        public async Task<T> GetSingleAsync<T>(object identifier)
            where T : class, new()
        {
            return await m_settings.LocalStore.GetSingleAsync<T>(identifier);
        }

        public T GetSingle<T>(object identifier)
            where T : class, new()
        {
            return m_settings.LocalStore.GetSingle<T>(identifier);
        }

        public async Task<T[]> GetMultipleAsync<T>()
            where T : class, new()
        {
            return await m_settings.LocalStore.GetMultipleAsync<T>();
        }

        public T[] GetMultiple<T>()
            where T : class, new()
        {
            return m_settings.LocalStore.GetMultiple<T>();
        }

        public async Task<T> StoreAsync<T>(T item)
            where T : class
        {
            var result = await m_settings.LocalStore.StoreAsync(item);
            // TODO: was it an insert or update?
            QueueForTransmit(item);
            return result;
        }

        public T Store<T>(T item)
            where T : class
        {
            var result = m_settings.LocalStore.Store(item);
            // TODO: was it an insert or update?
            QueueForTransmit(item);
            return result;
        }

        private void QueueForTransmit(object item)
        {
            var t = item.GetType();

            lock (m_transmitBuffers)
            {
                if (!m_transmitBuffers.ContainsKey(t))
                {
                    m_transmitBuffers.Add(t, new CircularBuffer<object>(m_settings.DefaultSendBufferDepth));
                    // TODO: add high-water event watcher to force send, even if outside of a sync period
                    m_txDataRead.Set();
                }
            }
        }

        public async Task RemoveAsync<T>(object identifier)
        {
            var pi = TypeIdentifierRegistrations[typeof(T)];
            await m_settings.LocalStore.RemoveAsync<T>(pi, identifier);
        }

        public void Remove<T>(object identifier)
        {
            var pi = TypeIdentifierRegistrations[typeof(T)];
            m_settings.LocalStore.Remove<T>(pi, identifier);
        }

        public async Task RemoveAsync<T>(T item)
            where T : class
        {
            // extract the identifier
            var pi = TypeIdentifierRegistrations[item.GetType()];
            var identifier = pi.GetValue(item);
            await RemoveAsync<T>(identifier);
        }

        public void Remove<T>(T item)
            where T : class
        {
            // extract the identifier
            var pi = TypeIdentifierRegistrations[item.GetType()];
            var identifier = pi.GetValue(item);
            Remove<T>(identifier);
        }

        public void RemoveAll<T>()
            where T : class
        {
            m_settings.LocalStore.RemoveAll<T>();
        }

        public async Task RemoveAllAsync<T>()
            where T : class
        {
            await m_settings.LocalStore.RemoveAllAsync<T>();
        }

        public int Count<T>()
        {
            return m_settings.LocalStore.Count<T>();
        } 
    }
}
