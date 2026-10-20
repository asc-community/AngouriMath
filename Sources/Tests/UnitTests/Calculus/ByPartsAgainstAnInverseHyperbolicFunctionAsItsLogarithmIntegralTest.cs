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
    /// By parts against an inverse hyperbolic function beside something that is not a polynomial,
    /// the logarithm the parser writes it as differentiated as the algebraic function it is:
    /// <c>ln(A + sqrt(A^2 + k))' = A'/sqrt(A^2 + k)</c>. These timed out. Rubi's 7.2, 7.5 and 7.6.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ByPartsAgainstAnInverseHyperbolicFunctionAsItsLogarithmIntegralTest
    {
        [Theory]
        [InlineData("x*(a + b*acosh(c*x))/(d + k*x^2)^2", 1.2, 2.9)]
        [InlineData("x*(a + b*acsch(c*x))/(d + k*x^2)^3", 0.2, 2.9)]
        [InlineData("x^3*(a + b*asech(c*x))/(d + k*x^2)^3", 0.05, 0.7)]
        [InlineData("acsch(a + b*x)/x^3", 0.2, 2.9)]
        // Rubi's 7.6.1:130: the remainder's coefficients reach c^294, and read at the pinned symbols
        // they were taken for zero, which dropped two of its three terms.
        [InlineData("(d + k*x^2)^2*(a + b*acsch(c*x))/x^8", 0.2, 2.9)]
        // Rubi's 7.3.4:380 and 7.4.1:82: a power of the function's own quadratic below the bar, which
        // the substitution under the function answers. The rule declines it; asked first, it timed out.
        [InlineData("atanh(a*x)/(1 - a^2*x^2)^3", 0.05, 0.7)]
        [InlineData("acoth(x)/(1 - x^2)^3", 1.2, 2.9)]
        public void ByParts(string integrand, double from, double to)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 1.1).Substitute("d", 0.4).Substitute("k", 0.3);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // Where the function is real.
            for (var i = 0; i < 6; i++)
            {
                var at = from + (to - from) * (i + 0.5) / 6;
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
    }
}
