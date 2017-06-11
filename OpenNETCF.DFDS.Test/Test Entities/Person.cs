using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;
using System.Runtime.Serialization;

namespace OpenNETCF.DFDS.Test
{
    public class Person
    {
        public Person()
        {
            LastChanged = DateTime.Now;
        }

        public Person(string name)
        {
            Name = name;
        }

        public int PersonID { get; set; }
        public string Name { get; set; }

        [DoNotSerialize]
        public DateTime LastChanged { get; set; }
    }
}
