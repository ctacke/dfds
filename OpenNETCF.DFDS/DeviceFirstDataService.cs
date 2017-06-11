using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OpenNETCF.Data
{
    public partial class DeviceFirstDataService : DisposableBase
	{
        private string RootUri { get; set; }

        private DfdsServiceSettings m_settings;

        public Dictionary<Type, PropertyInfo> TypeIdentifierRegistrations { get; private set; }

        public DeviceFirstDataService(DfdsServiceSettings settings)
        {
            Validate
                .Begin()
                .ParameterIsNotNull(settings, "settings")
                .Check()
                .IsNotNull(settings.LocalStore, "LocalStore must not be null")
                .IsNotNull(settings.RemoteStore, "RemoteStore must not be null")
                .Check();

            settings.LocalStore.Parent = this;
            settings.RemoteStore.Parent = this;

            TypeIdentifierRegistrations = new Dictionary<Type, PropertyInfo>();
            m_settings = settings;

            StartSync();
        }

        // This is exposed for unit testing
        internal IDfdsRemoteStore RemoteStore
        {
            get { return m_settings.RemoteStore; }
        }

        internal IDfdsLocalStore LocalStore
        {
            get { return m_settings.LocalStore; }
        }
    }
}
