using LinkCommands.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestNetTypeClassificator
    {
        [TestMethod]
        public void CheckPowerPositives()
        {
            var validPositives = new List<string>() { "+24В", "+24", "+5V", "ПИ+", "ПИ1+", "ПИ2+", "ПИ3+", "ПИ4+", "ПИ5+", "ПИ6+", "ПИ7+", "ПИ8+", "+U1", "+U2" };
            var notValidPositives = new List<string>() { "-24В", "-24", "-5V", "0В", "0", "+" };

            foreach (var item in validPositives)
            {
                Assert.AreEqual(NetTypes.PowerPositive, NetTypeClassificator.GetNetTypes(item));
            }
            foreach (var item in notValidPositives)
            {
                Assert.AreNotEqual(NetTypes.PowerPositive, NetTypeClassificator.GetNetTypes(item));
            }           
        }

        [TestMethod]
        public void CheckPowerNegatives()
        {
            var validNegatives = new List<string>() { "-24В", "-24", "-5V", "0В", "GND", "ПИ-", "ПИ1-", "ПИ2-", "ПИ3-", "ПИ4-", "ПИ5-", "ПИ6-", "ПИ7-", "ПИ8-", "-U1", "-U2" };
            var notValidNegatives = new List<string>() { "+24В", "+24", "+5V", "0", "-" };

            foreach (var item in validNegatives)
            {
                Assert.AreEqual(NetTypes.PowerNegative, NetTypeClassificator.GetNetTypes(item));
            }
            foreach (var item in notValidNegatives)
            {
                Assert.AreNotEqual(NetTypes.PowerNegative, NetTypeClassificator.GetNetTypes(item));
            }
        }

        [TestMethod]
        public void CheckRS485A()
        {
            var validNegatives = new List<string> { "RS485A", "RS485(A)", "A", "A1", "A2", "A3", "A4", "ЛС+" };
            var notValidNegatives = new List<string>() { "K" };

            foreach (var item in validNegatives)
            {
                Assert.AreEqual(NetTypes.Rs485A, NetTypeClassificator.GetNetTypes(item));
            }
            foreach (var item in notValidNegatives)
            {
                Assert.AreNotEqual(NetTypes.Rs485A, NetTypeClassificator.GetNetTypes(item));
            }
        }

        [TestMethod]
        public void CheckRS48B()
        {
            var validNegatives = new List<string> { "RS485B", "RS485(B)", "B", "B1", "B2", "B3", "B4", "ЛС-" };
            var notValidNegatives = new List<string>() { "K" };

            foreach (var item in validNegatives)
            {
                Assert.AreEqual(NetTypes.Rs485B, NetTypeClassificator.GetNetTypes(item));
            }
            foreach (var item in notValidNegatives)
            {
                Assert.AreNotEqual(NetTypes.Rs485B, NetTypeClassificator.GetNetTypes(item));
            }
        }
    }
}
