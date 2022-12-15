using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkCommands.Services;

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
            };

            var validSet = new List<(string, string)>()
            {
                new ("0В", "0В"),
                new ("0В", "GND"),
                new ("ШС16+", "ШС16+"),
                new ("NC1", "ШС2-"),
            };

            var invalidSetTerminal = new List<(string, string)>()
            {
                new ("ШС16+", "NO1"),
                new ("NO1", "COM1"),
                new ("ШС16+", "A"),
                new ("A", "B"),
                new ("0В", "B"),
            };

            var validSetTerminal = new List<(string, string)>()
            {
                new ("ШС16+", "ШС16+"),
                new ("NO1", "NO1"),
                new ("0В", "0В"),
            };

            foreach (var invalid in invalidSet)
            {
                var validator = new ElectricalValidation(invalid.Item1, invalid.Item2);
                var result = validator.IsValid;
                Assert.IsFalse(result);
            }

            foreach (var valid in validSet)
            {
                var validator = new ElectricalValidation(valid.Item1, valid.Item2);
                var result = validator.IsValid;
                Assert.IsTrue(result);
            }

            foreach (var invalid in invalidSetTerminal)
            {
                var validator = new ElectricalValidation()
                {
                    ValidationParameterIsTerminal = true
                };
                validator.ValidateWire(invalid.Item1, invalid.Item2);
                var result = validator.IsValid;
                Assert.IsFalse(result);
            }

            foreach (var valid in validSetTerminal)
            {
                var validator = new ElectricalValidation()
                {
                    ValidationParameterIsTerminal = true
                };
                validator.ValidateWire(valid.Item1, valid.Item2);
                var result = validator.IsValid;
                Assert.IsTrue(result);
            }
        }
    }
}
