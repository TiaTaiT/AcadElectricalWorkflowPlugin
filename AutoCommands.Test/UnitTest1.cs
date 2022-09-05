using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AutocadTerminalsManager;
using AutocadCommands.Services;
using System.Collections.Generic;
using LinkCommands.Models;
using AutocadCommands.Models;
using Autodesk.AutoCAD.DatabaseServices;
using LinkCommands.Services;

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

        [TestMethod]
        public void CheckGetShortWireName()
        {
            var sourceShleif = "2ШС6+";
            var targetShleif = "ШС6+";
            
            var shortNameShleif = WireNameGenerator.GetShortWireName(sourceShleif,
                targetShleif, WireNameGenerator.SignalType.Shleif);
            StringAssert.StartsWith(shortNameShleif, "ШС6+");

            var sourceKc = "2КЦ6+";
            var targetKc = "КЦ6+";
            var shortNameKc = WireNameGenerator.GetShortWireName(sourceKc,
                targetKc, WireNameGenerator.SignalType.Shleif);

            StringAssert.StartsWith(shortNameKc, "КЦ6+");
        }
    }
}
