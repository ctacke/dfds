using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Threading;
using OpenNETCF.Test;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.IO;

namespace OpenNETCF.DFDS.Test
{
    public static class SimpleServerMethods
    {
        private static List<Person> people = new List<Person>();

        static SimpleServerMethods()
        {
            AddPerson(new Person("John Doe"));
            AddPerson(new Person("Jane Doe"));
            AddPerson(new Person("Marie Smith"));
        }

        private static void AddPerson(Person p)
        {
            p.PersonID = people.Count + 1;
            p.Added = DateTime.Now;
            people.Add(p);
        }

        internal static string InsertAfterGetMultiplePeople(HttpListenerRequest request)
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

                    AddPerson(new Person("Johnny Come Lately"));

                    return result;
                }
            }
            else
            {
                return null;
            }
        }

        internal static string GetMultiplePeople(HttpListenerRequest request)
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
                var result = JsonConvert.SerializeObject(people);
                return result;
            }
            else
            {
                return null;
            }
        }

        internal static string PostPeople(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                var data = reader.ReadToEnd();
                var p = JsonConvert.DeserializeObject<Person>(data);
                AddPerson(p);                
            }

                return string.Empty;
        }
    }
}
