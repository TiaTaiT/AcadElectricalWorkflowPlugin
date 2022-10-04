using CommonHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkCommands.Services;
using System.Security.Cryptography;
using System.Security.Policy;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestLinkSymbolNameResolver
    {
        [TestMethod]
        public void TestGetAllNames()
        {
            // Arrange
            var referenceNames = new List<string>()
            {
                "HA3S3",
                "HA3S4",
                "HA3S1",
                "HA3S2",
                "HA3D3",
                "HA3D4",
                "HA3D1",
                "HA3D2",
                "HA4S3",
                "HA4S4",
                "HA4S1",
                "HA4S2",
                "HA4D3",
                "HA4D4",
                "HA4D1",
                "HA4D2"
            };

            // Act
            var names = LinkSymbolNameResolver.GetAllNames().ToList();

            // Assert
            CollectionAssert.AreEquivalent(referenceNames, names);
        }
    }
}
