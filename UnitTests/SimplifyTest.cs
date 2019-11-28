using MathSharp;
using MathSharp.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class SimplifyTest
    {
        [TestMethod]
        public void TestMinus()
        {
            var x = MathS.Var("x");
            var expr = x - x;
            Assert.IsTrue(expr.Simplify() == 0);
        }
        [TestMethod]
        public void TestMul0()
        {
            var x = MathS.Var("x");
            var expr = (x * 3) * 0;
            Assert.IsTrue(expr.Simplify() == 0);
        }
        [TestMethod]
        public void TestMul1()
        {
            var x = MathS.Var("x");
            var ex = (x * 3);
            var expr = ex * 1;
            Assert.IsTrue(expr.Simplify() == ex);
        }

        [TestMethod]
        public void TestPow1()
        {
            var x = MathS.Var("x");
            var ex = (x * 3);
            var expr = MathS.Pow(ex, 1);
            Assert.IsTrue(expr.Simplify() == ex);
        }
        [TestMethod]
        public void TestPow0()
        {
            var x = MathS.Var("x");
            var ex = (x * 3);
            var expr = MathS.Pow(ex, 0);
            Assert.IsTrue(expr.Simplify() == 1);
        }
        [TestMethod]
        public void TestSum0()
        {
            var x = MathS.Var("x");
            var ex = x + 0;
            Assert.IsTrue(ex.Simplify() == x);
        }
    }
}
