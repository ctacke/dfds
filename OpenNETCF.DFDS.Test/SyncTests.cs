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
    }
}
