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
        public void Register<T>(string idProperty, int syncPeriodSeconds = SyncPeriod.UseServiceDefault)
        {
            Validate
                .Begin()
                .ParameterIsNotNullOrWhitespace(idProperty, "idProperty")
                .Check();

            var t = typeof(T);
            var prop = t.GetRuntimeProperty(idProperty);

            if (prop == null)
            {
                throw new Exception(string.Format("Property '{0}' not found on type '{1}'", idProperty, t.Name));
            }

            if (!TypeIdentifierRegistrations.ContainsKey(t))
            {
                TypeIdentifierRegistrations.Add(t, prop);
            }
        }
    }
}
