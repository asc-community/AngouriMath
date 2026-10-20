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
    /// <c>sqrt(a + b x^2 + c x^4)/(d + e x^4)</c> with <c>c d + a e = 0</c> and <c>a c &lt; 0</c>, a
    /// rational function of <c>u = x/sqrt(a + b x^2 + c x^4)</c>: an arctangent and an inverse
    /// hyperbolic tangent. Welz's <c>sqrt(1 - x^4)/(1 + x^4)</c> was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back inside the trinomial's real zeros and outside them, where
    /// the integrand is imaginary, at two values of <c>p</c>.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TheRootOfATrinomialInTheSquareOverABinomialInTheFourthPowerIntegralTest
    {
        [Theory]
        [InlineData("sqrt(1 - x^4)/(1 + x^4)")]
        [InlineData("sqrt(1 + p*x^2 - x^4)/(1 + x^4)")]
        [InlineData("sqrt(2 - x^4)/(2 + x^4)")]
        // a < 0 < c, and a multiple above and below.
        [InlineData("3*sqrt(-1 + x^2 + 2*x^4)/(5 + 10*x^4)")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            foreach (var p in new[] { 1.3, -2.5 })
            {
                var derivative = integral.Substitute("C", 0).Substitute("p", p).Differentiate("x");
                var original = integrand.ToEntity().Substitute("p", p);
                var compared = 0;
                foreach (var at in new[] { -1.7, -0.6, -0.2, 0.3, 0.55, 1.4, 2.6 })
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    if (want.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, p = {p}, where the integrand is {want}");
                }
                Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand} at p = {p}");
            }
        }
    }
}
