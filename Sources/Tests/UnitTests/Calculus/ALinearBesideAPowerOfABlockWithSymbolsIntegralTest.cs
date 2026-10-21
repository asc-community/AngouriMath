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
    /// A power of a linear with symbols in it beside a power of a block with symbols in it, split at
    /// the linear's root named while the series and the rest are computed, the rest written over the
    /// powers of the block's base. With the root carried as <c>-d/e</c>, <c>1/((d + e x)^2 (a + c x^4)^3)</c>
    /// was 203,730 characters after 27 seconds, and the cube 243,601 after 73. Rubi's 1.3.1:568, 569.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALinearBesideAPowerOfABlockWithSymbolsIntegralTest
    {
        [Theory]
        [InlineData("1/((d + k*x)^2*(a + c*x^4)^3)")]
        [InlineData("1/((d + k*x)^3*(a + c*x^4)^3)")]
        [InlineData("1/((d + k*x)*(a + c*x^4)^3)")]
        [InlineData("1/((d + k*x)^3*(a + c*x^4)^2)")]
        [InlineData("1/((d + k*x)^2*(a + b*x^3)^3)")]
        public void SplitAtTheNamedRoot(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 16000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.8).Substitute("c", 0.6).Substitute("d", 0.9).Substitute("k", 1.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.1, -1.1, -0.3, 0.4, 1.2, 2.3 })
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
