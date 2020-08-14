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
        public void TestComplex1()
        {
            var x = ComplexNumber.Create(2, 0.0m);
            Assert.IsInstanceOfType(x, typeof(IntegerNumber));
            var res = x / 0;
            Assert.IsInstanceOfType(res, typeof(RealNumber));
            Assert.AreEqual(RealNumber.NaN, res);
        }
        [TestMethod]
        public void TestComplex2()
        {
            ComplexNumber a = 3;
            ComplexNumber b = MathS.i;
            ComplexNumber c = a + b;
            Assert.AreEqual(ComplexNumber.Create(3, 1), c);
        }
        [TestMethod]
        public void TestRationalDowncasting()
        {
            var frac21_10 = RationalNumber.Create(21, 10);
            Assert.IsNotInstanceOfType(frac21_10, typeof(IntegerNumber));
            Assert.AreEqual(frac21_10, frac21_10.Eval());
            Assert.AreEqual(frac21_10, frac21_10.Simplify().Eval());

            var squared = Entity.Number.Pow(frac21_10, 2);
            Assert.IsInstanceOfType(squared, typeof(RationalNumber));
            Assert.IsNotInstanceOfType(squared, typeof(IntegerNumber));
            Assert.AreEqual(RationalNumber.Create(441, 100), squared);
            Assert.AreEqual(squared, squared.Eval());
            Assert.AreEqual(squared, squared.Simplify().Eval());

            var cubed = Entity.Number.Pow(squared, RationalNumber.Create(3, 2));
            Assert.IsInstanceOfType(cubed, typeof(RationalNumber));
            Assert.IsNotInstanceOfType(cubed, typeof(IntegerNumber));
            Assert.AreEqual(RationalNumber.Create(9261, 1000), cubed);
            Assert.AreEqual(cubed, cubed.Eval());
            Assert.AreEqual(cubed, cubed.Simplify().Eval());

            var ten = cubed + RationalNumber.Create(739, 1000);
            Assert.IsInstanceOfType(ten, typeof(IntegerNumber));
            Assert.AreEqual(IntegerNumber.Create(10), ten);
            Assert.AreEqual(ten, ten.Eval());
            Assert.AreEqual(ten, ten.Simplify().Eval());
        }
        [TestMethod]
        public void TestComplexDowncasting()
        {
            Entity x = "x2 + 1/9";
            var roots = x.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
            {
                Assert.IsInstanceOfType(root, typeof(ComplexNumber));
                var number = (ComplexNumber)root;
                Assert.IsInstanceOfType(number.Real, typeof(RationalNumber));
                Assert.IsInstanceOfType(number.Imaginary, typeof(RationalNumber));
            }
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
        public void TestWithUndefined15() =>
            Assert.AreEqual(RealNumber.NaN, RealNumber.NegativeInfinity / 0);

        [TestMethod]
        public void TestWithUndefined16() =>
            Assert.AreEqual(RealNumber.NaN, RealNumber.PositiveInfinity / 0);

        [TestMethod]
        public void TestWithUndefined17() =>
            Assert.AreEqual(RealNumber.NaN, RealNumber.NaN / 0);

        [TestMethod]
        public void TestWithUndefined18() =>
            Assert.AreEqual(RealNumber.NaN, RealNumber.Create(MathS.DecimalConst.pi) / 0);

        [TestMethod]
        public void TestWithUndefined19() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.PosPosInfinity / 0);

        [TestMethod]
        public void TestWithUndefined20() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.PosNegInfinity / 0);

        [TestMethod]
        public void TestWithUndefined21() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.NegPosInfinity / 0);

        [TestMethod]
        public void TestWithUndefined22() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.NegNegInfinity / 0);

        [TestMethod]
        public void TestWithUndefined23() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.ImaginaryOne / 0);

        [TestMethod]
        public void TestWithUndefined24() =>
            Assert.AreEqual(RealNumber.NaN, ComplexNumber.Create(2.13m, 4.21m) / 0);
        [TestMethod]
        public void TestImaginaryInfinity1() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                RealNumber.PositiveInfinity * ComplexNumber.ImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity2() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                RealNumber.NegativeInfinity * ComplexNumber.ImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity3() =>
            Assert.AreEqual(RealNumber.NaN,
                RealNumber.NaN * ComplexNumber.ImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity4() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                ComplexNumber.ImaginaryOne * RealNumber.PositiveInfinity);
        [TestMethod]
        public void TestImaginaryInfinity5() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                ComplexNumber.ImaginaryOne * RealNumber.NegativeInfinity);
        [TestMethod]
        public void TestImaginaryInfinity6() =>
            Assert.AreEqual(RealNumber.NaN,
                ComplexNumber.ImaginaryOne * RealNumber.NaN);
        [TestMethod]
        public void TestImaginaryInfinity7() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                RealNumber.PositiveInfinity * ComplexNumber.MinusImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity8() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                RealNumber.NegativeInfinity * ComplexNumber.MinusImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity9() =>
            Assert.AreEqual(RealNumber.NaN,
                RealNumber.NaN * ComplexNumber.MinusImaginaryOne);
        [TestMethod]
        public void TestImaginaryInfinity10() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                ComplexNumber.MinusImaginaryOne * RealNumber.PositiveInfinity);
        [TestMethod]
        public void TestImaginaryInfinity11() =>
            Assert.AreEqual(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                ComplexNumber.MinusImaginaryOne * RealNumber.NegativeInfinity);
        [TestMethod]
        public void TestImaginaryInfinity12() =>
            Assert.AreEqual(RealNumber.NaN,
                ComplexNumber.MinusImaginaryOne * RealNumber.NaN);
        [TestMethod]
        public void TestImaginaryInfinity13() =>
            Assert.AreEqual(RealNumber.NegativeInfinity,
                ComplexNumber.Create(0, RealNumber.PositiveInfinity) * ComplexNumber.Create(0, RealNumber.PositiveInfinity));
        [TestMethod]
        public void TestImaginaryInfinity14() =>
            Assert.AreEqual(RealNumber.PositiveInfinity,
                ComplexNumber.Create(0, RealNumber.PositiveInfinity) * ComplexNumber.Create(0, RealNumber.NegativeInfinity));
        [TestMethod]
        public void TestImaginaryInfinity15() =>
            Assert.AreEqual(RealNumber.PositiveInfinity,
                ComplexNumber.Create(0, RealNumber.NegativeInfinity) * ComplexNumber.Create(0, RealNumber.PositiveInfinity));
        [TestMethod]
        public void TestImaginaryInfinity16() =>
            Assert.AreEqual(RealNumber.NegativeInfinity,
                ComplexNumber.Create(0, RealNumber.NegativeInfinity) * ComplexNumber.Create(0, RealNumber.NegativeInfinity));
    }
}
