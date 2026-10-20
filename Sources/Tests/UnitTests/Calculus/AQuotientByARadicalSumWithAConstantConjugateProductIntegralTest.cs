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
    /// A quotient by <c>A + B sqrt(R)</c> whose conjugate product is a constant, over the conjugate:
    /// <c>e^asech(a x)</c> arrives as <c>1/(a x) + sqrt(1/(a x)^2 - 1)</c>, whose conjugate product
    /// is 1, and <c>x^m/e^asech(a x)</c> timed out. Rubi's 7.5.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AQuotientByARadicalSumWithAConstantConjugateProductIntegralTest
    {
        [Theory]
        [InlineData("x/e^asech(a*x)")]
        [InlineData("x^3/e^asech(a*x)")]
        [InlineData("1/(e^asech(a*x)*x)")]
        public void OverTheConjugate(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // Where asech(1.3 x) is real: 0 < x < 1/1.3.
            foreach (var at in new[] { 0.05, 0.1, 0.2, 0.35, 0.5, 0.7 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || got.IsNaN)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }

        // By parts against asech or acsch asks a quotient by the same sum; written over the conjugate
        // below the top, these ran past the budget where they are answered at once as written.
        [Theory]
        [InlineData("x^3*asech(a*x)^2")]
        [InlineData("x*(a+b*asech(c*x))^2")]
        [InlineData("x^3*(a+b*acsch(c*x))^2")]
        public void LeavesTheQuotientsByPartsAsksAsWritten(string integrand)
            => Assert.DoesNotContain("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
