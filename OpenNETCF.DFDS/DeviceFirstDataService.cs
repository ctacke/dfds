using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.Data
{
    public class DeviceFirstDataService
	{
        private string RootUri { get; set; }

        private DfdsServiceSettings m_settings;

        private Dictionary<Type, PropertyInfo> m_identifierLookup;

        public DeviceFirstDataService(DfdsServiceSettings settings)
        {
            Validate
                .Begin()
                .ParameterIsNotNull(settings, "settings")
                .Check()
                .IsNotNull(settings.LocalStore, "LocalStore must not be null")
                .IsNotNull(settings.RemoteStore, "RemoteStore must not be null")
                .Check();

            m_identifierLookup = new Dictionary<Type, PropertyInfo>();
            m_settings = settings;
        }

        public void Register<T>(string idProperty)
        {
            var t = typeof(T);
            var prop = t.GetRuntimeProperty(idProperty);

            if (prop == null)
            {
                throw new Exception(string.Format("Property '{0}' not found on type '{1}'", idProperty, t.Name));
            }

            if (!m_identifierLookup.ContainsKey(t))
            {
                m_identifierLookup.Add(t, prop);
            }
        }

        public T Get<T>(object identifier)
            where T : class, new()
        {
            return m_settings.LocalStore.Get<T>(identifier);
        }

        public T Store<T>(T item)
            where T : class
        {
            // extract the identifier
            var pi = m_identifierLookup[item.GetType()];
            var identifier = pi.GetValue(item);
            return m_settings.LocalStore.Store(item, pi, identifier);
        }

        public void Remove<T>(object identifier)
        {
            var pi = m_identifierLookup[typeof(T)];
            m_settings.LocalStore.Remove<T>(pi, identifier);
        }

        public void Remove<T>(T item)
            where T : class
        {
            // extract the identifier
            var pi = m_identifierLookup[item.GetType()];
            var identifier = pi.GetValue(item);
            Remove<T>(identifier);
        }

        public int Count<T>()
        {
            return m_settings.LocalStore.Count<T>();
        } 
    }
}
