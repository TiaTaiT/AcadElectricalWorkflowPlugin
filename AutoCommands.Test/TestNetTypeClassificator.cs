
using CommonHelpers.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestNetTypeClassificator
    {
        [TestMethod]
        public void TestGetNetType()
        {
            List<(NetTypes, string, string)> validSet = new()
            {
                new ( NetTypes.ShleifPositive, "ШС", "+" ),
                new ( NetTypes.ShleifPositive, "КЦ", "+" ),
                new ( NetTypes.ShleifNegative, "КЦ", "-" ),
                new ( NetTypes.ShleifNegative, "ШС", "-" ),
                new ( NetTypes.PowerPositive, "В", "+" ),
                new ( NetTypes.PowerPositive, "ПИ", "+" ),
                new ( NetTypes.PowerPositive, "+U", "+" ),
                new ( NetTypes.PowerNegative, "В", "-" ),
                new ( NetTypes.PowerNegative, "ПИ", "-" ),
                new ( NetTypes.PowerNegative, "-U", "-" ),
                new ( NetTypes.Rs485A, "A", "" ),
            };
        }
    }
}
