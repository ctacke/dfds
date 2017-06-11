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
        internal static List<Person> People { get; private set; } = new List<Person>();

        static SimpleServerMethods()
        {
            AddPerson(new Person("John Doe"));
            AddPerson(new Person("Jane Doe"));
            AddPerson(new Person("Marie Smith"));
        }

        private static void AddPerson(Person p)
        {
            p.PersonID = People.Count + 1;
            p.LastChanged = DateTime.Now;
            People.Add(p);
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
                    return JsonConvert.SerializeObject(People.Where(p => p.LastChanged >= since.Value));
                }
                else
                {
                    var result = JsonConvert.SerializeObject(People);

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

            if (request.Url.Segments.Length == 0) return null;

            switch (request.Url.Segments.Length)
            {
                case 2:
                    if (string.Compare(request.Url.Segments[1], "person", true) == 0)
                    {
                        var result = JsonConvert.SerializeObject(People);
                        return result;
                    }
                    break;
                case 3:
                    if (string.Compare(request.Url.Segments[1], "person", true) == 0)
                    {
                        var id = Convert.ToInt32(request.Url.Segments[2]);
                        var person = People.FirstOrDefault(p => p.PersonID == id);
                        if (person == null) return null;
                        var result = JsonConvert.SerializeObject(person);
                        return result;
                    }
                    break;

            }
            return null;
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

        internal static string PutPerson(HttpListenerRequest request)
        {
            using (var reader = new StreamReader(request.InputStream))
            {
                var data = reader.ReadToEnd();
                var person = JsonConvert.DeserializeObject<Person>(data);

                var existing = People.FirstOrDefault(p => person.PersonID == p.PersonID);

                if (existing != null)
                {
                    existing.Name = person.Name;
                    existing.LastChanged = DateTime.Now;
                }
            }

            return string.Empty;
        }
    }
}
