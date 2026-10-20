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
    /// Numeric powers of <c>a ± a sin(y)</c> beside a fractional power of <c>k cos(y)</c>, under
    /// <c>u = sin(y)</c>: <c>(1 + u)^A (1 - u)^B</c> with <c>A + B</c> whole, which the half angle
    /// does not read and the substitution search spent the budget on. Checked where the cosine is
    /// positive, and with <c>k</c> negative where it is negative, both real. Rubi's 4.1.1.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class FractionalPowersOfACosineBesideOnePlusASineIntegralTest
    {
        [Theory]
        [InlineData("(k*cos(c + d*x))^(5/2)*(a + a*sin(c + d*x))^(3/2)")]
        [InlineData("sqrt(k*cos(c + d*x))/sqrt(a + a*sin(c + d*x))")]
        [InlineData("(a + a*sin(c + d*x))^(5/2)/(k*cos(c + d*x))^(13/2)")]
        [InlineData("(k*cos(c + d*x))^(9/2)/(a + a*sin(c + d*x))^(5/2)")]
        [InlineData("1/((k*cos(c + d*x))^(7/2)*(a + a*sin(c + d*x))^(3/2))")]
        [InlineData("(k*cos(c + d*x))^(3/2)*sqrt(a - a*sin(c + d*x))")]
        [InlineData("sin(c + d*x)*sqrt(k*cos(c + d*x))*(a + a*sin(c + d*x))^(3/2)")]
        [InlineData("(k*sin(c + d*x))^(3/2)*(a + a*cos(c + d*x))^(3/2)")]
        public void ThroughTheSine(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 3000, $"{text.Length} characters of answer for {integrand}");
            // y = c + d x stays inside (0, pi/2) at the first pins and inside (pi/2, pi) at the second.
            foreach (var (k, c) in new[] { (0.7, 0.4), (-0.7, 2.2) })
            {
                Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("k", k).Substitute("c", c).Substitute("d", 0.3);
                var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
                var original = Pinned(integrand.ToEntity());
                var compared = 0;
                foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    if (want.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, k = {k}, where the integrand is {want}");
                }
                Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
            }
        }
    }
}
