using AngouriMath;
using AngouriMath.Core.Numerix;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests.Core
{
    [TestClass]
    public class Numeric
    {
        [TestMethod]
        public void TestRational1()
        {
            Assert.IsTrue(
                new RationalNumber(3, 4) + new RationalNumber(5, 6) ==
                new RationalNumber(19, 12)
                );
        }

        [TestMethod]
        public void TestRational2()
        {
            Assert.IsTrue(
                new RationalNumber(3, 4) + 1 ==
                new RationalNumber(7, 4)
            );
        }

        [TestMethod]
        public void TestRational3()
        {
            Assert.IsTrue(
                new RationalNumber(7, 4) + new RationalNumber(8, 4) ==
                new RationalNumber(15, 4)
            );
        }

        [TestMethod]
        public void TestInteger()
        {
            Assert.IsTrue(
                Number.Create(3) + Number.Create(5) ==
                new RationalNumber(8)
            );
        }

        [TestMethod]
        public void TestComplex()
        {
            var x = new ComplexNumber(new RealNumber(2), new RealNumber(0.0m));
            var res = x / 0;
            var target = new ComplexNumber(
                new RealNumber(RealNumber.UndefinedState.POSITIVE_INFINITY), 
                new RealNumber(RealNumber.UndefinedState.NAN)
                );
        }

        [TestMethod]
        public void TestWithUndefined1()
        {
            var a = RealNumber.NegativeInfinity();
            var b = RealNumber.PositiveInfinity();
            Assert.IsTrue(a + b == RealNumber.NaN());
        }

        [TestMethod]
        public void TestWithUndefined2()
        {
            var a = RealNumber.NegativeInfinity();
            var b = RealNumber.NegativeInfinity();
            Assert.IsTrue(a + b == RealNumber.NegativeInfinity());
        }

        [TestMethod]
        public void TestWithUndefined3()
        {
            var a = RealNumber.NegativeInfinity();
            var b = RealNumber.NegativeInfinity();
            Assert.IsTrue(a * b == RealNumber.PositiveInfinity());
        }

        [TestMethod]
        public void TestWithUndefined4()
        {
            var a = RealNumber.NegativeInfinity();
            var b = RealNumber.NaN();
            Assert.IsTrue(a * b == RealNumber.NaN());
        }

        [TestMethod]
        public void TestWithUndefined5()
        {
            var a = new RealNumber(4);
            var b = new RealNumber(0.0m);
            Assert.IsTrue(a / b == RealNumber.NaN(), (a / b).ToString());
        }
        [TestMethod]
        public void TestWithUndefined6()
        {
            var a = new RealNumber(4);
            var b = new RationalNumber(0, 6);
            Assert.IsTrue(a / b == RealNumber.NaN(), (a / b).ToString());
        }
        [TestMethod]
        public void TestWithUndefined7()
        {
            var a = Number.Create(4);
            var b = new RealNumber(0.0m);
            Assert.IsTrue(a / b == RealNumber.NaN(), (a / b).ToString());
        }

        [TestMethod]
        public void TestWithUndefined8()
        {
            var x = new RationalNumber(Number.Create(-1), Number.Create(2));
            Assert.IsTrue(x / 0 == RealNumber.NaN(), (x / 0).ToString());
        }

        [TestMethod]
        public void TestWithUndefined9()
        {
            var x = new RationalNumber(Number.Create(-1), Number.Create(2));
            Assert.IsTrue(x / RealNumber.PositiveInfinity() == 0);
        }

        [TestMethod]
        public void TestWithUndefined10()
        {
            var x = new RealNumber(0.5);
            Assert.IsTrue(x / RealNumber.PositiveInfinity() == 0);
        }

        [TestMethod]
        public void TestWithUndefined11()
        {
            Assert.IsTrue(RealNumber.PositiveInfinity() / 5 == RealNumber.PositiveInfinity());
        }

        [TestMethod]
        public void TestWithUndefined12()
        {
            Assert.IsTrue(RealNumber.PositiveInfinity() / -5 == RealNumber.NegativeInfinity());
        }

        [TestMethod]
        public void TestWithUndefined13()
        {
            Assert.IsTrue(RealNumber.PositiveInfinity() / RealNumber.NegativeInfinity() == RealNumber.NaN());
        }

        [TestMethod]
        public void TestWithUndefined14()
        {
            Assert.IsTrue(RealNumber.NegativeInfinity() / 5 == RealNumber.NegativeInfinity());
        }

        [TestMethod]
        public void TestComplexDowncasting()
        {
            Entity x = "x2 + 1/9";
            var roots = x.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
                Assert.IsTrue(root.GetValue().Real.IsRational() && root.GetValue().Imaginary.IsRational());
        }
    }
}
