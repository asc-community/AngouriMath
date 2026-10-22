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

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// The upper incomplete gamma function <c>Gamma(a, z) = int_z^oo t^(a - 1) e^(-t) dt</c>, written <c>gamma(a, z)</c>,
    /// with the principal branch of <c>z^a</c>. References from mpmath's <c>gammainc</c> at 130 digits: by the series
    /// near the origin, by the continued fraction past it, at the poles of <c>Gamma(a)</c>, and on the cut.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/2075">#2075</a>
    /// </summary>
    [Trait("Area", "Convenience")]
    public sealed class IncompleteGammaTest
    {
        private static void AssertClose(string reference, EDecimal actual)
        {
            var expected = EDecimal.FromString(reference);
            if (expected.IsZero)
            {
                Assert.True(actual.IsZero, $"{actual} where it is 0");
                return;
            }
            var error = actual.Subtract(expected, EContext.ForPrecision(200)).Abs().Divide(expected.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString("1E-55")), $"{actual} where it is {expected}");
        }

        [Theory]
        [InlineData("gamma(1/3, 2)", "0.0681364144414591361659959249099704570952527539499344393170888", "0")]
        [InlineData("gamma(-1/2, 3)", "0.00677613600177021229377514699942294840656536635691110578503547", "0")]
        // E_1(1), and a pole of Gamma(a) handled by the recurrence.
        [InlineData("gamma(0, 1)", "0.219383934395520273677163775460121649031047293406908207577979", "0")]
        [InlineData("gamma(-3, 1/2)", "1.32194260686678451978439959176024018549333450108404397738537", "0")]
        // By the continued fraction, and far below the floor at zero.
        [InlineData("gamma(5/2, 40)", "1.11555920556816836584315244570307218006275734693311260218414E-15", "0")]
        [InlineData("gamma(7, 300)", "3.82936658212129710001498193653594875603891571118487941125183E-116", "0")]
        // On the cut, from above.
        [InlineData("gamma(1/2, -3)", "1.77245385090551602729816748334114518279754945612238712821381", "-14.6261713840190916850829290647365438829669286131782239586917")]
        [InlineData("gamma(-1, -20)", "1.35739289356707492202577927702104389515074891049797988869358E+6", "3.14159265358979323846264338327950288419716939937510582097494")]
        [InlineData("gamma(100, 10)", "9.3326215443944152681699238856266700490715968264381621468593E+155", "0")]
        [InlineData("gamma(1/2 + i, 2 + 3i)", "-0.0106186868505695828645674618919283599789635032416382778377351", "-0.026574511726313175577690285230774989836262216835255934709977")]
        public void IsItsValue(string expression, string real, string imaginary)
        {
            var value = Assert.IsAssignableFrom<Complex>(expression.EvalNumerical());
            AssertClose(real, value.RealPart.EDecimal);
            AssertClose(imaginary, value.ImaginaryPart.EDecimal);
        }

        [Theory]
        [InlineData("gamma(1, x)", "e ^ (-x)")]
        [InlineData("gamma(3, x)", "2 * e ^ (-x) * (1 + x + x ^ 2 / 2)")]
        [InlineData("gamma(1/2, x)", "sqrt(pi) * erfc(sqrt(x))")]
        [InlineData("gamma(5/2, 0)", "gamma(5/2)")]
        public void IsElementaryWhereItCanBe(string expression, string elementary)
            => Assert.Equal(elementary.ToEntity().InnerSimplified, expression.ToEntity().InnerSimplified);

        [Fact]
        public void TheDerivativeInTheLowerLimitIsTheIntegrandThere()
        {
            var derivative = "gamma(1/3, x^2)".ToEntity().Differentiate("x");
            var at = derivative.Substitute("x", Rational.Create(3, 2)).EvalNumerical();
            var expected = "-(x^2)^(-2/3) * e^(-x^2) * 2 * x".ToEntity().Substitute("x", Rational.Create(3, 2)).EvalNumerical();
            AssertClose(expected.Stringize(), Assert.IsType<Real>(at).EDecimal);
        }

        [Fact]
        public void AVaryingParameterHasNoDerivativeHere()
            => Assert.IsType<Entity.Derivativef>("gamma(x, 2)".ToEntity().Differentiate("x").InnerSimplified);

        [Fact]
        public void IsParsedAndPrinted()
        {
            var entity = "gamma(1/3, x)".ToEntity();
            Assert.IsType<Entity.IncompleteGammaf>(entity);
            Assert.Equal("gamma(1/3, x)", entity.Stringize());
            Assert.Equal(@"\Gamma\left(\frac{1}{3}, x\right)", entity.Latexize());
            Assert.Contains("sympy.uppergamma(sympy.Rational(1, 3), x)", MathS.ToSympyCode(entity));
            Assert.Equal(entity, entity.Stringize().ToEntity());
            Assert.IsType<Entity.Factorialf>("gamma(x)".ToEntity());
        }
    }
}
