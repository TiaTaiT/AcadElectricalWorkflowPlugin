using AutocadTerminalsManager;
using CommonHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoCommands.Test
{
    [TestClass]
    public class TestMath
    {
        [TestMethod]
        public void CheckRangesInterset()
        {
            // Arrange
            var intervalA1 = (18, 5);
            var intervalA2 = (12, 24);

            var intervalB1 = (12, 12);
            var intervalB2 = (12, 24);

            var intervalC1 = (9, 36);
            var intervalC2 = (24, 24);

            var intervalD1 = (7, 12);
            var intervalD2 = (15, 15);
            // Act
            var resultA = Mathematic.AreRangesIntersect(intervalA1, intervalA2);
            var resultB = Mathematic.AreRangesIntersect(intervalB1, intervalB2);
            var resultC = Mathematic.AreRangesIntersect(intervalC1, intervalC2);
            var resultD = Mathematic.AreRangesIntersect(intervalD1, intervalD2);

            // Assert
            Assert.IsTrue(resultA);
            Assert.IsTrue(resultB);
            Assert.IsTrue(resultC);
            Assert.IsFalse(resultD);
        }

        [TestMethod]
        public void CheckGetAllNumbersFromString()
        {
            // Arrange
            var strWithNumbs = "(10-65)В";
            var strWithoutNumbs = "(-)В";

            // Act
            var result = StringUtils.GetIntNumbers(strWithNumbs);
            var result2 = StringUtils.GetIntNumbers(strWithoutNumbs);

            // Assert
            Assert.AreEqual(result.Count(), 2);
            Assert.AreEqual(result[0], 10);
            Assert.AreEqual(result[1], 65);
            Assert.AreEqual(result2.Count(), 0);
        }

    }
}
