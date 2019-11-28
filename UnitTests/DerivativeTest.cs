using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class DerivativeTest
    {
        [TestMethod]
        public void Test1()
        {
            var x = MathS.Var("x");
            var func = MathS.Sqr(x) + 2 * x + 1;
            Assert.IsTrue(func.Derive(x).Simplify() == 2 * x + 2);
        }
        [TestMethod]
        public void TestSin()
        {
            var x = MathS.Var("x");
            var func = MathS.Sin(x);
            Assert.IsTrue(func.Derive(x).Simplify() == MathS.Cos(x));
        }
        [TestMethod]
        public void TestCosCustom()
        {
            var x = MathS.Var("x");
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -MathS.Sin(MathS.Pow(x, 3)) * (3 * MathS.Sqr(x));
            var actual = func.Derive(x).Simplify();
            Assert.IsTrue(expected == actual);
        }
        [TestMethod]
        public void TestPow()
        {
            var x = MathS.Var("x");
            var func = MathS.Pow(MathS.e, x);
            Assert.IsTrue(func.Derive(x).Simplify() == func);
        }
        [TestMethod]
        public void TestPoly()
        {
            var x = MathS.Var("x");
            var func = MathS.Pow(x, 4);
            Assert.IsTrue(func.Derive(x).Simplify() == 4 * MathS.Pow(x, 3));
        }
    }
}
