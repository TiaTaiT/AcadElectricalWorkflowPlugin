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
                targetKc, WireNameGenerator.SignalType.KC);

            StringAssert.StartsWith(shortNameKc, "КЦ6+");

            var sourcePowerN = "N";
            var targetPowerN = "N";
            var shortNamePowerN1 = WireNameGenerator.GetShortWireName(sourcePowerN,
                targetPowerN, WireNameGenerator.SignalType.Power);

            StringAssert.StartsWith(shortNamePowerN1, "N");

            var shortNamePowerN2 = WireNameGenerator.GetShortWireName("N", "GND", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerN2, "??");

            var shortNamePowerGND1 = WireNameGenerator.GetShortWireName("0В", "GND", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerGND1, "0В");

            var shortNamePowerGND2 = WireNameGenerator.GetShortWireName("GND", "0В", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerGND2, "GND");

            var shortNamePowerGND3 = WireNameGenerator.GetShortWireName("GND", "GND1", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerGND3, "GND");

            var shortNamePowerGND4 = WireNameGenerator.GetShortWireName("0В", "-U1", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerGND4, "0В");

            var shortNamePowerPlus1 = WireNameGenerator.GetShortWireName("+24В", "(12-24)В", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus1, "+24В");

            var shortNamePowerPlus2 = WireNameGenerator.GetShortWireName("+24В", "+U1", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus2, "+24В");

            var shortNamePowerPlus3 = WireNameGenerator.GetShortWireName("+U1", "+24В", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus3, "+24В");

            var shortNamePowerPlus4 = WireNameGenerator.GetShortWireName("0В", "4", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus4, "0В");

            var shortNamePowerPlus5 = WireNameGenerator.GetShortWireName("-ПИ1", "0В", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus5, "0В");

            var shortNamePowerPlus6 = WireNameGenerator.GetShortWireName("0В", "-ПИ2", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNamePowerPlus6, "0В");

            var shortNameTheSame = WireNameGenerator.GetShortWireName("+12В", "+12В", WireNameGenerator.SignalType.Power);
            StringAssert.StartsWith(shortNameTheSame, "+12В");
        }
    }
}
