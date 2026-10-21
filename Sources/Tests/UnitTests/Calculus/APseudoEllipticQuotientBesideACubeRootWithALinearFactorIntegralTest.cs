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
    /// A quotient beside the cube root of a polynomial with a linear factor, or of its square,
    /// that is a constant times <c>T'/(1 - T^3)</c> for <c>T = m L/y</c> or <c>m y/L</c>. With
    /// symbols in the radicand, Welz's three were declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APseudoEllipticQuotientBesideACubeRootWithALinearFactorIntegralTest
    {
        [Theory]
        [InlineData("1/(x*((-1+x)*(q-2*q*x+x^2))^(1/3))")]
        [InlineData("(2-(1+k)*x)/(((1-x)*x*(1-k*x))^(1/3)*(1-(1+k)*x))")]
        [InlineData("(1-k*x)/((1+(-2+k)*x)*((1-x)*x*(1-k*x))^(2/3))")]
        public void InTheCubeRootOverALinearFactor(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("q", 1.3).Substitute("k", 0.4);
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
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
