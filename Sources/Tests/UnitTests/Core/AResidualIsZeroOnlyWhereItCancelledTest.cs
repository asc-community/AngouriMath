//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A number an evaluation produces is rounded onto zero only as a residual of the cancellation that produced it:
    /// a difference below half the working digits of its larger term. A product, a quotient, a power, a root, an
    /// exponential or a modulus cancels nothing and is never zero by size. Below half the working digits outright,
    /// <c>e^(-155)</c> was 0 at a hundred digits, a modulus near 1e-136 was 0 and a quotient by it NaN, and the
    /// derivative of a right antiderivative at a large hyperbolic argument was NaN at a hundred digits and wrong at
    /// three hundred. References from mpmath at 120 digits.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1944">#1944</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class AResidualIsZeroOnlyWhereItCancelledTest
    {
        private static void AssertClose(string reference, EDecimal actual)
        {
            var expected = EDecimal.FromString(reference);
            var error = actual.Subtract(expected, EContext.ForPrecision(200)).Abs().Divide(expected.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString("1E-40")), $"{actual} where it is {expected}");
        }

        [Theory]
        [InlineData("e^(-155)", "4.83454163805333583428273878250224602960536796E-68")]
        [InlineData("e^(-120)", "7.66764807372199963243420837527582768233983151E-53")]
        [InlineData("e^(-60) * e^(-60)", "7.66764807372199963243420837527582768233983151E-53")]
        // e^(-200) e^(i pi): the imaginary part is the residual of a sine, at the modulus's scale.
        [InlineData("e^(-200 + i*pi)", "-1.38389652673673753064868145697908468540304758233947720939393E-87")]
        public void AProductOrAPowerIsNotZeroBySize(string expression, string reference)
            => AssertClose(reference, Assert.IsType<Real>(expression.EvalNumerical()).EDecimal);

        [Fact]
        public void AQuotientKeepsItsDigitsAndItsImaginaryPart()
        {
            var value = Assert.IsType<Complex>("1/(10^67*sqrt(2) + 10^67*sqrt(2)*i)".EvalNumerical());
            AssertClose("3.53553390593273762200422181052424519642417969E-68", value.RealPart.EDecimal);
            AssertClose("-3.53553390593273762200422181052424519642417969E-68", value.ImaginaryPart.EDecimal);
        }

        [Fact]
        public void TheRootOfASmallNegativeIsImaginary()
        {
            var value = Assert.IsType<Complex>("sqrt(-e^(-300))".EvalNumerical());
            Assert.True(value.RealPart.EDecimal.IsZero, $"{value}");
            AssertClose("7.17509597316441041983269290720898818840868827424412900583245E-66", value.ImaginaryPart.EDecimal);
        }

        [Theory]
        [InlineData("sin(pi)", "0")]
        [InlineData("cos(pi/2)", "0")]
        [InlineData("e^(i*pi) + 1", "0")]
        [InlineData("sin(100*pi)", "0")]
        [InlineData("sqrt(2)*sqrt(3) - sqrt(6)", "0")]
        [InlineData("(1 + sqrt(2)*i) * (1 - sqrt(2)*i)", "3")]
        // A difference of e^(-155) and a value 1e-90 of it larger: below half the digits of the terms.
        [InlineData("e^(-155) - e^(-155) * (1 + 10^(-90))", "0")]
        public void AResidualOfCancellationIsStillZero(string expression, string exact)
            => Assert.Equal(exact.ToEntity(), expression.EvalNumerical());

        [Theory]
        [InlineData(200)]
        [InlineData(300)]
        public void TheDerivativeOfAnAntiderivativeAtALargeHyperbolicArgumentIsTheIntegrand(int digits)
        {
            // The derivative's terms cancel in their sixty-eighth digit, so below twice that it reads as a residual.
            var integrand = "1/sqrt(a + i*a*sinh(c + d*x))".ToEntity();
            var derivative = integrand.Integrate("x").Differentiate("x");
            Entity Pin(Entity e) => e.Substitute("a", Rational.Create(21, 10)).Substitute("c", Rational.Create(19, 10))
                .Substitute("d", Rational.Create(17, 10)).Substitute("x", Rational.Create(227283, 1250));
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(new EContext(digits, ERounding.HalfEven, -100000, 100000, false));
            var value = Assert.IsType<Complex>(Pin(derivative).EvalNumerical());
            AssertClose("2.01854640568606010538100888350636282143172892E-68", value.RealPart.EDecimal);
            AssertClose("-2.01854640568606010538100888350636282143172892E-68", value.ImaginaryPart.EDecimal);
        }

        [Theory]
        [InlineData(50)]
        [InlineData(100)]
        public void TheIntegrandAtALargeHyperbolicArgumentHasItsValue(int digits)
        {
            var integrand = "1/sqrt(21/10 + i*21/10*sinh(19/10 + 17/10*227283/1250))".ToEntity();
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(new EContext(digits, ERounding.HalfEven, -100000, 100000, false));
            var value = Assert.IsType<Complex>(integrand.EvalNumerical());
            AssertClose("2.01854640568606010538100888350636282143172892E-68", value.RealPart.EDecimal);
            AssertClose("-2.01854640568606010538100888350636282143172892E-68", value.ImaginaryPart.EDecimal);
        }
    }
}
