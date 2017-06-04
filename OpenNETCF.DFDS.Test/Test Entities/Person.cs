using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenNETCF.Data;

namespace OpenNETCF.DFDS.Test
{
    public class Person
    {
        public Person()
        {
        }

        public Person(string name)
        {
            Name = name;
        }

        public int PersonID { get; set; }
        public string Name { get; set; }
    }
}
