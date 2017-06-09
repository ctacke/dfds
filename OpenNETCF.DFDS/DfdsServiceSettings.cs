using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.Data
{
    public enum SyncBehavior
    {
        PullOnly,
        PushOnly,
        PullThenPush,
        PushThenPull
    }

    public class DfdsServiceSettings
    {
        private int m_syncPeriod;
        private int m_defaultBufferDepth;

        public DfdsServiceSettings()
        {
            DefaultSyncPeriodSeconds = 60;
            DefaultSendBufferDepth = 20;

            SyncBehavior = SyncBehavior.PullThenPush;
        }

        public IDfdsLocalStore LocalStore { get; set; }
        public IDfdsRemoteStore RemoteStore { get; set; }
        public SyncBehavior SyncBehavior { get; set; }

        public int DefaultSyncPeriodSeconds
        {
            get { return m_syncPeriod; }
            set
            {
                Validate
                    .Begin()
                    .IsGreaterThanOrEqualTo(value, -1)
                    .Check();

                m_syncPeriod = value;
            }
        }

        public int DefaultSendBufferDepth
        {
            get { return m_defaultBufferDepth; }
            set
            {
                Validate
                    .Begin()
                    .IsGreaterThanOrEqualTo(value, 1)
                    .Check();

                m_defaultBufferDepth = value;
            }
        }
    }
}
