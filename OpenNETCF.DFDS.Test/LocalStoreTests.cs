using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Threading.Tasks;

namespace OpenNETCF.DFDS.Test
{
    [TestClass]
    public class LocalStoreTests
    {
        [TestMethod]
        public void BasicSyncOperationsTest()
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

            // Store
            var p1 = svc.Store(person1);
            Assert.IsNotNull(p1);
            Assert.AreEqual(1, p1.PersonID);
            var p2 = svc.Store(person2);
            Assert.IsNotNull(p2);
            Assert.AreEqual(2, p2.PersonID);
            var p3 = svc.Store(person3);
            Assert.IsNotNull(p3);
            Assert.AreEqual(3, p3.PersonID);

            // retrieve all
            var all = svc.GetMultiple<Person>();
            Assert.AreEqual(3, all.Length);

            // retrieve by ID
            var pg1 = svc.GetSingle<Person>(1);
            Assert.AreEqual(p1, pg1);
            var pg2 = svc.GetSingle<Person>(2);
            Assert.AreEqual(p2, pg2);
            var pg3 = svc.GetSingle<Person>(3);
            Assert.AreEqual(p3, pg3);

            // update
            var newName = "Johnathan Doe";
            person1.Name = newName;
            svc.Store(person1);
            var up1 = svc.GetSingle<Person>(person1.PersonID);
            Assert.AreEqual(up1.Name, newName);

            // delete by id
            Assert.AreEqual(3, svc.Count<Person>());
            svc.Remove(person2);
            var np2 = svc.GetSingle<Person>(2);
            Assert.AreEqual(2, svc.Count<Person>());
            Assert.IsNull(np2);
            svc.Remove<Person>(3);
            var np3 = svc.GetSingle<Person>(3);
            Assert.AreEqual(1, svc.Count<Person>());
            Assert.IsNull(np3);

            // delete all
            svc.Store(person1);
            svc.Store(person2);
            svc.Store(person3);
            Assert.AreEqual(3, svc.Count<Person>());
            svc.RemoveAll<Person>();
            Assert.AreEqual(0, svc.Count<Person>());
        }

        [TestMethod]
        public async Task BasicAsyncOperationsTest()
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

            // Store
            var p1 = await svc.StoreAsync(person1);
            Assert.IsNotNull(p1);
            Assert.AreEqual(1, p1.PersonID);
            var p2 = await svc.StoreAsync(person2);
            Assert.IsNotNull(p2);
            Assert.AreEqual(2, p2.PersonID);
            var p3 = await svc.StoreAsync(person3);
            Assert.IsNotNull(p3);
            Assert.AreEqual(3, p3.PersonID);

            // retrieve all
            var all = await svc.GetMultipleAsync<Person>();
            Assert.AreEqual(3, all.Length);

            // retrieve by ID
            var pg1 = await svc.GetSingleAsync<Person>(1);
            Assert.AreEqual(p1, pg1);
            var pg2 = await svc.GetSingleAsync<Person>(2);
            Assert.AreEqual(p2, pg2);
            var pg3 = await svc.GetSingleAsync<Person>(3);
            Assert.AreEqual(p3, pg3);

            // update
            var newName = "Johnathan Doe";
            person1.Name = newName;
            await svc.StoreAsync(person1);
            var up1 = await svc.GetSingleAsync<Person>(person1.PersonID);
            Assert.AreEqual(up1.Name, newName);

            // delete by id
            Assert.AreEqual(3, svc.Count<Person>());
            await svc.RemoveAsync(person2);
            var np2 = await svc.GetSingleAsync<Person>(2);
            Assert.AreEqual(2, svc.Count<Person>());
            Assert.IsNull(np2);
            svc.Remove<Person>(3);
            var np3 = await svc.GetSingleAsync<Person>(3);
            Assert.AreEqual(1, svc.Count<Person>());
            Assert.IsNull(np3);

            // delete all
            await svc.StoreAsync(person1);
            await svc.StoreAsync(person2);
            await svc.StoreAsync(person3);
            Assert.AreEqual(3, svc.Count<Person>());
            await svc.RemoveAllAsync<Person>();
            Assert.AreEqual(0, svc.Count<Person>());

        }
    }
}
