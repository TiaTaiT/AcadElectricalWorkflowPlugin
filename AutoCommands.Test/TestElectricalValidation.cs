using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkCommands.Services;
using System.Diagnostics;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestElectricalValidation
    {
        [TestMethod]
        public void TestValidateMethod()
        {
            var invalidSet = new List<(string, string)>()
            {
                new ("0В", "A"),
                new ("0В", "B"),
                new ("ШС1+", "+24В"),
                new ("ШС3+", "ШС3-"),
                new ("ЛС+", "RS485(A)"),
                new ("ЛС-", "RS485(B)"),
                new ("RS485(A)", "RS485(B)"),
            };

            var validSet = new List<(string, string)>()
            {
                new ("0В", "0В"),
                new ("0В", "GND"),
                new ("ШС16+", "ШС16+"),
                new ("NC1", "ШС2-"),
                new ("ЛС+", "ЛС+"),
                new ("RS485(A)", "RS485(A)"),
                new ("RS485(B)", "RS485(B)"),
            };

            var invalidSetTerminal = new List<(string, string)>()
            {
                new ("ШС16+", "NO1"),
                new ("NO1", "COM1"),
                new ("ШС16+", "A"),
                new ("A", "B"),
                new ("0В", "B"),
                new ("ШС1-", "0В"),
                new ("ШС4+", "+12В1"),
                new ("ШС20-", "GND"),
                new ("ШС1+", "RS485(A)"),
                new ("ШС5-", "RS485(B)"),
                new ("ШС5+", "ЛС+"),
                new ("0В", "ЛС-"),

            };

            var validSetTerminal = new List<(string, string)>()
            {
                new ("ШС16+", "ШС16+"),
                new ("NO1", "NO1"),
                new ("0В", "0В"),
                new ("ЛС+", "ЛС+"),
            };

            foreach (var invalid in invalidSet)
            {
                var validator = new ElectricalValidation();
                var result = validator.ValidateWire(invalid.Item1, invalid.Item2);
                Debug.WriteLine("Source = " + invalid.Item1 + " & Destination = " + invalid.Item2);
                Assert.IsFalse(result);
            }

            foreach (var valid in validSet)
            {
                var validator = new ElectricalValidation();
                var result = validator.ValidateWire(valid.Item1, valid.Item2);
                Debug.WriteLine("Source = " + valid.Item1 + " & Destination = " + valid.Item2);
                Assert.IsTrue(result);
            }

            foreach (var invalid in invalidSetTerminal)
            {
                var validator = new ElectricalValidation()
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.ValidateWire(invalid.Item1, invalid.Item2);
                Debug.WriteLine("Source = " + invalid.Item1 + " & Destination = " + invalid.Item2);
                Assert.IsFalse(result);
            }

            foreach (var valid in validSetTerminal)
            {
                var validator = new ElectricalValidation()
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.ValidateWire(valid.Item1, valid.Item2);
                Debug.WriteLine("Source = " + valid.Item1 + " & Destination = " + valid.Item2);
                Assert.IsTrue(result);
            }
        }
    }
}
