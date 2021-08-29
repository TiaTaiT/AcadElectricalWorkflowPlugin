using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AutocadTerminalsManager;

namespace AutoCommands.Test
{
    [TestClass]
    public class UnitTest1
    {
        private const string _jsonPath = @"C:\Temp\assembly.json";

        [TestMethod]
        public void CheckPresenceCable()
        {
            // Arrange

            // Act
            var assemblyManager = new AssemblyManager();
            var assemblyList = assemblyManager.GetAssemblies(_jsonPath);
            var cableList = assemblyList[0].PerimeterCables;

            // Assert
            Assert.IsNotNull(cableList);
        }

        
    }
}
