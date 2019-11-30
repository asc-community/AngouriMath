using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CircleTest
    {
        [TestMethod]
        public void Test1()
        {
            string expr = "1 + 2 * log(2, 3)";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }
        [TestMethod]
        public void Test2()
        {
            string expr = "2 ^ 3 + sin(3)";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }
        [TestMethod]
        public void Test3()
        {
            string expr = "23.3 + 3 / 3 + i";
            Assert.AreEqual(expr, MathS.FromString(expr).ToString());
        }
    }
}
