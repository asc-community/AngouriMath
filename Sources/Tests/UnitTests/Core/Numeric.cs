//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;
using static AngouriMath.Entity.Set;
using AngouriMath.Extensions;
using System;

namespace AngouriMath.Tests.Core
{
    public sealed class Numeric
    {
        [Fact]
        public void TestRational1() =>
            Assert.Equal(Rational.Create(19, 12),
                Rational.Create(3, 4) + Rational.Create(5, 6));

        [Fact]
        public void TestRational2() =>
            Assert.Equal(Rational.Create(7, 4), Rational.Create(3, 4) + 1);

        [Fact]
        public void TestRational3() =>
            Assert.Equal(Rational.Create(15, 4),
                Rational.Create(7, 4) + Rational.Create(8, 4));

        [Fact]
        public void TestInteger()
        {
            var actual = Rational.Create(3);
            Assert.IsType<Integer>(actual);
            Assert.Equal(Integer.Create(3), actual);
        }

        [Fact]
        public void TestComplex1()
        {
            var x = Complex.Create(2, 0.0m);
            Assert.IsType<Integer>(x);
            var res = x / 0;
            Assert.IsType<Real>(res);
            Assert.Equal(Real.NaN, res);
        }
        [Fact]
        public void TestComplex2()
        {
            Complex a = 3;
            Complex b = MathS.i;
            Complex c = a + b;
            Assert.Equal(Complex.Create(3, 1), c);
        }
        [Fact]
        public void TestRationalDowncasting()
        {
            var frac21_10 = Rational.Create(21, 10);
            Assert.IsType<Rational>(frac21_10);
            Assert.Equal(frac21_10, frac21_10.EvalNumerical());
            Assert.Equal(frac21_10, frac21_10.Simplify().EvalNumerical());

            var squared = Entity.Number.Pow(frac21_10, 2);
            Assert.IsType<Rational>(squared);
            Assert.Equal(Rational.Create(441, 100), squared);
            Assert.Equal(squared, squared.EvalNumerical());
            Assert.Equal(squared, squared.Simplify().EvalNumerical());

            var cubed = Entity.Number.Pow(squared, Rational.Create(3, 2));
            Assert.IsType<Rational>(cubed);
            Assert.Equal(Rational.Create(9261, 1000), cubed);
            Assert.Equal(cubed, cubed.EvalNumerical());
            Assert.Equal(cubed, cubed.Simplify().EvalNumerical());

            var ten = cubed + Rational.Create(739, 1000);
            Assert.IsType<Integer>(ten);
            Assert.Equal(Integer.Create(10), ten);
            Assert.Equal(ten, ten.EvalNumerical());
            Assert.Equal(ten, ten.Simplify().EvalNumerical());
        }
        [Fact]
        public void TestComplexDowncasting()
        {
            Entity x = "x2 + 1/9";
            var roots = x.SolveEquation("x");
            if (roots is not FiniteSet finite)
                throw new Exception("Mee?");
            foreach (var root in finite)
            {
                var number = Assert.IsType<Complex>(root);
                Assert.True(Assert.IsType<Integer>(number.RealPart).EInteger.IsZero);
                var im = Assert.IsType<Rational>(number.ImaginaryPart).ERational;
                Assert.Equal(1, im.UnsignedNumerator);
                Assert.Equal(3, im.Denominator);
            }
        }

        [Fact]
        public void TestWithUndefined1()
        {
            var a = Real.NegativeInfinity;
            var b = Real.PositiveInfinity;
            Assert.Equal(Real.NaN, a + b);
        }

        [Fact]
        public void TestWithUndefined2()
        {
            var a = Real.NegativeInfinity;
            var b = Real.NegativeInfinity;
            Assert.Equal(Real.NegativeInfinity, a + b);
        }

        [Fact]
        public void TestWithUndefined3()
        {
            var a = Real.NegativeInfinity;
            var b = Real.NegativeInfinity;
            Assert.Equal(Real.PositiveInfinity, a * b);
        }

