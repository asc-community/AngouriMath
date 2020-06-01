using System;
using System.Collections.Generic;
using System.Text;
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
            Assert.IsTrue(a / b == RealNumber.PositiveInfinity());
        }
        [TestMethod]
        public void TestWithUndefined6()
        {
            var a = new RealNumber(4);
            var b = new RationalNumber(0, 6);
            Assert.IsTrue(a / b == RealNumber.PositiveInfinity());
        }
        [TestMethod]
        public void TestWithUndefined7()
        {
            var a = Number.Create(4);
            var b = new RealNumber(0.0m);
            Assert.IsTrue(a / b == RealNumber.PositiveInfinity());
        }

        [TestMethod]
        public void TestWithUndefined8()
        {
            var x = new RationalNumber(Number.Create(-1), Number.Create(2));
            Assert.IsTrue(x / 0 == RealNumber.NegativeInfinity());
        }
    }
}
