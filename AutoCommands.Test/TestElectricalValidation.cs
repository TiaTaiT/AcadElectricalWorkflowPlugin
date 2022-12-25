using LinkCommands.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestElectricalValidation
    {
        private List<(string, string)> _invalidSet = new()
            {
                new ("0В", "A"),
                new ("0В", "B"),
                new ("ШС1+", "+24В"),
                new ("ШС3+", "ШС3-"),
                new ("ШС2-", "ШС3-"),
                new ("ЛС+", "RS485(A)"),
                new ("ЛС-", "RS485(B)"),
                new ("RS485(A)", "RS485(B)"),
            };

        private List<(string, string)> _validSet = new()
            {
                new ("0В", "0В"),
                new ("0В", "GND"),
                new ("ШС16+", "ШС16+"),
                new ("NC1", "ШС2-"),
                new ("ЛС+", "ЛС+"),
                new ("RS485(A)", "RS485(A)"),
                new ("RS485(B)", "RS485(B)"),
            };

        private List<(string, string)> _invalidSetTerminal = new()
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
                new ("NC2(+12В)", "NO2"),
            };

        private List<(string, string)> _validSetTerminal = new()
            {
                new ("ШС16+", "ШС16+"),
                new ("NO1", "NO1"),
                new ("0В", "0В"),
                new ("ЛС+", "ЛС+"),
                new ("A", "RS485(A)"),
                new ("B1", "RS485(B)"),
            };

        [TestMethod]
        public void TestValidateMethod()
        {
            var designationParser = new DesignationParser();
            var namesConverter = new NamesConverter();


            foreach (var invalid in _invalidSet)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter);
                var result = validator.IsConnectionValid(invalid.Item1, invalid.Item2);
                Debug.WriteLine("Source = " + invalid.Item1 + " & Destination = " + invalid.Item2);
                Assert.IsFalse(result);
            }

            foreach (var valid in _validSet)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter);
                var result = validator.IsConnectionValid(valid.Item1, valid.Item2);
                Debug.WriteLine("Source = " + valid.Item1 + " & Destination = " + valid.Item2);
                Assert.IsTrue(result);
            }

            foreach (var invalid in _invalidSetTerminal)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter)
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.IsConnectionValid(invalid.Item1, invalid.Item2);
                Debug.WriteLine("Source = " + invalid.Item1 + " & Destination = " + invalid.Item2);
                Assert.IsFalse(result);
            }

            foreach (var valid in _validSetTerminal)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter)
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.IsConnectionValid(valid.Item1, valid.Item2);
                Debug.WriteLine("Source = " + valid.Item1 + " & Destination = " + valid.Item2);
                Assert.IsTrue(result);
            }
        }

        [TestMethod]
        public void TestIsConnectionValid()
        {
            var designationParser = new DesignationParser();
            var namesConverter = new NamesConverter();

            for (var i = 0; i < _invalidSet.Count; i++)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter);
                var result = validator.IsConnectionValid(_invalidSet[i].Item1, _invalidSet[i].Item2);
                Debug.WriteLine(i + "  Source = " + _invalidSet[i].Item1 + " & Destination = " + _invalidSet[i].Item2 + "; Invalid");
                Assert.IsFalse(result);
            }

            for (var i = 0; i < _validSet.Count; i++)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter);
                var result = validator.IsConnectionValid(_validSet[i].Item1, _validSet[i].Item2);
                Debug.WriteLine(i + "  Source = " + _validSet[i].Item1 + " & Destination = " + _validSet[i].Item2 + "; Valid");
                Assert.IsTrue(result);
            }

            for (var i = 0; i < _invalidSetTerminal.Count; i++)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter)
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.IsConnectionValid(_invalidSetTerminal[i].Item1, _invalidSetTerminal[i].Item2);
                Debug.WriteLine(i + "  Source = " + _invalidSetTerminal[i].Item1 + " & Destination = " + _invalidSetTerminal[i].Item2 + "; Invalid Terminal");
                Assert.IsFalse(result);
            }

            for (var i = 0; i < _validSetTerminal.Count; i++)
            {
                var validator = new ElectricalValidation(designationParser, namesConverter)
                {
                    ValidationParameterIsTerminal = true
                };
                var result = validator.IsConnectionValid(_validSetTerminal[i].Item1, _validSetTerminal[i].Item2);
                Debug.WriteLine(i + "  Source = " + _validSetTerminal[i].Item1 + " & Destination = " + _validSetTerminal[i].Item2 + "; Valid Terminal");
                Assert.IsTrue(result);
            }
        }
    }
}
