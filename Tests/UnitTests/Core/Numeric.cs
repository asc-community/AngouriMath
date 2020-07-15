using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class Numeric
    {
        [TestMethod]
        public void TestRational1() =>
            Assert.AreEqual(RationalNumber.Create(19, 12),
                RationalNumber.Create(3, 4) + RationalNumber.Create(5, 6));

        [TestMethod]
        public void TestRational2() =>
            Assert.AreEqual(RationalNumber.Create(7, 4), RationalNumber.Create(3, 4) + 1);

        [TestMethod]
        public void TestRational3() =>
            Assert.AreEqual(RationalNumber.Create(15, 4),
                RationalNumber.Create(7, 4) + RationalNumber.Create(8, 4));

        [TestMethod]
        public void TestInteger()
        {
            var actual = RationalNumber.Create(3);
            Assert.IsInstanceOfType(actual, typeof(IntegerNumber));
            Assert.AreEqual(IntegerNumber.Create(3), actual);
        }

        [TestMethod]
        public void TestComplex()
        {
            var x = ComplexNumber.Create(2, 0.0m);
            Assert.IsInstanceOfType(x, typeof(IntegerNumber));
            var res = x / 0;
            Assert.IsInstanceOfType(res, typeof(RealNumber));
            Assert.AreEqual(RealNumber.NaN, res);
        }

        [TestMethod]
        public void TestWithUndefined1()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.PositiveInfinity;
            Assert.AreEqual(RealNumber.NaN, a + b);
        }

        [TestMethod]
        public void TestWithUndefined2()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NegativeInfinity;
            Assert.AreEqual(RealNumber.NegativeInfinity, a + b);
        }

        [TestMethod]
        public void TestWithUndefined3()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NegativeInfinity;
            Assert.AreEqual(RealNumber.PositiveInfinity, a * b);
        }

        [TestMethod]
        public void TestWithUndefined4()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NaN;
            Assert.AreEqual(RealNumber.NaN, a * b);
        }

        [TestMethod]
        public void TestWithUndefined5()
        {
            var a = RealNumber.Create(4);
            var b = RealNumber.Create(0.0m);
            Assert.AreEqual(RealNumber.NaN, a / b);
        }
        [TestMethod]
        public void TestWithUndefined6()
        {
            var a = RealNumber.Create(4);
            var b = RationalNumber.Create(0, 6);
            Assert.IsInstanceOfType(b, typeof(IntegerNumber));
            Assert.AreEqual(RealNumber.NaN, a / b);
        }
        [TestMethod]
        public void TestWithUndefined7()
        {
            var a = IntegerNumber.Create(4);
            var b = RealNumber.Create(0);
            Assert.AreEqual(RealNumber.NaN, a / b);
        }

        [TestMethod]
        public void TestWithUndefined8()
        {
            var x = RationalNumber.Create(-1, 2);
            Assert.IsNotInstanceOfType(x, typeof(IntegerNumber));
            Assert.AreEqual(RealNumber.NaN, x / 0);
        }

        [TestMethod]
        public void TestWithUndefined9()
        {
            var x = RationalNumber.Create(-1, 2);
            var result = x / RealNumber.PositiveInfinity;
            Assert.IsInstanceOfType(result, typeof(IntegerNumber));
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestWithUndefined10()
        {
            var x = RealNumber.Create(PeterO.Numbers.EDecimal.FromDouble(0.5));
            Assert.IsInstanceOfType(x, typeof(RationalNumber));
            Assert.IsNotInstanceOfType(x, typeof(IntegerNumber));
            var result = x / RealNumber.PositiveInfinity;
            Assert.IsInstanceOfType(result, typeof(IntegerNumber));
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void TestWithUndefined11() =>
            Assert.AreEqual(RealNumber.PositiveInfinity, RealNumber.PositiveInfinity / 5);
        [TestMethod]
        public void TestWithUndefined12() =>
            Assert.AreEqual(RealNumber.NegativeInfinity, RealNumber.PositiveInfinity / -5);
        [TestMethod]
        public void TestWithUndefined13() =>
            Assert.AreEqual(RealNumber.NaN, RealNumber.PositiveInfinity / RealNumber.NegativeInfinity);

        [TestMethod]
        public void TestWithUndefined14() =>
            Assert.AreEqual(RealNumber.NegativeInfinity, RealNumber.NegativeInfinity / 5);

        [TestMethod]
        public void TestComplexDowncasting()
        {
            Entity x = "x2 + 1/9";
            var roots = x.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
            {
                Assert.IsInstanceOfType(root, typeof(NumberEntity));
                var number = (NumberEntity)root;
                Assert.IsInstanceOfType(number.Value.Real, typeof(RationalNumber));
                Assert.IsInstanceOfType(number.Value.Imaginary, typeof(RationalNumber));
            }
        }
    }
}
