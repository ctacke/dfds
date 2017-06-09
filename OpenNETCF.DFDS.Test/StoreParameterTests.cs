using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Threading;

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

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void InvalidRegistrationTest1()
        {
            var settings = new DfdsServiceSettings();
            settings.LocalStore = new MemoryStore();
            settings.RemoteStore = new NopRemoteStore();
            var svc = new DeviceFirstDataService(settings);
            svc.Register<Person>(null);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void InvalidRegistrationTest2()
        {
            var settings = new DfdsServiceSettings();
            settings.LocalStore = new MemoryStore();
            settings.RemoteStore = new NopRemoteStore();
            var svc = new DeviceFirstDataService(settings);
            svc.Register<Person>(string.Empty);
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void InvalidRegistrationTest3()
        {
            var settings = new DfdsServiceSettings();
            settings.LocalStore = new MemoryStore();
            settings.RemoteStore = new NopRemoteStore();
            var svc = new DeviceFirstDataService(settings);
            svc.Register<Person>("    ");
        }
    }
}
