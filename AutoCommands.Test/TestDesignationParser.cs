using LinkCommands.Models;
using LinkCommands.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCommands.Test
{ 
    [TestClass]
    public class TestDesignationParser
    {
        [TestMethod]  
        public void TestGetDesignation()
        {
            List<(string, HalfWireDesignation)> validExpected = new()
            {
                ("ШСi3+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "3",
                        SparkProtection = "i",
                        Suffix = "+",
                    }
                ),
                ("L", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "L",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("N1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "N",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("PE1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "PE",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("GND1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("GND", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("2.1GND5", new HalfWireDesignation()
                    {
                        Location = "2.1",
                        Appointment = "GND",
                        Number = "5",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("0В", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "0В",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("0В3", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "0В",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("0V", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "0V",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("+5В", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "+5В",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("+24В24", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "+24В",
                        Number = "24",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("ШС3+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "+",
                    }
                ),
                
                ("ШС20-", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "20",
                        SparkProtection = "",
                        Suffix = "-",
                    }
                ),
                ("1.3ШС18-", new HalfWireDesignation()
                    {
                        Location = "1.3",
                        Appointment = "ШС",
                        Number = "18",
                        SparkProtection = "",
                        Suffix = "-",
                    }
                ),
                ("1.3ШСi18-", new HalfWireDesignation()
                    {
                        Location = "1.3",
                        Appointment = "ШС",
                        Number = "18",
                        SparkProtection = "i",
                        Suffix = "-",
                    }
                ),
                ("5ШС19-", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "ШС",
                        Number = "19",
                        SparkProtection = "",
                        Suffix = "-",
                    }
                ),
                ("1.3.4КЦ10-", new HalfWireDesignation()
                    {
                        Location = "1.3.4",
                        Appointment = "КЦ",
                        Number = "10",
                        SparkProtection = "",
                        Suffix = "-",
                    }
                ),
                ("5NO3", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "NO",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("5NC3", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "NC",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("1.6NO2(+12В)", new HalfWireDesignation()
                    {
                        Location = "1.6",
                        Appointment = "NO",
                        Number = "2",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("4K1(+12В)", new HalfWireDesignation()
                    {
                        Location = "4",
                        Appointment = "K",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("K", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "K",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("NO", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "NO",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("ДК1+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "+",
                    }
                ),
                ("4.3.8ДК1+", new HalfWireDesignation()
                    {
                        Location = "4.3.8",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "+",
                    }
                ),
                ("ДКi12+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ДК",
                        Number = "12",
                        SparkProtection = "i",
                        Suffix = "+",
                    }
                ),
                ("4.3.8ДКi1+", new HalfWireDesignation()
                    {
                        Location = "4.3.8",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "i",
                        Suffix = "+",
                    }
                ),
                ("RS485(A)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "A",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("RS485(B)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "B",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("RS485(GND)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("A", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "A",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("B5", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "B",
                        Number = "5",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
                ("1.2B1", new HalfWireDesignation()
                    {
                        Location = "1.2",
                        Appointment = "B",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                    }
                ),
            };

            foreach(var expected in validExpected)
            {
                var result = DesignationParser.GetDesignation(expected.Item1);

                Assert.AreEqual(expected.Item2, result);
            }
            

        }
    }
}
