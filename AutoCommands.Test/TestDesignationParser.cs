using CommonHelpers.Models;
using LinkCommands.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestDesignationParser
    {
        [TestMethod]
        public void TestGetDesignation()
        {
            var designationParser = new DesignationParser();

            List<(string, HalfWireDesignation)> validExpected = new()
            {
                ("ЛС+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ЛС",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "+",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.LadogaRsPositive,
                        SurgeProtection = false,
                    }
                ),
                ("ЛС-", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ЛС",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "-",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.LadogaRsNegative,
                        SurgeProtection = false,
                    }
                ),
                ("+U1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "+U",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("ПИ1+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ПИ",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "+",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("ШСi3+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "3",
                        SparkProtection = "i",
                        Suffix = "+",
                        ElectricalType = NetTypes.ShleifPositive,
                        SurgeProtection = false,
                    }
                ),
                ("L", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "L",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerAc,
                        SurgeProtection = false,
                    }
                ),
                ("N1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "N",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerAc,
                        SurgeProtection = false,
                    }
                ),
                ("PE1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "PE",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerAc,
                        SurgeProtection = false,
                    }
                ),
                ("GND1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("GND", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("2.1GND5", new HalfWireDesignation()
                    {
                        Location = "2.1",
                        Appointment = "GND",
                        Number = "5",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("0В", new HalfWireDesignation()  // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("0В3", new HalfWireDesignation() // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("0V", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "V",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 0,
                        UpperVoltage = 0,
                        ElectricalType = NetTypes.PowerNegative,
                        SurgeProtection = false,
                    }
                ),
                ("+5В", new HalfWireDesignation() // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 5,
                        UpperVoltage = 5,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("+24В25", new HalfWireDesignation() // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "25",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 24,
                        UpperVoltage = 24,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("(10-28)В", new HalfWireDesignation() // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 10,
                        UpperVoltage = 28,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("(10-28)В1", new HalfWireDesignation() // В - русская
                    {
                        Location = "",
                        Appointment = "В", // В - русская
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 10,
                        UpperVoltage = 28,
                        ElectricalType = NetTypes.PowerPositive,
                        SurgeProtection = false,
                    }
                ),
                ("ШС3+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "+",
                        ElectricalType = NetTypes.ShleifPositive,
                        SurgeProtection = false,
                    }
                ),

                ("ШС20-", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ШС",
                        Number = "20",
                        SparkProtection = "",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                        SurgeProtection = false,
                    }
                ),
                ("1.3ШС18-", new HalfWireDesignation()
                    {
                        Location = "1.3",
                        Appointment = "ШС",
                        Number = "18",
                        SparkProtection = "",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                        SurgeProtection = false,
                    }
                ),
                ("1.3ШСi18-", new HalfWireDesignation()
                    {
                        Location = "1.3",
                        Appointment = "ШС",
                        Number = "18",
                        SparkProtection = "i",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                        SurgeProtection = false,
                    }
                ),
                ("5ШС19-", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "ШС",
                        Number = "19",
                        SparkProtection = "",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                        SurgeProtection = false,
                    }
                ),
                ("(5ШС19-)", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "ШС",
                        Number = "19",
                        SparkProtection = "",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                        SurgeProtection = true,
                    }
                ),
                ("1.3.4КЦ10-", new HalfWireDesignation()
                    {
                        Location = "1.3.4",
                        Appointment = "КЦ",
                        Number = "10",
                        SparkProtection = "",
                        Suffix = "-",
                        ElectricalType = NetTypes.ShleifNegative,
                    }
                ),
                ("NO1", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "NO",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Relay,
                    }
                ),
                ("5NO3", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "NO",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Relay,
                    }
                ),
                ("5NC3", new HalfWireDesignation()
                    {
                        Location = "5",
                        Appointment = "NC",
                        Number = "3",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Relay,
                    }
                ),
                ("NC2(+12В)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "NC",
                        Number = "2",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 12,
                        UpperVoltage = 12,
                        ElectricalType = NetTypes.Relay,
                    }
                ),
                ("1.6NO2(+12В)", new HalfWireDesignation()
                    {
                        Location = "1.6",
                        Appointment = "NO",
                        Number = "2",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 12,
                        UpperVoltage = 12,
                        ElectricalType = NetTypes.Relay,
                    }
                ),
                ("(1.6NO2(+12В))", new HalfWireDesignation()
                    {
                        Location = "1.6",
                        Appointment = "NO",
                        Number = "2",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 12,
                        UpperVoltage = 12,
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = true,
                    }
                ),
                ("4K1(+12В)", new HalfWireDesignation()
                    {
                        Location = "4",
                        Appointment = "K",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        LowerVoltage = 12,
                        UpperVoltage = 12,
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("K", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "K",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("NO", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "NO",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("ДК1+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "+",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("4.3.8ДК1+", new HalfWireDesignation()
                    {
                        Location = "4.3.8",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "+",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("ДКi12+", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "ДК",
                        Number = "12",
                        SparkProtection = "i",
                        Suffix = "+",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("4.3.8ДКi1+", new HalfWireDesignation()
                    {
                        Location = "4.3.8",
                        Appointment = "ДК",
                        Number = "1",
                        SparkProtection = "i",
                        Suffix = "+",
                        ElectricalType = NetTypes.Relay,
                        SurgeProtection = false,
                    }
                ),
                ("RS485(A)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "A",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485A,
                        SurgeProtection = false,
                    }
                ),
                ("RS485(B)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "B",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485B,
                        SurgeProtection = false,
                    }
                ),
                ("RS485(GND)", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "GND",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485Gnd,
                        SurgeProtection = false,
                    }
                ),
                ("A", new HalfWireDesignation()
                    {
                        Location = "",
                        Appointment = "A",
                        Number = "",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485A,
                        SurgeProtection = false,
                    }
                ),
                ("B5", new HalfWireDesignation() // B - english
                    {
                        Location = "",
                        Appointment = "B", // B - english
                        Number = "5",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485B,
                        SurgeProtection = false,
                    }
                ),
                ("1.2B1", new HalfWireDesignation() // B - english
                    {
                        Location = "1.2",
                        Appointment = "B", // B - english
                        Number = "1",
                        SparkProtection = "",
                        Suffix = "",
                        ElectricalType = NetTypes.Rs485B,
                        SurgeProtection = false,
                    }
                ),
            };

            var counter = 0;
            foreach (var expected in validExpected)
            {
                var result = designationParser.GetDesignation(expected.Item1);
                Debug.WriteLine(counter + "  Test Designation: " + expected.Item1);
                Assert.AreEqual(expected.Item2, result);
                counter++;
            }


        }
    }
}
