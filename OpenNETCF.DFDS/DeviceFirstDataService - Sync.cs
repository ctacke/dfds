using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenNETCF.Data
{
    public partial class DeviceFirstDataService : DisposableBase
	{
        public event EventHandler<GenericEventArgs<Type>> SyncCompleted;

        private bool m_stop;
        private int m_syncTick;
        private Dictionary<Type, DateTime> m_pullTimestamps = new Dictionary<Type, DateTime>();
        private Dictionary<Type, DateTime> m_pushTimestamps = new Dictionary<Type, DateTime>();

        private void StartSync()
        {
            m_stop = false;
            Task.Run(async () => await SyncProc());
        }

        private void StopSync()
        {
            m_stop = true;
        }

        private async Task SyncProc()
        {
            if (m_stop)
            {
                Debug.WriteLine("DFDS: Stopping sync...");
                return;
            }
            Debug.WriteLine("DFDS: Sync");
            m_syncTick++;

            Type[] keys;

            lock (TypeIdentifierRegistrations)
            {
                keys = TypeIdentifierRegistrations.Keys.ToArray();
            }

            foreach (var syncType in keys)
            {
                if (m_syncTick % m_settings.DefaultSyncPeriodSeconds == 0)
                {
                    // sync all defaults
                    SyncType(syncType);
                    SyncCompleted.Fire(this, new GenericEventArgs<Type>(syncType)); 
                }
                else
                {
                    // TODO: look to sync anyone with an override
                }
            }

            await Task.Delay(1000);
            Task.Run(async () => await SyncProc());
        }

        private void SyncType(Type t)
        {
            SendDeletes(t);

            switch (m_settings.SyncBehavior)
            {
                case SyncBehavior.PullThenPush:
                    PullType(t);
                    PushType(t);
                    break;
                case SyncBehavior.PushThenPull:
                    PushType(t);
                    PullType(t);
                    break;
                case SyncBehavior.PushOnly:
                    PushType(t);
                    break;
                case SyncBehavior.PullOnly:
                    PullType(t);
                    break;
            }
        }

        private void SendDeletes(Type t)
        {
            Type[] typeList;

            // get a copy of the types - don't lock that buffer for too long
            lock (m_deleteBuffers)
            {
                typeList = m_deleteBuffers.Keys.ToArray();
            }

            if (!typeList.Contains(t))
            {
                // no pending deletes
                return;
            }

            while (m_deleteBuffers[t].Count > 0)
            {
                try
                {
                    // we Peek and Dequeue on uccess to keep the items in the original order rather than Dequeue and Enqueue on fail
                    var entity = m_deleteBuffers[t].Peek();
                    RemoteStore.Delete(entity);
                    m_deleteBuffers[t].Dequeue();
                }
                catch
                {
                    // leave it in the queue, but abandon syncing this type

                    // TODO: add in some form of notification that this happened

                    break;
                }
            }

        }

        private void PushType(Type t)
        {
            Type[] typeList;

            // TODO: add support for some idea of type priority (i.e. those that should go first)?

            // get a copy of the types - don't lock that buffer for too long
            lock (m_transmitBuffers)
            {
                typeList = m_transmitBuffers.Keys.ToArray();
            }

            if (!typeList.Contains(t))
            {
                // no pending items
                return;
            }

            while (m_transmitBuffers[t].Count > 0)
            {
                try
                {
                    // we Peek and Dequeue on uccess to keep the items in the original order rather than Dequeue and Enqueue on fail
                    var entity = m_transmitBuffers[t].Peek();

                    if (entity.IsInsert)
                    {
                        RemoteStore.Insert(entity.Entity);
                    }
                    else
                    {
                        RemoteStore.Update(entity.Entity);
                    }
                    m_transmitBuffers[t].Dequeue();
                }
                catch
                {
                    // leave it in the queue, but abandon syncing this type

                    // TODO: add in some form of notification that this happened

                    break;
                }
            }
        }

        /// <summary>
        /// Returns the age of the local cache for a given type
        /// </summary>
        /// <typeparam name="T">The entity type to query</typeparam>
        /// <returns>The last time the entity was synced, or null if a sync has never happened.</returns>
        public DateTime? GetEntityAge<T>()
        {
            var t = typeof(T);

            if (m_pullTimestamps.ContainsKey(t))
            {
                return m_pullTimestamps[t];
            }

            return null;
        }

        private void PullType(Type t)
        {
            var now = DateTime.Now;
            DateTime? lastPull = null;

            lock (m_pullTimestamps)
            {
                if (m_pullTimestamps.ContainsKey(t))
                {
                    lastPull = m_pullTimestamps[t];
                }
            }

            var items = RemoteStore.GetMultiple(t, lastPull);

            // append to local store
            LocalStore.StoreMultiple(items);

            lock (m_pullTimestamps)
            {
                if (m_pullTimestamps.ContainsKey(t))
                {
                    m_pullTimestamps[t] = now;
                }
                else
                {
                    m_pullTimestamps.Add(t, now);
                }
            }
        }
    }
}
