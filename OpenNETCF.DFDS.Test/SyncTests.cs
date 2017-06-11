using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Threading;
using OpenNETCF.Test;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;

namespace OpenNETCF.DFDS.Test
{
    [TestClass]
    public class SyncTests
    {
        [TestMethod]
        public void SyncRemoteInsertTest()
        {
            var serverUri = "http://localhost:8081";

            using (var server = new SimpleServer(serverUri,
                getMethod: SimpleServerMethods.InsertAfterGetMultiplePeople))
            {
                server.Start();

                DfdsJsonSerializer serializer = new DfdsJsonSerializer();

                var settings = new DfdsServiceSettings();
                settings.DefaultSyncPeriodSeconds = 5;

                settings.LocalStore = new MemoryStore();
                settings.RemoteStore = new RestBasedStore(serverUri, serializer);

                bool syncComplete = false;

                var svc = new DeviceFirstDataService(settings);
                svc.SyncCompleted += delegate { syncComplete = true; };

                svc.Register<Person>("PersonID");
                // local store will be empty
                Assert.AreEqual(0, settings.LocalStore.Count<Person>());

                // when sync happens, we'll have 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                // initial population of the store is 3
                Assert.AreEqual(3, settings.LocalStore.Count<Person>());

                // the query above triggers an insert on the remote, local still will be 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                Assert.AreEqual(4, settings.LocalStore.Count<Person>());
            }
        }

        [TestMethod]
        public void SyncLocalInsert()
        {
            var serverUri = "http://localhost:8081";

            using (var server = new SimpleServer(serverUri,
                getMethod: SimpleServerMethods.GetMultiplePeople,
                postMethod: SimpleServerMethods.PostPeople))
            {
                server.Start();

                DfdsJsonSerializer serializer = new DfdsJsonSerializer();

                var settings = new DfdsServiceSettings();
                settings.DefaultSyncPeriodSeconds = 5;

                settings.LocalStore = new MemoryStore();
                settings.RemoteStore = new RestBasedStore(serverUri, serializer);

                bool syncComplete = false;

                var svc = new DeviceFirstDataService(settings);
                svc.SyncCompleted += delegate { syncComplete = true; };

                svc.Register<Person>("PersonID");
                // local store will be empty
                Assert.AreEqual(0, settings.LocalStore.Count<Person>());

                // when sync happens, we'll have 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                svc.Store(new Person("Lola L-O-L-A Lola"));

                // initial population of the store is 3
                Assert.AreEqual(4, settings.LocalStore.Count<Person>());

                // the query above triggers an insert on the remote, local still will be 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                Assert.AreEqual(4, settings.LocalStore.Count<Person>());
            }
        }

        [TestMethod]
        public void SyncLocalUpdate()
        {
            var serverUri = "http://localhost:8081";

            using (var server = new SimpleServer(serverUri,
                getMethod: SimpleServerMethods.GetMultiplePeople,
                postMethod: SimpleServerMethods.PostPeople,
                putMethod: SimpleServerMethods.PutPerson))
            {
                server.Start();

                DfdsJsonSerializer serializer = new DfdsJsonSerializer();

                var settings = new DfdsServiceSettings();
                settings.DefaultSyncPeriodSeconds = 5;

                settings.LocalStore = new MemoryStore();
                settings.RemoteStore = new RestBasedStore(serverUri, serializer);

                bool syncComplete = false;

                var svc = new DeviceFirstDataService(settings);
                svc.SyncCompleted += delegate { syncComplete = true; };

                svc.Register<Person>("PersonID");
                // local store will be empty
                Assert.AreEqual(0, settings.LocalStore.Count<Person>());

                // when sync happens, we'll have 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                var person = svc.GetSingle<Person>(1);

                // make a change
                person.Name = "Mr. Bojangles";
                svc.Store(person);

                // wait for the change to sync
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                // does the remote match?            
                var check = SimpleServerMethods.People.First(p => p.PersonID == 1);

                Assert.AreEqual(person.Name, check.Name);

                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }

                Assert.AreEqual(3, settings.LocalStore.Count<Person>());
            }
        }

        [TestMethod]
        public void SyncRemoteUpdate()
        {
            var serverUri = "http://localhost:8081";

            using (var server = new SimpleServer(serverUri,
                getMethod: SimpleServerMethods.GetMultiplePeople))
            {
                server.Start();

                DfdsJsonSerializer serializer = new DfdsJsonSerializer();

                var settings = new DfdsServiceSettings();
                settings.DefaultSyncPeriodSeconds = 5;

                settings.LocalStore = new MemoryStore();
                settings.RemoteStore = new RestBasedStore(serverUri, serializer);

                bool syncComplete = false;

                var svc = new DeviceFirstDataService(settings);
                svc.SyncCompleted += delegate { syncComplete = true; };

                svc.Register<Person>("PersonID");
                // local store will be empty
                Assert.AreEqual(0, settings.LocalStore.Count<Person>());

                // when sync happens, we'll have 3
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                // get a remote store person and directly modify it
                var person = SimpleServerMethods.People.First(p => p.PersonID == 1);
                person.Name = "Billy Jean";
                person.LastChanged = DateTime.Now;

                // wait for the change to sync
                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }
                syncComplete = false;

                // does the local match?            
                var check = svc.GetSingle<Person>(1);

                Assert.AreEqual(person.Name, check.Name);

                while (!syncComplete)
                {
                    Thread.Sleep(500);
                }

                Assert.AreEqual(3, settings.LocalStore.Count<Person>());
            }
        }
    }
}
