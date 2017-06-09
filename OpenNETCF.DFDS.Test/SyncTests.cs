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
        private List<Person> people = new List<Person>();

        public SyncTests()
        {
//            var people = new List<Person>();
            people.Add(new Person("John Doe"));
            people.Add(new Person("Jane Doe"));
            people.Add(new Person("Marie Smith"));
        }

        private string GetMultiplePeople(HttpListenerRequest request)
        {
            // when was the last call
            DateTime? since = null;
            var sinceText = request.QueryString["since"];
            if (!string.IsNullOrWhiteSpace(sinceText))
            {
                since = DateTime.Parse(sinceText);
            }

            var url = request.Url.AbsolutePath;
            if (string.Compare(url, "/person", true) == 0)
            {
                if (since.HasValue)
                {
                    return JsonConvert.SerializeObject(people.Where(p => p.Added >= since.Value));
                }
                else
                {
                    var result = JsonConvert.SerializeObject(people);

                    people.Add(new Person("Johnny Come Lately") { Added = DateTime.Now });

                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ValidationException))]
        public void SyncRemoteInsertTest()
        {
            var serverUri = "http://localhost:8081";

            var server = new SimpleServer(serverUri,
                getMethod: GetMultiplePeople);
            server.Start();

            var serializer = new DfdsJsonSerializer();

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
}
