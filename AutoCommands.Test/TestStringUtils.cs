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
    public class TestStringUtils
    {
        [TestMethod]
        public void TestGetStringNumbersWithPoint()
        {
            var testList = new List<string>
            {
                "1.2A",
                "iШС13+",
                "ШСi3+",
                "1.1ШСi2.3+",
                "0В",
                "+24В1",
                "GND",
            };
            var expectedList = new List<List<string>>()
            {
                new List<string>()
                {
                    "1.2",
                    "A",
                },
                new List<string>()
                {
                    "iШС",
                    "13",
                    "+",
                },
                new List<string>()
                {
                    "ШС",
                    "i",
                    "3",
                    "+",
                },
                new List<string>()
                {
                    "1.1",
                    "ШС",
                    "i",
                    "2.3",
                    "+",
                },
                new List<string>()
                {
                    "0",
                    "В",
                },
                new List<string>()
                {
                    "+",
                    "24",
                    "В",
                    "1",
                },
                new List<string>()
                {
                    "GND",
                },
            };
            
            for(var i = 0; i < testList.Count; i++)
            {
                var result = StringUtils.GetStringNumbersWithPoint(testList[i]).ToList();
                CollectionAssert.AreEquivalent(expectedList[i], result);
            }
            
        }

        [TestMethod]
        public void TestRemovePrefix()
        {
            var testStr1 = "1.1NO1";
            var expected1 = "NO1";

            var result1 = StringUtils.RemovePrefix(testStr1);
            
            Assert.AreEqual(expected1, result1);

            var testStr2 = "NO1";
            var expected2 = "NO1";

            var result2 = StringUtils.RemovePrefix(testStr2);

            Assert.AreEqual(expected2, result2);
        }
    }
}
