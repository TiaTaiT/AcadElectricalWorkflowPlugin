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
            var testStr1 = "1.1ШСi2.3+";
            var expected1 = "2.3";

            var result = StringUtils.GetStringNumbersWithPoint(testStr1).Last();

            Assert.AreEqual(expected1, result);
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
