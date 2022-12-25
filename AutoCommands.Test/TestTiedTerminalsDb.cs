using LinkCommands.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestTiedTerminalsDb
    {
        [TestMethod]
        public void TestGetTiedTerminals()
        {
            // Arrange
            var componentName = "УЗЛ-СД-24";
            var terminalDescription = "1";
            var expectedResult = new List<string>() { "6" };

            // Act
            var result = TiedTerminalsDb.GetTiedTerminals(componentName, terminalDescription).ToList();

            // Assert
            CollectionAssert.AreEquivalent(expectedResult, result);
        }
    }
}
