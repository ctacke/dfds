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

        private class TransmitObject
        {
            public TransmitObject(object entity, bool isInsert)
            {
                Entity = entity;
                IsInsert = IsInsert;
            }

            public object Entity { get; set; }
            public bool IsInsert { get; set; }
        }

        private Dictionary<Type, CircularBuffer<TransmitObject>> m_transmitBuffers = new Dictionary<Type, CircularBuffer<TransmitObject>>();
        private Dictionary<Type, CircularBuffer<object>> m_deleteBuffers = new Dictionary<Type, CircularBuffer<object>>();

        private AutoResetEvent m_txDataRead = new AutoResetEvent(false);

        public object GetEntityIdentifier(object entity)
        {
            Validate
                .Begin()
                .ParameterIsNotNull(entity)
                .Check();

            var t = entity.GetType();

            lock (TypeIdentifierRegistrations)
            {
                var identifierProperty = TypeIdentifierRegistrations[t];
                return identifierProperty.GetValue(entity);
            }
        }

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
            Validate
                .Begin()
                .ParameterIsNotNull(item, "item")
                .Check();

            var isInsert = await m_settings.LocalStore.StoreAsync(item);
            QueueForTransmit(item, isInsert);
            return item;
        }

        public T Store<T>(T item)
            where T : class
        {
            Validate
                .Begin()
                .ParameterIsNotNull(item, "item")
                .Check();

            var isInsert = m_settings.LocalStore.Store(item);
            QueueForTransmit(item, isInsert);
            return item;
        }

        private void QueueForDelete(object item)
        {
            var t = item.GetType();

            lock (m_deleteBuffers)
            {
                if (!m_transmitBuffers.ContainsKey(t))
                {

                    // TODO: account for non-default buffer depth
                    m_deleteBuffers.Add(t, new CircularBuffer<object>(m_settings.DefaultSendBufferDepth));
                    // TODO: add high-water event watcher to force send, even if outside of a sync period
                }

                m_deleteBuffers[t].Enqueue(item);
                m_txDataRead.Set();
            }
        }

        private void QueueForTransmit(object item, bool isInsert)
        {
            var t = item.GetType();

            lock (m_transmitBuffers)
            {
                var entity = new TransmitObject(item, isInsert);

                if (!m_transmitBuffers.ContainsKey(t))
                {

                    // TODO: account for non-default buffer depth
                    m_transmitBuffers.Add(t, new CircularBuffer<TransmitObject>(m_settings.DefaultSendBufferDepth));
                    // TODO: add high-water event watcher to force send, even if outside of a sync period
                }

                m_transmitBuffers[t].Enqueue(entity);
                m_txDataRead.Set();
            }
        }

        public async Task RemoveAsync<T>(object identifier)
        {
            Validate
                .Begin()
                .ParameterIsNotNull(identifier, "identifier")
                .Check();

            var pi = TypeIdentifierRegistrations[typeof(T)];
            await m_settings.LocalStore.RemoveAsync<T>(pi, identifier);
        }

        public void Remove<T>(object identifier)
            where T : class, new()
        {
            Validate
                .Begin()
                .ParameterIsNotNull(identifier, "identifier")
                .Check();

            var item = LocalStore.GetSingle<T>(identifier);
            var pi = TypeIdentifierRegistrations[typeof(T)];
            m_settings.LocalStore.Remove<T>(pi, identifier);
            QueueForDelete(item);
        }

        public async Task RemoveAsync<T>(T item)
            where T : class
        {
            Validate
                .Begin()
                .ParameterIsNotNull(item, "item")
                .Check();

            // extract the identifier
            var pi = TypeIdentifierRegistrations[item.GetType()];
            var identifier = pi.GetValue(item);
            await RemoveAsync<T>(identifier);
            QueueForDelete(item);
        }

        public void Remove<T>(T item)
            where T : class, new()
        {
            Validate
                .Begin()
                .ParameterIsNotNull(item, "item")
                .Check();

            // extract the identifier
            var pi = TypeIdentifierRegistrations[item.GetType()];
            var identifier = pi.GetValue(item);
            Remove<T>(identifier);
            QueueForDelete(item);
        }

        public void RemoveAll<T>()
            where T : class
        {
            m_settings.LocalStore.RemoveAll<T>();
            // TODO: remote sync
        }

        public async Task RemoveAllAsync<T>()
            where T : class
        {
            await m_settings.LocalStore.RemoveAllAsync<T>();
            // TODO: remote sync
        }

        public int Count<T>()
        {
            return m_settings.LocalStore.Count<T>();
        } 
    }
}
