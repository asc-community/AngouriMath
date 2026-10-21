//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A half-odd power of <c>a + b cosh(y) + c sinh(y)</c> with <c>a^2 = b^2 - c^2</c>, a square of
    /// the half angle turned by a phase, in closed form through <c>T = b sinh(y) + c cosh(y)</c>.
    /// Rubi's 6.7.1.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Compared as complex numbers: with <c>b &lt; |c|</c> the constant is imaginary and so is the
    /// base everywhere. Away from the zero of the base where <c>a</c> is negative.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAHyperbolicCosineAndSinePlusTheirAmplitudeIntegralTest
    {
        [Theory]
        [InlineData("(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^(5/2)", 2.0, 1.0)]
        [InlineData("sqrt(b*cosh(x) + c*sinh(x) - sqrt(b^2 - c^2))", 2.0, 1.0)]
        [InlineData("1/sqrt(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))", 2.0, 1.0)]
        [InlineData("1/(b*cosh(x) + c*sinh(x) - sqrt(b^2 - c^2))^(5/2)", 2.0, -1.0)]
        [InlineData("1/(b*cosh(g + f*x) + c*sinh(g + f*x) + sqrt(b^2 - c^2))^(3/2)", 0.7, 1.1)]
        [InlineData("(b*cosh(g + f*x) + c*sinh(g + f*x) - sqrt(b^2 - c^2))^(3/2)", 0.7, 1.1)]
        [InlineData("1/sqrt(5*cosh(2*x + 1) + 3*sinh(2*x + 1) + 4)", 0.0, 0.0)]
        [InlineData("3*(2*cosh(x) - sinh(x) - sqrt(3))^(-3/2)", 0.0, 0.0)]
        public void IsWrittenThroughTheDerivativeOfItsBase(string integrand, double b, double c)
            => DifferentiatesBack(integrand, b, c);

        // A whole power is left to the exponential substitution, which writes `1/S^2`, `1/S^3` and
        // `1/S^4` shorter than the same recurrence stepped up to `T/(a S)`: answered all the same.
        [Theory]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^2", 2.0, 1.0)]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^3", 2.0, -1.0)]
        [InlineData("1/(b*cosh(x) + c*sinh(x) + sqrt(b^2 - c^2))^4", 0.7, 1.1)]
        [InlineData("1/(b*cosh(g + f*x) + c*sinh(g + f*x) - sqrt(b^2 - c^2))^3", 2.0, 1.0)]
        [InlineData("3*(2*cosh(x) - sinh(x) - sqrt(3))^(-2)", 0.0, 0.0)]
        [InlineData("1/(5*cosh(2*x + 1) + 3*sinh(2*x + 1) + 4)^3", 0.0, 0.0)]
        public void AWholePowerIsAnsweredToo(string integrand, double b, double c)
            => DifferentiatesBack(integrand, b, c);

        private static void DifferentiatesBack(string integrand, double b, double c)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Entity Pinned(Entity e) => e.Substitute("b", b).Substitute("c", c).Substitute("g", 0.4).Substitute("f", 1.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -1.9, -1.1, -0.2, 0.4, 1.1, 1.9 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        /// <summary>
        /// Real and continuous where the integrand is: across the point where <c>T</c> vanishes and
        /// the base does not, and on either side of the base's zero where <c>a</c> is negative.
        /// The difference of the answer against Simpson's rule.
        /// </summary>
        [Theory]
        [InlineData(1.0, -2.0, 1.0)]
        [InlineData(-1.0, 0.1, 2.0)]
        [InlineData(-1.0, -3.0, -1.0)]
        public void IsAContinuousAntiderivativeOnTheRealLine(double sign, double from, double to)
        {
            var root = sign > 0 ? "sqrt(3)" : "-sqrt(3)";
            var integral = $"1/sqrt(2*cosh(x) + sinh(x) + {root})".ToEntity().Integrate("x").Substitute("C", 0);
            Assert.DoesNotContain("integral(", integral.Stringize());
            double Integrand(double t) => 1 / Math.Sqrt(2 * Math.Cosh(t) + Math.Sinh(t) + sign * Math.Sqrt(3));
            const int steps = 400;
            var h = (to - from) / steps;
            var simpson = Integrand(from) + Integrand(to);
            for (var k = 1; k < steps; k++)
                simpson += (k % 2 == 1 ? 4 : 2) * Integrand(from + k * h);
            simpson *= h / 3;
            var difference = integral.Substitute("x", to).EvalNumerical() - integral.Substitute("x", from).EvalNumerical();
            Assert.True(Math.Abs((double)difference.ImaginaryPart) < 1e-9, $"F({to}) - F({from}) is {difference}");
            Assert.True(Math.Abs((double)difference.RealPart - simpson) < 1e-7, $"F({to}) - F({from}) is {difference}, Simpson's rule {simpson}");
        }
    }
}
