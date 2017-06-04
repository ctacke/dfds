using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;

namespace OpenNETCF.DFDS.Test
{
    [TestClass]
    public class StoreParameterTests
    {
        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void NullSettingsTest()
        {
            var svc = new DeviceFirstDataService(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void NullLocalStoreTest()
        {
            var settings = new DfdsServiceSettings();
            settings.RemoteStore = new NopRemoteStore();
            var svc = new DeviceFirstDataService(settings);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void NullRemoteStoreTest()
        {
            var settings = new DfdsServiceSettings();
            settings.LocalStore = new MemoryStore();
            var svc = new DeviceFirstDataService(settings);
        }
    }
}
