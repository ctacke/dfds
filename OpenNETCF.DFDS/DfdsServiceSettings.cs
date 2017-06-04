using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenNETCF.Data
{
    public class DfdsServiceSettings
    {
        public DfdsServiceSettings()
        {
        }

        public IDfdsLocalStore LocalStore { get; set; }
        public IDfdsRemoteStore RemoteStore { get; set; }
    }
}
