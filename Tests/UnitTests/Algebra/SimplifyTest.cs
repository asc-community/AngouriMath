using AngouriMath;
using AngouriMath.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class SimplifyTest
    {
        public static readonly VariableEntity x = MathS.Var("x");
        [TestMethod]
        public void TestMinus()
        {
            var expr = x - x;
            Assert.IsTrue(expr.Simplify() == 0);
        }
        [TestMethod]
        public void TestMul0()
        {
            var expr = (x * 3) * 0;
            Assert.IsTrue(expr.Simplify() == 0);
        }
        [TestMethod]
        public void TestMul1()
        {
            var ex = x * 3;
            var expr = ex * 2;
            Assert.IsTrue(expr.Simplify() == 6 * x);
        }

        [TestMethod]
        public void TestPow1()
        {
            var ex = 3 * x;
            var expr = MathS.Pow(ex, 1);
            var res = expr.Simplify();
            Assert.IsTrue(res == ex);
        }
        [TestMethod]
        public void TestPow0()
        {
            var ex = (x * 3);
            var expr = MathS.Pow(ex, 0);
            Assert.IsTrue(expr.Simplify() == 1);
        }
        [TestMethod]
        public void TestSum0()
        {
            var ex = x + 0;
            Assert.IsTrue(ex.Simplify() == x);
        }
        [TestMethod]
        public void TestPatt1()
        {
            var expr = MathS.Pow(x * 4, 3);
            Assert.IsTrue(expr.Simplify() == MathS.Pow(4 * x, 3));
        }
        [TestMethod]
        public void TestPatt2()
        {
            var y = MathS.Var("y");
            var expr = (MathS.Sqr(MathS.Sin(x + 2 * y)) + MathS.Sqr(MathS.Cos(x + 2 * y))) / (2 * MathS.Sin(x - y) * MathS.Cos(x - y) + 1);
            Assert.IsTrue(expr.Simplify() == 1 / (MathS.Sin(2 * (x - y)) + 1));
        }
        [TestMethod]
        public void TestPatt3()
        {
            var y = MathS.Var("y");
            var expr = (x - y) * (x + y);
            Assert.IsTrue(expr.Simplify() == MathS.Sqr(x) - MathS.Sqr(y));
        }
        [TestMethod]
        public void TestPatt4()
        {
            var y = MathS.Var("y");
            var expr = (x - y) * (x + y) / (x * x - y * y);
            Assert.IsTrue(expr.Simplify() == 1);
        }
        [TestMethod]
        public void TestPatt5()
        {
            var expr = (x + 3) * (3 / (x + 3));
            Assert.IsTrue(expr.Simplify() == 3);
        }
        [TestMethod]
        public void TestPatt6()
        {
            var expr = (x + 1) * (x + 2) * (x + 3) / ((x + 2) * (x + 3));
            Assert.IsTrue(expr.Simplify() == 1 + x);
        }
        [TestMethod]
        public void TestPatt7()
        {
            var expr = MathS.Arcsin(x * 3) + MathS.Arccos(x * 3);
            Assert.IsTrue(expr.Simplify() == 0.5 * MathS.pi);
        }
        [TestMethod]
        public void TestPatt8()
        {
            var expr = MathS.Arccotan(x * 3) + MathS.Arctan(x * 3);
            Assert.IsTrue(expr.Simplify() == 0.5 * MathS.pi);
        }
        [TestMethod]
        public void TestPatt9()
        {
            var expr = MathS.Arccotan(x * 3) + MathS.Arctan(x * 6);
            Assert.IsTrue(expr.Simplify() == MathS.Arccotan(3 * x) + MathS.Arctan(6 * x));
        }
        [TestMethod]
        public void TestPatt10()
        {
            var expr = MathS.Arcsin(x * 3) + MathS.Arccos(x * 1);
            Assert.IsTrue(expr.Simplify() == MathS.Arccos(x) + MathS.Arcsin(3 * x));
        }
        [TestMethod]
        public void TestPatt11()
        {
            var expr = ((3 + x) + 4) + x;
            Assert.IsTrue(expr.Simplify() == 7 + 2 * x);
        }
        [TestMethod]
        public void TestPatt12()
        {
            var y = MathS.Var("y");
            var a = MathS.Var("a");
            var b = MathS.Var("b");
            var c = MathS.Var("c");
            var expr = (x * y * a * b * c) / (c * b * a * x * x);
            Assert.IsTrue(expr.Simplify(4) == y / x);
        }

        [TestMethod]
        public void TestFrac1()
        {
            Assert.IsTrue("x / (y / z)".ToEntity().Simplify() == 
                          "x * z / y".ToEntity());
        }

        [TestMethod]
        public void TestFrac2()
        {
            Assert.IsTrue("x / y / z".ToEntity().Simplify() ==
                          "x / (y * z)".ToEntity());
        }

    }
}
