using AngouriMath;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Algebra
{
    [TestClass]
    public class DerivativeTest
    {
        static readonly VariableEntity x = MathS.Var("x");

        public void AssertEqEntity(Entity actual, Entity target)
            => Assert.AreEqual(target, actual);

        [TestMethod]
        public void Test1()
        {
            var func = MathS.Sqr(x) + 2 * x + 1;
            var derived = func.Derive(x);
            AssertEqEntity(derived.Simplify(), (2 * x + 2).Simplify());
        }
        [TestMethod]
        public void TestSin()
        {
            var func = MathS.Sin(x);
            AssertEqEntity(func.Derive(x).Simplify(), MathS.Cos(x));
        }
        [TestMethod]
        public void TestCosCustom()
        {
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -3 * MathS.Sin(MathS.Pow(x, 3)) * MathS.Sqr(x);
            var actual = func.Derive(x).Simplify();
            AssertEqEntity(expected.ToString(), actual.ToString());
        }
        [TestMethod]
        public void TestPow()
        {
            var func = MathS.Pow(MathS.e, x);
            AssertEqEntity(func.Derive(x).Simplify(), func);
        }
        [TestMethod]
        public void TestPoly()
        {
            var func = MathS.Pow(x, 4);
            AssertEqEntity(func.Derive(x).Simplify(), 4 * MathS.Pow(x, 3));
        }
        [TestMethod]
        public void TestCusfunc()
        {
            var func = MathS.Sin(x).Pow(2);
            AssertEqEntity(func.Derive(x).Simplify(3), MathS.Sin(2 * x));
        }
        [TestMethod]
        public void TestTan()
        {
            var func = MathS.Tan(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), 2 / MathS.Pow(MathS.Cos(2 * x), 2));
        }
        [TestMethod]
        public void TestCoTan()
        {
            var func = MathS.Cotan(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), -2 / MathS.Pow(MathS.Sin(2 * x), 2));
        }
        [TestMethod]
        public void TestArc1()
        {
            var func = MathS.Arcsin(x);
            AssertEqEntity(func.Derive(x).Simplify(), 1 / MathS.Sqrt(1 - MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestArc2()
        {
            var func = MathS.Arcsin(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), 2 / MathS.Sqrt(1 + (-4) * MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestArc3()
        {
            var func = MathS.Arccos(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), (-2) / MathS.Sqrt(1 + (-4) * MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestArc4()
        {
            var func = MathS.Arctan(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), 2 / (1 + 4 * MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestArc5()
        {
            var func = MathS.Arccotan(2 * x);
            AssertEqEntity(func.Derive(x).Simplify(), -2 / (1 + 4 * MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestNaN()
        {
            var func = new NumberEntity(MathS.Numbers.Create(double.NaN));
            AssertEqEntity(func.Derive(x).Simplify(), MathS.Numbers.Create(double.NaN));
        }
        [TestMethod]
        public void TestNaN2()
        {
            var func = MathS.Pow(21, MathS.Numbers.Create(double.NaN));
            AssertEqEntity(func.Derive(x).Simplify(), MathS.Numbers.Create(double.NaN));
        }
    }
}
