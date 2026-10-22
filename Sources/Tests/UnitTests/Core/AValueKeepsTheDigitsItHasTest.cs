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
    /// A numerical evaluation rounds a value onto a small rational only within half the working digits, as it
    /// rounds onto an integer, where it rounded within 1e-16 whatever the precision: at two thousand digits the
    /// derivative of a right antiderivative of <c>1/sqrt(a + i a sinh(c + d x))</c> lost its imaginary part that
    /// way. The residuals of exact cancellation are still exact, and so are numbers from a double or a decimal.
    /// The reference is mpmath's at 120 digits.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1944">#1944</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class AValueKeepsTheDigitsItHasTest
    {
        private static void AssertClose(string reference, EDecimal actual)
        {
            var expected = EDecimal.FromString(reference);
            var error = actual.Subtract(expected, EContext.ForPrecision(200)).Abs().Divide(expected.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString("1E-40")), $"{actual} where it is {expected}");
        }

        [Theory]
        [InlineData("1/3 + 1e-30")]
        [InlineData("0.333333333333333333333")]
        public void AValueNearARationalIsNotThatRational(string expression)
            => Assert.IsType<Real>(expression.EvalNumerical());

        [Fact]
        public void AMachineNumberIsTheRationalItIsNearToItsOwnDigits()
        {
            // A double or a decimal carries its type's digits, not the working precision's, and is read as
            // before: within 1e-16 of a small rational, it is that rational.
            Assert.Equal((Entity)Rational.Create(1, 3), (Entity)(1m / 3));
            Assert.Equal((Entity)Rational.Create(1, 3), (Entity)(1.0 / 3));
            Assert.Equal((Entity)Rational.Create(1, 4), (Entity)0.25);
        }

        [Theory]
        [InlineData("sin(pi)", "0")]
        [InlineData("cos(pi/2)", "0")]
        [InlineData("e^(i*pi) + 1", "0")]
        [InlineData("sin(100*pi)", "0")]
        [InlineData("sqrt(2)*sqrt(3) - sqrt(6)", "0")]
        [InlineData("sin(pi/6)", "1/2")]
        [InlineData("sqrt(2)^2/3", "2/3")]
        [InlineData("gamma(1/2)^2/pi", "1")]
        public void AResidualOfExactArithmeticIsStillExact(string expression, string exact)
            => Assert.Equal(exact.ToEntity(), expression.EvalNumerical());

        [Fact]
        public void WithinHalfTheDigitsAValueIsTheRationalItIsNear()
            => Assert.Equal(Rational.Create(1, 3), "1/3 + 1e-60".EvalNumerical());

        [Fact]
        public void TheDerivativeOfAnAntiderivativeAtALargeHyperbolicArgumentIsTheIntegrand()
        {
            const int digits = 2000;
            var integrand = "1/sqrt(a + i*a*sinh(c + d*x))".ToEntity();
            var derivative = integrand.Integrate("x").Differentiate("x");
            Entity Pin(Entity e) => e.Substitute("a", Rational.Create(21, 10)).Substitute("c", Rational.Create(19, 10))
                .Substitute("d", Rational.Create(17, 10)).Substitute("x", Rational.Create(227283, 1250));
            using var _ = MathS.Settings.DecimalPrecisionContext.Set(new EContext(digits, ERounding.HalfEven, -100000, 100000, false));
            var value = Assert.IsType<Complex>(Pin(derivative).EvalNumerical());
            AssertClose("2.01854640568606010538100888350636282143172892E-68", value.RealPart.EDecimal);
            AssertClose("-2.01854640568606010538100888350636282143172892E-68", value.ImaginaryPart.EDecimal);
        }
    }
}
