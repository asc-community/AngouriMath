using AngouriMath;
using AngouriMath.Core.Numerix;
using Xunit;

namespace UnitTests.Core
{
    public class Numeric
    {
        [Fact]
        public void TestRational1() =>
            Assert.Equal(RationalNumber.Create(19, 12),
                RationalNumber.Create(3, 4) + RationalNumber.Create(5, 6));

        [Fact]
        public void TestRational2() =>
            Assert.Equal(RationalNumber.Create(7, 4), RationalNumber.Create(3, 4) + 1);

        [Fact]
        public void TestRational3() =>
            Assert.Equal(RationalNumber.Create(15, 4),
                RationalNumber.Create(7, 4) + RationalNumber.Create(8, 4));

        [Fact]
        public void TestInteger()
        {
            var actual = RationalNumber.Create(3);
            Assert.IsType<IntegerNumber>(actual);
            Assert.Equal(IntegerNumber.Create(3), actual);
        }

        [Fact]
        public void TestComplex1()
        {
            var x = ComplexNumber.Create(2, 0.0m);
            Assert.IsType<IntegerNumber>(x);
            var res = x / 0;
            Assert.IsType<RealNumber>(res);
            Assert.Equal(RealNumber.NaN, res);
        }
        [Fact]
        public void TestComplex2()
        {
            ComplexNumber a = 3;
            ComplexNumber b = MathS.i;
            ComplexNumber c = a + b;
            Assert.Equal(ComplexNumber.Create(3, 1), c);
        }
        [Fact]
        public void TestRationalDowncasting()
        {
            var frac21_10 = RationalNumber.Create(21, 10);
            Assert.IsType<RationalNumber>(frac21_10);
            Assert.Equal(frac21_10, frac21_10.Eval());
            Assert.Equal(frac21_10, frac21_10.Simplify().Eval());

            var squared = Entity.Number.Pow(frac21_10, 2);
            Assert.IsType<RationalNumber>(squared);
            Assert.Equal(RationalNumber.Create(441, 100), squared);
            Assert.Equal(squared, squared.Eval());
            Assert.Equal(squared, squared.Simplify().Eval());

            var cubed = Entity.Number.Pow(squared, RationalNumber.Create(3, 2));
            Assert.IsType<RationalNumber>(cubed);
            Assert.Equal(RationalNumber.Create(9261, 1000), cubed);
            Assert.Equal(cubed, cubed.Eval());
            Assert.Equal(cubed, cubed.Simplify().Eval());

            var ten = cubed + RationalNumber.Create(739, 1000);
            Assert.IsType<IntegerNumber>(ten);
            Assert.Equal(IntegerNumber.Create(10), ten);
            Assert.Equal(ten, ten.Eval());
            Assert.Equal(ten, ten.Simplify().Eval());
        }
        [Fact]
        public void TestComplexDowncasting()
        {
            Entity x = "x2 + 1/9";
            var roots = x.SolveEquation("x");
            foreach (var root in roots.FiniteSet())
            {
                Assert.IsType<ComplexNumber>(root);
                var number = (ComplexNumber)root;
                Assert.IsType<RationalNumber>(number.Real);
                Assert.IsType<RationalNumber>(number.Imaginary);
            }
        }

        [Fact]
        public void TestWithUndefined1()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.PositiveInfinity;
            Assert.Equal(RealNumber.NaN, a + b);
        }

