using Autodesk.AutoCAD.Geometry;
using LinkCommands.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestElectricalComponent
    {
        [TestMethod]
        public void TestGetAllTiedTerminals()
        {
            //Arrange
            var terminals = new List<ComponentTerminal>()
            {
                new ComponentTerminal(new List<Point3d>(), "TERM01", "1"),
                new ComponentTerminal(new List<Point3d>(), "TERM02", "2"),
                new ComponentTerminal(new List<Point3d>(), "TERM03", "3"),
                new ComponentTerminal(new List<Point3d>(), "TERM05", "4"),
                new ComponentTerminal(new List<Point3d>(), "TERM06", "PE"),
                new ComponentTerminal(new List<Point3d>(), "TERM07", "PE"),
                new ComponentTerminal(new List<Point3d>(), "TERM08", "PE"),

            };

            var tiedTerminals = new List<List<ComponentTerminal>>()
            {
                new List<ComponentTerminal>(){ terminals[0], terminals[1] },
                new List<ComponentTerminal>(){ terminals[3], terminals[4] },
                new List<ComponentTerminal>(){ terminals[5], terminals[6], terminals[7] },
            };

            var component = new ElectricalComponent(1, "УЗЛ-СД-24", "U10", terminals, null);
            //Act
            var result1 = component.GetAllTiedTerminals("1");
            var result2 = component.GetAllTiedTerminals("3");
            var result3 = component.GetAllTiedTerminals("PE");

            //Assert
            Assert.Equals(result1.Count(), 2);
            Assert.Equals(result2.Count(), 2);
            Assert.Equals(result3.Count(), 3);
        }
    }
}
