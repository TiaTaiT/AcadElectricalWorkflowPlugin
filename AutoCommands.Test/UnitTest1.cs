using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using AutocadTerminalsManager;
using AutocadCommands.Services;
using System.Collections.Generic;
using LinkCommands.Models;
using AutocadCommands.Models;
using Autodesk.AutoCAD.DatabaseServices;

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
        public void CheckFindReplaceWithIncrement()
        {
            // Arrange
            /*
            var unsortedWires = new List<CompareWire>()
            {
                new CompareWire(){ X = 252.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 247.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 267.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 257.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 237.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 232.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 185, WireId = new ObjectId() },
            };

            var sortedWires = new List<CompareWire>()
            {
                new CompareWire(){ X = 257.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 267.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 252.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 232.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 237.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 247.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 185, WireId = new ObjectId() },
            };

            var sortedWires2 = new List<CompareWire>()
            {
                new CompareWire(){ X = 257.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 267.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 175, WireId = new ObjectId() },
                new CompareWire(){ X = 252.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 232.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 237.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 247.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 262.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 272.5, Y = 185, WireId = new ObjectId() },
                new CompareWire(){ X = 277.5, Y = 185, WireId = new ObjectId() },
            };
            // Act

            unsortedWires.Sort(new HalfWire());
            // Assert
            CollectionAssert.AreEqual(sortedWires, sortedWires2, new CompareWireComparer());
            */
        }
    }
}
