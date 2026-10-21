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
    /// Roots of two linears over a quadratic with all three of its terms, under
    /// <c>t = sqrt(d + e x)/sqrt(f + g x)</c> with the coefficients the substitution writes named:
    /// asked as they came, <c>(d + e x)^(3/2)/((a + b x + c x^2) sqrt(f + g x))</c> ran past two
    /// minutes and its three neighbours took 16 to 60 seconds. Rubi's 1.2.1.4:1097-1100.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfALinearOverACompleteQuadraticBesideTwoRootsIntegralTest
    {
        [Theory]
        [InlineData("(d + k*x)^(3/2)/((a + b*x + c*x^2)*sqrt(f + g*x))")]
        [InlineData("(d + k*x)^(1/2)/((a + b*x + c*x^2)*sqrt(f + g*x))")]
        [InlineData("1/((d + k*x)^(1/2)*(a + b*x + c*x^2)*sqrt(f + g*x))")]
        [InlineData("1/((d + k*x)^(3/2)*(a + b*x + c*x^2)*sqrt(f + g*x))")]
        [InlineData("sqrt(d + k*x)*sqrt(f + g*x)/(a + b*x + c*x^2)")]
        public void UnderTheQuotientOfTheRoots(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            // A quadratic with no real root, and one with two.
            foreach (var a in new[] { 1.3, -1.3 })
            {
                Entity Pinned(Entity e) => e.Substitute("a", a).Substitute("b", 0.4).Substitute("c", 0.9)
                    .Substitute("d", 0.7).Substitute("k", 1.1).Substitute("f", 1.5).Substitute("g", 0.6);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                var compared = 0;
                foreach (var at in new[] { -0.5, 0.3, 0.8, 1.3, 2.9, 4.1 })
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
}