        [Fact]
        public void TestWithUndefined4()
        {
            var a = Real.NegativeInfinity;
            var b = Real.NaN;
            Assert.Equal(Real.NaN, a * b);
        }

        [Fact]
        public void TestWithUndefined5()
        {
            var a = Real.Create(4);
            var b = Real.Create(0.0m);
            Assert.Equal(Real.NaN, a / b);
        }
        [Fact]
        public void TestWithUndefined6()
        {
            var a = Real.Create(4);
            var b = Rational.Create(0, 6);
            Assert.IsType<Integer>(b);
            Assert.Equal(Real.NaN, a / b);
        }
        [Fact]
        public void TestWithUndefined7()
        {
            var a = Integer.Create(4);
            var b = Real.Create(0);
            Assert.Equal(Real.NaN, a / b);
        }

        [Fact]
        public void TestWithUndefined8()
        {
            var x = Rational.Create(-1, 2);
            Assert.IsType<Rational>(x);
            Assert.Equal(Real.NaN, x / 0);
        }

        [Fact]
        public void TestWithUndefined9()
        {
            var x = Rational.Create(-1, 2);
            var result = x / Real.PositiveInfinity;
            Assert.IsType<Integer>(result);
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestWithUndefined10()
        {
            var x = Real.Create(PeterO.Numbers.EDecimal.FromDouble(0.5));
            Assert.IsType<Rational>(x);
            var result = x / Real.PositiveInfinity;
            Assert.IsType<Integer>(result);
            Assert.Equal(0, result);
        }

        [Fact]
        public void TestWithUndefined11() =>
            Assert.Equal(Real.PositiveInfinity, Real.PositiveInfinity / 5);
        [Fact]
        public void TestWithUndefined12() =>
            Assert.Equal(Real.NegativeInfinity, Real.PositiveInfinity / -5);
        [Fact]
        public void TestWithUndefined13() =>
            Assert.Equal(Real.NaN, Real.PositiveInfinity / Real.NegativeInfinity);

        [Fact]
        public void TestWithUndefined14() =>
            Assert.Equal(Real.NegativeInfinity, Real.NegativeInfinity / 5);

        [Fact]
        public void TestWithUndefined15() =>
            Assert.Equal(Real.NaN, Real.NegativeInfinity / 0);

        [Fact]
        public void TestWithUndefined16() =>
            Assert.Equal(Real.NaN, Real.PositiveInfinity / 0);

        [Fact]
        public void TestWithUndefined17() =>
            Assert.Equal(Real.NaN, Real.NaN / 0);

        [Fact]
        public void TestWithUndefined18() =>
            Assert.Equal(Real.NaN, Real.Create(MathS.DecimalConst.pi) / 0);

        [Fact]
        public void TestWithUndefined19() =>
            Assert.Equal(Real.NaN, Complex.PosPosInfinity / 0);

        [Fact]
        public void TestWithUndefined20() =>
            Assert.Equal(Real.NaN, Complex.PosNegInfinity / 0);

        [Fact]
        public void TestWithUndefined21() =>
            Assert.Equal(Real.NaN, Complex.NegPosInfinity / 0);

        [Fact]
        public void TestWithUndefined22() =>
            Assert.Equal(Real.NaN, Complex.NegNegInfinity / 0);

        [Fact]
        public void TestWithUndefined23() =>
            Assert.Equal(Real.NaN, Complex.ImaginaryOne / 0);

        [Fact]
        public void TestWithUndefined24() =>
            Assert.Equal(Real.NaN, Complex.Create(2.13m, 4.21m) / 0);
        [Fact]
        public void TestImaginaryInfinity1() =>
            Assert.Equal(Complex.Create(0, Real.PositiveInfinity),
                Real.PositiveInfinity * Complex.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity2() =>
            Assert.Equal(Complex.Create(0, Real.NegativeInfinity),
                Real.NegativeInfinity * Complex.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity3() =>
            Assert.Equal(Real.NaN,
                Real.NaN * Complex.ImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity4() =>
            Assert.Equal(Complex.Create(0, Real.PositiveInfinity),
                Complex.ImaginaryOne * Real.PositiveInfinity);
        [Fact]
        public void TestImaginaryInfinity5() =>
            Assert.Equal(Complex.Create(0, Real.NegativeInfinity),
                Complex.ImaginaryOne * Real.NegativeInfinity);
        [Fact]
        public void TestImaginaryInfinity6() =>
            Assert.Equal(Real.NaN,
                Complex.ImaginaryOne * Real.NaN);
        [Fact]
        public void TestImaginaryInfinity7() =>
            Assert.Equal(Complex.Create(0, Real.NegativeInfinity),
                Real.PositiveInfinity * Complex.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity8() =>
            Assert.Equal(Complex.Create(0, Real.PositiveInfinity),
                Real.NegativeInfinity * Complex.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity9() =>
            Assert.Equal(Real.NaN,
                Real.NaN * Complex.MinusImaginaryOne);
        [Fact]
        public void TestImaginaryInfinity10() =>
            Assert.Equal(Complex.Create(0, Real.NegativeInfinity),
                Complex.MinusImaginaryOne * Real.PositiveInfinity);
        [Fact]
        public void TestImaginaryInfinity11() =>
            Assert.Equal(Complex.Create(0, Real.PositiveInfinity),
                Complex.MinusImaginaryOne * Real.NegativeInfinity);
        [Fact]
        public void TestImaginaryInfinity12() =>
            Assert.Equal(Real.NaN,
                Complex.MinusImaginaryOne * Real.NaN);
        [Fact]
        public void TestImaginaryInfinity13() =>
            Assert.Equal(Real.NegativeInfinity,
                Complex.Create(0, Real.PositiveInfinity) * Complex.Create(0, Real.PositiveInfinity));
        [Fact]
        public void TestImaginaryInfinity14() =>
            Assert.Equal(Real.PositiveInfinity,
                Complex.Create(0, Real.PositiveInfinity) * Complex.Create(0, Real.NegativeInfinity));
        [Fact]
        public void TestImaginaryInfinity15() =>
            Assert.Equal(Real.PositiveInfinity,
                Complex.Create(0, Real.NegativeInfinity) * Complex.Create(0, Real.PositiveInfinity));
        [Fact]
        public void TestImaginaryInfinity16() =>
            Assert.Equal(Real.NegativeInfinity,
                Complex.Create(0, Real.NegativeInfinity) * Complex.Create(0, Real.NegativeInfinity));

        [Theory]
        [InlineData(1, 0, 1, 0)]
        [InlineData(0, 1, 0, 1)]
        [InlineData(0, 2, 0, 1)]
        [InlineData(2, 0, 1, 0)]
        public void TestSignum(float argReal, float argImaginary,
            float expectedReal, float expectedImaginary)
            => Assert.Equal(
            Complex.Create(expectedReal, expectedImaginary),
            Complex.Signum(Complex.Create(argReal, argImaginary))
                );

        [Theory]
        [InlineData("1 / (a - 1)", "-1 / (1 - a)")]
        [InlineData("(a - 1) / (1 - b) - (1 - a) / (b - 1)", "0")]
        [InlineData("sin(x)2 + cos(x)2", "1")]
        public void TestAreNumericallyEqual(string one, string another)
        {
            var left = one.ToEntity();
            var right = another.ToEntity();
            Assert.True(MathS.UnsafeAndInternal.AreEqualNumerically(left, right));
        }

        [Theory]
        [InlineData("1 / (a - 1)", "-1 / (1 - a) + 1")]
        [InlineData("(a - 1) x", "a")]
        public void TestAreNumericallyNotEqual(string one, string another)
        {
            var left = one.ToEntity();
            var right = another.ToEntity();
            Assert.False(MathS.UnsafeAndInternal.AreEqualNumerically(left, right));
        }
        
        
        
    }
}
