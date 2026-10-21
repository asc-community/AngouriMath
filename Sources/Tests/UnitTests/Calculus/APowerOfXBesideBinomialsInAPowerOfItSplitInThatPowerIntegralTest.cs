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
    /// A power of x that is not whole beside binomials in <c>x^n</c>, split into partial fractions in
    /// <c>w = x^n</c> with the power of x kept beside each term. Rubi's 1.1.2.4 ran past the budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBesideBinomialsInAPowerOfItSplitInThatPowerIntegralTest
    {
        [Theory]
        [InlineData("x^(9/2)/((a + b*x^2)*(c + d*x^2)^2)")]
        [InlineData("x^(5/2)/((a + b*x^2)^2*(c + d*x^2)^2)")]
        [InlineData("x^(7/2)/((a + b*x^2)^2*(c + d*x^2)^3)")]
        [InlineData("sqrt(x)/((a + b*x^2)^2*(c + d*x^2)^3)")]
        [InlineData("1/(x^(3/2)*(a + b*x^2)^2*(c + d*x^2)^2)")]
        public void SplitOverTheBinomialsInXToTheN(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 8000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { 0.3, 0.8, 1.3, 1.9, 2.6, 3.1 })
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
