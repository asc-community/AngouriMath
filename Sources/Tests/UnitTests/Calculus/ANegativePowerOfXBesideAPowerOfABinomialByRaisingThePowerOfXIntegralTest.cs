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
    /// A power of x below -1 beside a power of a binomial <c>a + b x^n</c>, its pole at zero taken
    /// out by the reduction that raises the power of x by n. Beside a root of the binomial and a
    /// rational function of <c>x^n</c>, Rubi's 1.1.3.4 <c>1/(x^3 (1 - x^3)^(2/3) (1 + x^3))</c>, whose
    /// terms apart are not elementary; over a high whole power of a symbolic one, 1.1.2.2
    /// <c>1/(x^6 (a + b x^2)^10)</c>, which ran past the budget in the partial fractions.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ANegativePowerOfXBesideAPowerOfABinomialByRaisingThePowerOfXIntegralTest
    {
        [Theory]
        [InlineData("1/(x^3*(1-x^3)^(2/3)*(1+x^3))")]
        [InlineData("1/(x^3*(1+x^3)^(2/3)*(1-x^3))")]
        public void BesideARootOfTheBinomial(string integrand)
            => DifferentiatesBack(integrand, new[] { -1.7, -0.6, 0.3, 0.7, 1.4, 2.2 });

        [Theory]
        [InlineData("1/(x^6*(a + b*x^2)^10)")]
        [InlineData("1/(x^7*(a + b*x^2)^10)")]
        [InlineData("1/(x^5*(a + b*x^2)^10)")]
        public void OverAHighWholePowerOfTheBinomial(string integrand)
            => DifferentiatesBack(integrand, new[] { -1.9, -0.8, 0.3, 0.7, 1.4, 2.2 });

        private static void DifferentiatesBack(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
