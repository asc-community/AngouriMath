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
    /// <c>(x + s)/((x^2 + s x + s^2) (a + b x^3)^(1/3))</c>, which is
    /// <c>(s^2 - x^2)/((s^3 - x^3) (a + b x^3)^(1/3))</c>: a logarithm and two arctangents. Welz's
    /// <c>(1 + x)/((1 + x + x^2) (a + b x^3)^(1/3))</c> was declined with the symbols in it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back on either side of the binomial's real zero and of
    /// <c>x = s</c>, at signs of <c>a</c>, <c>b</c> and <c>a + b s^3</c> both ways.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ALinearOverTheQuadraticFactorOfADifferenceOfCubesBesideACubeRootIntegralTest
    {
        [Theory]
        [InlineData("(1 + x)/((1 + x + x^2)*(a + b*x^3)^(1/3))")]
        [InlineData("(2 + 2*x)/((3 + 3*x + 3*x^2)*(a - b*x^3)^(1/3))")]
        // s = -1.
        [InlineData("(x - 1)/((1 - x + x^2)*(a + b*x^3)^(1/3))")]
        [InlineData("(1 + x)/((1 + x + x^2)*(2 + 3*x^3)^(1/3))")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            foreach (var (a, b) in new[] { (1.3, 0.7), (-0.9, 2.1), (0.4, -2.2) })
            {
                var derivative = integral.Substitute("C", 0).Substitute("a", a).Substitute("b", b).Differentiate("x");
                var original = integrand.ToEntity().Substitute("a", a).Substitute("b", b);
                var compared = 0;
                foreach (var at in new[] { -2.3, -1.2, -0.4, 0.3, 0.9, 1.1, 1.9, 3.2 })
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    if (want.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, a = {a}, b = {b}, where the integrand is {want}");
                }
                Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand} at a = {a}, b = {b}");
            }
        }
    }
}
