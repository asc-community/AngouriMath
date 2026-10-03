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
    /// A rational function with complex numbers among its coefficients, written as its real and
    /// imaginary parts over the denominator times its conjugate, which is real: Rubi's
    /// <c>(a + i a tan)^n</c> family under the tangent substitution.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back at real points, both parts compared, since the integrand is
    /// complex there; the symbols are pinned after the integral is taken.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ComplexCoefficientRationalIntegralTest
    {
        private static readonly double[] Points = { -0.7, 0.3, 0.9, 1.3, 2.2 };

        private static void DifferentiatesBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in new[] { ("a", 1.3), ("A", 0.7), ("B", -0.4), ("c", 0.2), ("d", 1.1) })
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var compared = 0;
            foreach (var at in Points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>Numbers off the real line among the coefficients.</summary>
        [Theory]
        [InlineData("1/((1 + i*x)^2*(1 + x^2))")]
        [InlineData("x^3/((1 + i*x)^4*(1 + x^2))")]
        [InlineData("(2 - i*x)/((x - i)^2*(x^2 + 4))")]
        public void WithNumbers(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A symbol common to the coefficients, <c>a + i a x</c>, goes out with the constants.
        /// </summary>
        [Theory]
        [InlineData("1/((a + i*a*x)*(1 + x^2))")]
        [InlineData("x^3/((a + i*a*x)^4*(1 + x^2))")]
        public void WithACommonSymbol(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// Rubi's 4.3.2.1 and 4.3.3.1, which the tangent substitution makes rational functions with
        /// complex coefficients.
        /// </summary>
        [Theory]
        [InlineData("1/(a + i*a*tan(x))")]
        [InlineData("1/(a + i*a*tan(x))^2")]
        [InlineData("tan(x)^3/(a + i*a*tan(x))^4")]
        [InlineData("(A + B*tan(x))/(a + i*a*tan(x))^2")]
        [InlineData("1/(a + i*a*tan(c + d*x))^3")]
        public void APowerOfAPlusIATangent(string integrand) => DifferentiatesBack(integrand);

        /// <summary>
        /// A power of a quotient of linears with the imaginary unit in it, beside a power of x the
        /// substitution for it does not take down to something answered: declined, and within a
        /// minute. The scaling of the variable simplified it with the symbol beside the imaginary
        /// unit, and that ran past two minutes before the decline.
        /// </summary>
        [Theory]
        [InlineData("((1 + i*a*x)/(1 - i*a*x))^(-5/4)/x^2")]
        [InlineData("((1 + i*a*x)/(1 - i*a*x))^(-5/4)/x^3")]
        public void AnImaginaryMobiusPowerIsDeclinedWithinAMinute(string integrand)
        {
            var integrating = System.Threading.Tasks.Task.Run(() => integrand.ToEntity().Integrate("x"));
            Assert.True(integrating.Wait(TimeSpan.FromSeconds(60)), $"{integrand} was not settled within a minute");
        }
    }
}
