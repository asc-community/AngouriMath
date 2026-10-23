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
        // Far into the left half-plane, by the lower function's series of one sign there: each part to its own size,
        // the real part of the first being Gamma(1/2).
        [InlineData("gamma(1/2, -143)", "1.772453850905516027298167483341145182797549456122387128213808", "-1.066534514082282609180752885388079901236847457667946132456796E+61")]
        [InlineData("gamma(-5/2, -60)", "-0.9453087204829418812256893244486107641586930432652731350473642", "7.256677574613207107369164321252421746598199910922310065217093E+19")]
        [InlineData("gamma(1/2, -100 + 5i)", "2.607638471329857822055492550981639715386578693808752929552738E+42", "-7.00326380520744724063748887540346529570189148404061756494028E+41")]
        // Past the continued fraction's threshold on the cut, with the part that Gamma(a) is the whole of.
        [InlineData("gamma(1/2, -300)", "1.772453850905516027298167483341145182797549456122387128213808", "-1.123338927148883734198084176745306173925451665800872666587958E+129")]
        [InlineData("gamma(3/2, -250)", "0.8862269254527580136490837416705725913987747280611935641069039", "5.911793698136623273673640330606776777031254613823953153197099E+109")]
        [InlineData("gamma(-3, -300)", "2.430575190291251015731676197614951916724433231125758635038235E+120", "0.5235987755982988730771072305465838140328615665625176368291574")]
        [InlineData("gamma(0, -300)", "-6.496482508088665789025691894934247269238089700566375975103098E+127", "-3.141592653589793238462643383279502884197169399375105820974945")]
        [InlineData("gamma(100, 10)", "9.3326215443944152681699238856266700490715968264381621468593E+155", "0")]
        [InlineData("gamma(1/2 + i, 2 + 3i)", "-0.0106186868505695828645674618919283599789635032416382778377351", "-0.026574511726313175577690285230774989836262216835255934709977")]
        public void IsItsValue(string expression, string real, string imaginary)
        {
            var value = Assert.IsAssignableFrom<Complex>(expression.EvalNumerical());
            AssertClose(real, value.RealPart.EDecimal);
            AssertClose(imaginary, value.ImaginaryPart.EDecimal);
        }

        /// <summary>
        /// On the cut, for an exact order of odd denominator below 10, with the real root the power takes there:
        /// <c>Gamma(a) - z^a g(a, z)</c> with <c>g(a, z) = sum_k (-z)^k/(k! (a + k))</c> (DLMF 8.7.3) and <c>z^a</c> real. The
        /// references are that sum at 90 digits; the last is past the continued fraction's threshold.
        /// </summary>
        [Theory]
        [InlineData("gamma(1/3, -2)", "10.212552064243320672871765046103178972863292927441879189946367")]
        [InlineData("gamma(2/3, -4)", "-36.897696051078160810409733567406007863692044903007209488356707")]
        [InlineData("gamma(2/5, -17/15)", "-1.6376459092577785167485577680520445343763746015778210809826328")]
        [InlineData("gamma(1/3, -143)", "4.669432864644246857974117704263949442598436311750677119332775E+60")]
        [InlineData("gamma(-1/3, -300)", "9.71532351293767903599760645811774844613945799010165863950178E+126")]
        public void OnTheCutItTakesThePowersRealRoot(string expression, string real)
        {
            var value = Assert.IsAssignableFrom<Complex>(expression.EvalNumerical());
            AssertClose(real, value.RealPart.EDecimal);
            Assert.True(value.ImaginaryPart.EDecimal.IsZero, $"{value}");
        }

        /// <summary>
        /// And so its derivative, whose power takes the same root, is the derivative of its value there: with the principal
        /// branch it was the derivative turned by <c>e^(-2 i pi/3)</c>.
        /// </summary>
        [Fact]
        public void OnTheCutTheDerivativeIsTheDerivativeOfTheValue()
        {
            var at = Rational.Create(-2, 1);
            var h = EDecimal.FromString("1E-30");
            var above = "gamma(1/3, z)".ToEntity().Substitute("z", Real.Create(EDecimal.FromInt32(-2).Add(h))).EvalNumerical();
            var below = "gamma(1/3, z)".ToEntity().Substitute("z", Real.Create(EDecimal.FromInt32(-2).Subtract(h))).EvalNumerical();
            var difference = Assert.IsType<Real>(above - below).EDecimal.Divide(h.Multiply(2), EContext.ForPrecision(100));
            var derivative = Assert.IsType<Real>("gamma(1/3, z)".ToEntity().Differentiate("z").Substitute("z", at).EvalNumerical()).EDecimal;
            var error = difference.Subtract(derivative, EContext.ForPrecision(100)).Abs().Divide(derivative.Abs(), EContext.ForPrecision(10));
            Assert.True(error.LessThan(EDecimal.FromString("1E-25")), $"the value changes at {difference} where the derivative is {derivative}");
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
