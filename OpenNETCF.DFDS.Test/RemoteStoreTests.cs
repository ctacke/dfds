using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Threading.Tasks;
using OpenNETCF.Test;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace OpenNETCF.DFDS.Test
{
    [TestClass]
    public class RemoteStoreTests
    {
        private string GetMultipleItems(HttpListenerRequest request)
        {
            var people = new List<Person>();
            people.Add(new Person("John Doe"));
            people.Add(new Person("Jane Doe"));
            people.Add(new Person("Marie Smith"));

            var url = request.RawUrl;
            if (string.Compare(url, "/person", true) == 0)
            {
                return JsonConvert.SerializeObject(people);
            }
            else
            {
                return null;
            }
        }

        [TestMethod]
        public void TestGetMultiple()
        {
            var serverUri = "http://localhost:8081";

            var server = new SimpleServer(serverUri,
                getMethod: GetMultipleItems);
            server.Start();

            var serializer = new DfdsJsonSerializer();

            var settings = new DfdsServiceSettings()
            {
                LocalStore = new MemoryStore(),
                RemoteStore = new RestBasedStore(serverUri, serializer)
            };

            var svc = new DeviceFirstDataService(settings);

            // this is an internal call test - client will never actually use it
            var people = svc.RemoteStore.GetMultiple(typeof(Person), null);


        }
    }
}
