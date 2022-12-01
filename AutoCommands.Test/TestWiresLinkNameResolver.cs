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
    public class TestWiresLinkNameResolver
    {
        [TestMethod]
        public void TestResolving()
        {
            var expected = new List<string>()
            {
                "ha2s1_inline",
                "ha2s2_inline",
                "ha2s3_inline",
                "ha2s4_inline",
                "ha2d1_inline",
                "ha2d2_inline",
                "ha2d3_inline",
                "ha2d4_inline",
            };

            var result = WiresLinkNameResolver.GetAllNames().ToList();

            CollectionAssert.AreEquivalent(expected, result);
        }
    }
}
