using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;

namespace OpenNETCF.DFDS.Test
{
    [TestClass]
    public class LocalStoreTests
    {
        [TestMethod]
        public void BasicOperationsTest()
        {
            var settings = new DfdsServiceSettings()
            {
                LocalStore = new MemoryStore(),
                RemoteStore = new NopRemoteStore()
            };

            var svc = new DeviceFirstDataService(settings);
            svc.Register<Person>("PersonID");

            var person1 = new Person("John Doe");
            var person2 = new Person("Jane Doe");
            var person3 = new Person("Marie Smith");

            var p1 = svc.Store(person1);
            Assert.IsNotNull(p1);
            Assert.AreEqual(1, p1.PersonID);
            var p2 = svc.Store(person2);
            Assert.IsNotNull(p2);
            Assert.AreEqual(2, p2.PersonID);
            var p3 = svc.Store(person3);
            Assert.IsNotNull(p3);
            Assert.AreEqual(3, p3.PersonID);

            var pg1 = svc.Get<Person>(1);
            Assert.AreEqual(p1, pg1);
            var pg2 = svc.Get<Person>(2);
            Assert.AreEqual(p2, pg2);
            var pg3 = svc.Get<Person>(3);
            Assert.AreEqual(p3, pg3);

            var newName = "Johnathan Doe";
            person1.Name = newName;
            svc.Store(person1);
            var up1 = svc.Get<Person>(person1.PersonID);
            Assert.AreEqual(up1.Name, newName);

            Assert.AreEqual(3, svc.Count<Person>());
            svc.Remove(person2);
            var np2 = svc.Get<Person>(2);
            Assert.AreEqual(2, svc.Count<Person>());
            Assert.IsNull(np2);
            svc.Remove<Person>(3);
            var np3 = svc.Get<Person>(3);
            Assert.AreEqual(1, svc.Count<Person>());
            Assert.IsNull(np3);
        }
    }
}
