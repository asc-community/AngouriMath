using AngouriMath;
using AngouriMath.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class DerivativeTest
    {
        static VariableEntity x = MathS.Var("x");
        [TestMethod]
        public void Test1()
        {
            var func = MathS.Sqr(x) + 2 * x + 1;
            Assert.IsTrue(func.Derive(x).Simplify() == 2 * (x + 1));
        }
        [TestMethod]
        public void TestSin()
        {
            var func = MathS.Sin(x);
            Assert.IsTrue(func.Derive(x).Simplify() == MathS.Cos(x));
        }
        [TestMethod]
        public void TestCosCustom()
        {
            var func = MathS.Cos(MathS.Pow(x, 3));
            var expected = -3 * MathS.Sin(MathS.Pow(x, 3)) * MathS.Sqr(x);
            var actual = func.Derive(x).Simplify();
            Assert.IsTrue(expected.ToString() == actual.ToString());
        }
        [TestMethod]
        public void TestPow()
        {
            var func = MathS.Pow(MathS.e, x);
            Assert.IsTrue(func.Derive(x).Simplify() == func);
        }
        [TestMethod]
        public void TestPoly()
        {
            var func = MathS.Pow(x, 4);
            Assert.IsTrue(func.Derive(x).Simplify() == 4 * MathS.Pow(x, 3));
        }
        [TestMethod]
        public void TestCusfunc()
        {
            var func = MathS.Sin(x).Pow(2);
            Assert.IsTrue(func.Derive(x).Simplify() == MathS.Sin(2 * x));
        }
        [TestMethod]
        public void TestTan()
        {
            var func = MathS.Tan(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == 2 * MathS.Pow(MathS.Cos(2 * x), -2));
        }
        [TestMethod]
        public void TestCoTan()
        {
            var func = MathS.Cotan(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == -2 * MathS.Pow(MathS.Sin(2 * x), -2));
        }
        [TestMethod]
        public void TestArc1()
        {
            var func = MathS.Arcsin(x);
            Assert.IsTrue(func.Derive(x).Simplify() == MathS.Pow(1 - MathS.Sqr(x), -0.5));
        }
        [TestMethod]
        public void TestArc2()
        {
            var func = MathS.Arcsin(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == 2 * MathS.Pow(1 - 4 * MathS.Sqr(x), -0.5));
        }
        [TestMethod]
        public void TestArc3()
        {
            var func = MathS.Arccos(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == -2 * MathS.Pow(1 - 4 * MathS.Sqr(x), -0.5));
        }
        [TestMethod]
        public void TestArc4()
        {
            var func = MathS.Arctan(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == 2 / (1 + 4 * MathS.Sqr(x)));
        }
        [TestMethod]
        public void TestArc5()
        {
            var func = MathS.Arccotan(2 * x);
            Assert.IsTrue(func.Derive(x).Simplify() == -2 / (1 + 4 * MathS.Sqr(x)));
        }
    }
}
