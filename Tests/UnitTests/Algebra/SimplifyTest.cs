using AngouriMath;
using AngouriMath.Core;
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
            var ex = x * 3;
            var expr = ex * 2;
            Assert.IsTrue(expr.Simplify() == 6 * x);
        }

        [TestMethod]
        public void TestPow1()
        {
            var x = MathS.Var("x");
            var ex = 3 * x;
            var expr = MathS.Pow(ex, 1);
            var res = expr.Simplify();
            Assert.IsTrue(res == ex);
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
        [TestMethod]
        public void TestPatt1()
        {
            var x = MathS.Var("x");
            var expr = MathS.Pow(x * 4, 3);
            Assert.IsTrue(expr.Simplify() == 64 * MathS.Pow(x, 3));
        }
        [TestMethod]
        public void TestPatt2()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))) / (2 * MathS.Sin(x - y) * MathS.Cos(x - y) + 1);
            Assert.IsTrue(expr.Simplify() == 1 / (MathS.Sin(2 * (x - y)) + 1));
        }
        [TestMethod]
        public void TestPatt3()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = (x - y) * (x + y);
            Assert.IsTrue(expr.Simplify() == MathS.Sqr(x) - MathS.Sqr(y));
        }
        [TestMethod]
        public void TestPatt4()
        {
            var x = MathS.Var("x");
            var y = MathS.Var("y");
            var expr = (x - y) * (x + y) / (x * x - y * y);
            Assert.IsTrue(expr.Simplify() == 1);
        }
        [TestMethod]
        public void TestPatt5()
        {
            var x = MathS.Var("x");
            var expr = (x + 3) * (3 / (x + 3));
            Assert.IsTrue(expr.Simplify() == 3);
        }
        [TestMethod]
        public void TestPatt6()
        {
            var x = MathS.Var("x");
            var expr = (x + 1) * (x + 2) * (x + 3) / ((x + 2) * (x + 3));
            Assert.IsTrue(expr.Simplify() == x + 1);
        }
    }
}