        [Fact]
        public void TestWithUndefined2()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NegativeInfinity;
            Assert.Equal(RealNumber.NegativeInfinity, a + b);
        }

        [Fact]
        public void TestWithUndefined3()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NegativeInfinity;
            Assert.Equal(RealNumber.PositiveInfinity, a * b);
        }

        [Fact]
        public void TestWithUndefined4()
        {
            var a = RealNumber.NegativeInfinity;
            var b = RealNumber.NaN;
            Assert.Equal(RealNumber.NaN, a * b);
        }

        [Fact]
        public void TestWithUndefined5()
        {
            var a = RealNumber.Create(4);
            var b = RealNumber.Create(0.0m);
            Assert.Equal(RealNumber.NaN, a / b);
        }
        [Fact]
        public void TestWithUndefined6()
        {
            var a = RealNumber.Create(4);
            var b = RationalNumber.Create(0, 6);
            Assert.IsType<IntegerNumber>(b);
            Assert.Equal(RealNumber.NaN, a / b);
        }
        [Fact]
        public void TestWithUndefined7()
        {
            var a = IntegerNumber.Create(4);
            var b = RealNumber.Create(0);
            Assert.Equal(RealNumber.NaN, a / b);
        }

        [Fact]
        public void TestWithUndefined8()
        {
            var x = RationalNumber.Create(-1, 2);
            Assert.IsType<RationalNumber>(x);
            Assert.Equal(RealNumber.NaN, x / 0);
        }

        [Fact]
        public void TestWithUndefined9()
        {
            var x = RationalNumber.Create(-1, 2);
            var result = x / RealNumber.PositiveInfinity;
            Assert.IsType<IntegerNumber>(result);
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestWithUndefined10()
        {
            var x = RealNumber.Create(PeterO.Numbers.EDecimal.FromDouble(0.5));
            Assert.IsType<RationalNumber>(x);
            var result = x / RealNumber.PositiveInfinity;
            Assert.IsType<IntegerNumber>(result);
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestWithUndefined11() =>
            Assert.Equal(RealNumber.PositiveInfinity, RealNumber.PositiveInfinity / 5);
        [Fact]
        public void TestWithUndefined12() =>
            Assert.Equal(RealNumber.NegativeInfinity, RealNumber.PositiveInfinity / -5);
        [Fact]
        public void TestWithUndefined13() =>
            Assert.Equal(RealNumber.NaN, RealNumber.PositiveInfinity / RealNumber.NegativeInfinity);

        [Fact]
        public void TestWithUndefined14() =>
            Assert.Equal(RealNumber.NegativeInfinity, RealNumber.NegativeInfinity / 5);

        [Fact]
        public void TestWithUndefined15() =>
            Assert.Equal(RealNumber.NaN, RealNumber.NegativeInfinity / 0);

        [Fact]
        public void TestWithUndefined16() =>
            Assert.Equal(RealNumber.NaN, RealNumber.PositiveInfinity / 0);

        [Fact]
        public void TestWithUndefined17() =>
            Assert.Equal(RealNumber.NaN, RealNumber.NaN / 0);

        [Fact]
        public void TestWithUndefined18() =>
            Assert.Equal(RealNumber.NaN, RealNumber.Create(MathS.DecimalConst.pi) / 0);

        [Fact]
        public void TestWithUndefined19() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.PosPosInfinity / 0);

        [Fact]
        public void TestWithUndefined20() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.PosNegInfinity / 0);

        [Fact]
        public void TestWithUndefined21() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.NegPosInfinity / 0);

        [Fact]
        public void TestWithUndefined22() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.NegNegInfinity / 0);

        [Fact]
        public void TestWithUndefined23() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.ImaginaryOne / 0);

        [Fact]
        public void TestWithUndefined24() =>
            Assert.Equal(RealNumber.NaN, ComplexNumber.Create(2.13m, 4.21m) / 0);
        [Fact]
        public void TestImaginaryInfinity1() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                RealNumber.PositiveInfinity * ComplexNumber.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity2() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                RealNumber.NegativeInfinity * ComplexNumber.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity3() =>
            Assert.Equal(RealNumber.NaN,
                RealNumber.NaN * ComplexNumber.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity4() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                ComplexNumber.ImaginaryOne * RealNumber.PositiveInfinity);
        [Fact]
        public void TestImaginaryInfinity5() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                ComplexNumber.ImaginaryOne * RealNumber.NegativeInfinity);
        [Fact]
        public void TestImaginaryInfinity6() =>
            Assert.Equal(RealNumber.NaN,
                ComplexNumber.ImaginaryOne * RealNumber.NaN);
        [Fact]
        public void TestImaginaryInfinity7() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                RealNumber.PositiveInfinity * ComplexNumber.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity8() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                RealNumber.NegativeInfinity * ComplexNumber.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity9() =>
            Assert.Equal(RealNumber.NaN,
                RealNumber.NaN * ComplexNumber.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity10() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.NegativeInfinity),
                ComplexNumber.MinusImaginaryOne * RealNumber.PositiveInfinity);
        [Fact]
        public void TestImaginaryInfinity11() =>
            Assert.Equal(ComplexNumber.Create(0, RealNumber.PositiveInfinity),
                ComplexNumber.MinusImaginaryOne * RealNumber.NegativeInfinity);
        [Fact]
        public void TestImaginaryInfinity12() =>
            Assert.Equal(RealNumber.NaN,
                ComplexNumber.MinusImaginaryOne * RealNumber.NaN);
        [Fact]
        public void TestImaginaryInfinity13() =>
            Assert.Equal(RealNumber.NegativeInfinity,
                ComplexNumber.Create(0, RealNumber.PositiveInfinity) * ComplexNumber.Create(0, RealNumber.PositiveInfinity));
        [Fact]
        public void TestImaginaryInfinity14() =>
            Assert.Equal(RealNumber.PositiveInfinity,
                ComplexNumber.Create(0, RealNumber.PositiveInfinity) * ComplexNumber.Create(0, RealNumber.NegativeInfinity));
        [Fact]
        public void TestImaginaryInfinity15() =>
            Assert.Equal(RealNumber.PositiveInfinity,
                ComplexNumber.Create(0, RealNumber.NegativeInfinity) * ComplexNumber.Create(0, RealNumber.PositiveInfinity));
        [Fact]
        public void TestImaginaryInfinity16() =>
            Assert.Equal(RealNumber.NegativeInfinity,
                ComplexNumber.Create(0, RealNumber.NegativeInfinity) * ComplexNumber.Create(0, RealNumber.NegativeInfinity));
    }
}
