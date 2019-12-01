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
        [TestMethod]
        public void Test4()
        {
            var x = MathS.Var("x");
            MathS.FromString((MathS.Sin(x) / MathS.Cos(x)).Derive(x).ToString());
        }
        [TestMethod]
        public void Test5()
        {
            Assert.IsTrue(MathS.Num(1).ToString() == "1");
            Assert.IsTrue(MathS.Num(-1).ToString() == "(-1)");
        }
        [TestMethod]
        public void Test6()
        {
            Assert.IsTrue(MathS.Num(MathS.i).ToString() == "i");
            Assert.IsTrue(MathS.Num(-1 * MathS.i).Eval().ToString() == "(-i)");
        }
    }
}
