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
    /// A linear over the root of a quartic whose symbols are a scale of the variable, read under
    /// <c>x = s t</c> as a numeric quartic that the logarithmic ansatz answers: Rubi's 1.3.2
    /// <c>x/sqrt(-44375 b^4 + 576000 b^3 c x + 576000 b^2 c^2 x^2 + 5308416 c^4 x^4)</c>, and
    /// Bronstein's <c>x/sqrt(x^4 + 10 x^2 - 96 x - 71)</c> with a scale in it, declined before.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ALinearOverTheRootOfAQuarticWithItsSymbolsScaledOutIntegralTest
    {
        [Theory]
        [InlineData("x/sqrt(-44375*b^4 + 576000*b^3*c*x + 576000*b^2*c^2*x^2 + 5308416*c^4*x^4)")]
        [InlineData("x/sqrt(-44375*b^4 + 576000*b^3*x + 576000*b^2*x^2 + 5308416*x^4)")]
        [InlineData("x/sqrt(x^4 + 10*b^2*x^2 - 96*b^3*x - 71*b^4)")]
        [InlineData("x/sqrt(c^4*x^4 + 10*b^2*c^2*x^2 - 96*b^3*c*x - 71*b^4)")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 1500, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("b", -0.7).Substitute("c", 1.3);
            var antiderivative = Pinned(integral.Substitute("C", 0));
            var derivative = antiderivative.Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -4.1, -2.3, -1.6, 1.1, 2.2, 3.7 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || Math.Abs((double)want.ImaginaryPart) > 1e-12)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 4, $"only {compared} points could be compared for {integrand}");
        }
    }
}
