using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class DerivativeTest
    {
        static readonly Entity.Variable x = MathS.Var(nameof(x));
        [TestMethod]
        public void Test1()
        {
            var func = MathS.Sqr(x) + 2 * x + 1;
            var derived = func.Derive(x);
            Assert.AreEqual(2 + 2 * x, derived.Simplify());
        }
        [TestMethod]
        public void TestSin()
        {
            var func = MathS.Sin(x);
            Assert.AreEqual(MathS.Cos(x), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestCosCustom()
        {
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -3 * (MathS.Sin(MathS.Pow(x, 3)) * MathS.Sqr(x));
            var actual = func.Derive(x).Simplify();
            Assert.AreEqual(expected, actual);
        }
        [TestMethod]
        public void TestPow()
        {
            var func = MathS.Pow(MathS.e, x);
            Assert.AreEqual(func, func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestPoly()
        {
            var func = MathS.Pow(x, 4);
            Assert.AreEqual(4 * MathS.Pow(x, 3), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestCusfunc()
        {
            var func = MathS.Sin(x).Pow(2);
            Assert.AreEqual(MathS.Sin(2 * x), func.Derive(x).Simplify(3));
        }
        [TestMethod]
        public void TestTan()
        {
            var func = MathS.Tan(2 * x);
            Assert.AreEqual(2 / MathS.Pow(MathS.Cos(2 * x), 2), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestCoTan()
        {
            var func = MathS.Cotan(2 * x);
            Assert.AreEqual(-2 / MathS.Pow(MathS.Sin(2 * x), 2), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestArc1()
        {
            var func = MathS.Arcsin(x);
            Assert.AreEqual(1 / MathS.Sqrt(1 - MathS.Sqr(x)), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestArc2()
        {
            var func = MathS.Arcsin(2 * x);
            Assert.AreEqual(2 / MathS.Sqrt(1 + (-4) * MathS.Sqr(x)), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestArc3()
        {
            var func = MathS.Arccos(2 * x);
            Assert.AreEqual((-2) / MathS.Sqrt(1 + (-4) * MathS.Sqr(x)), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestArc4()
        {
            var func = MathS.Arctan(2 * x);
            Assert.AreEqual(2 / (1 + 4 * MathS.Sqr(x)), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestArc5()
        {
            var func = MathS.Arccotan(2 * x);
            Assert.AreEqual(-2 / (1 + 4 * MathS.Sqr(x)), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestNaN()
        {
            var func = MathS.Numbers.Create(double.NaN);
            Assert.AreEqual(MathS.Numbers.Create(double.NaN), func.Derive(x).Simplify());
        }
        [TestMethod]
        public void TestNaN2()
        {
            var func = MathS.Pow(21, MathS.Numbers.Create(double.NaN));
            Assert.AreEqual(MathS.Numbers.Create(double.NaN), func.Derive(x).Simplify());
        }

        [TestMethod]
        public void TestDerOverDer1()
        {
            var func = MathS.Derivative("x + 2", x);
            var derFunc = func.Derive(x);
            Assert.AreEqual(MathS.Derivative("x + 2", x, "1 + 1"), derFunc);
        }

        [TestMethod]
        public void TestDerOverDer2()
        {
            var func = MathS.Derivative("x + 2", "y");
            var derFunc = func.Derive(x);
            Assert.AreEqual(MathS.Derivative(func, x), derFunc);
        }
    }
}
