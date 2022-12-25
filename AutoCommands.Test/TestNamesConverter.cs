using LinkCommands.Models;
using LinkCommands.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestNamesConverter
    {
        [TestMethod]
        public void TestGetShortName()
        {
            var namesConverter = new NamesConverter();
            var parser = new DesignationParser();

            var valids = new List<Triad>()
            {
                new Triad() 
                {
                    ExpectedShortName = "0В", 
                    Source = parser.GetDesignation("0В"), 
                    Destination = parser.GetDesignation("0В"),
                },
                new Triad()
                {
                    ExpectedShortName = "+12В",
                    Source = parser.GetDesignation("+12В"),
                    Destination = parser.GetDesignation("+12В"),
                },
                new Triad()
                {
                    ExpectedShortName = "NO1",
                    Source = parser.GetDesignation("NO1"),
                    Destination = parser.GetDesignation("NO1"),
                },
                new Triad()
                {
                    ExpectedShortName = "NO1",
                    Source = parser.GetDesignation("NO1"),
                    Destination = parser.GetDesignation(""),
                },
                new Triad()
                {
                    ExpectedShortName = "+12В",
                    Source = parser.GetDesignation(""),
                    Destination = parser.GetDesignation("+12В"),
                },
                new Triad()
                {
                    ExpectedShortName = "+12В",
                    Source = parser.GetDesignation("+12В"),
                    Destination = parser.GetDesignation(""),
                },
            };

            for(var i = 0; i < valids.Count; i++)
            {
                var valid = valids[i];
                var result = namesConverter.GetShortName(valid.Source, valid.Destination);

                Debug.WriteLine(i + "  " + valid.ExpectedShortName + " <=> " + result);

                Assert.AreEqual(valid.ExpectedShortName, result);
            }
        }
    }

    internal class Triad
    {
        public string ExpectedShortName = "";
        public HalfWireDesignation Source;
        public HalfWireDesignation Destination;

        public override bool Equals(object obj)
        {
            if (obj == null) 
                return false;

            if (obj is Triad triad)
            {
                if (ExpectedShortName.Equals(triad.ExpectedShortName) &&
                    Source.Equals(triad.Source) &&
                    Destination.Equals(triad.Destination))
                    return true;
            }
            
            return false;
        }

        public override int GetHashCode()
        {
            int hashCode = 1423233825;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ExpectedShortName);
            hashCode = hashCode * -1521134295 + EqualityComparer<HalfWireDesignation>.Default.GetHashCode(Source);
            hashCode = hashCode * -1521134295 + EqualityComparer<HalfWireDesignation>.Default.GetHashCode(Destination);
            return hashCode;
        }
    }
}
