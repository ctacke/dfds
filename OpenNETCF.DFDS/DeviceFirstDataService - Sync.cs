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
        private Dictionary<Type, CircularBuffer<object>> m_pushBuffers;
        private int m_syncTick;

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

        private Dictionary<Type, DateTime> m_pullTimestamps = new Dictionary<Type, DateTime>();
        private Dictionary<Type, DateTime> m_pushTimestamps = new Dictionary<Type, DateTime>();

        private void SyncType(Type t)
        {
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

        private void PushType(Type t)
        {
            // TODO:
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
