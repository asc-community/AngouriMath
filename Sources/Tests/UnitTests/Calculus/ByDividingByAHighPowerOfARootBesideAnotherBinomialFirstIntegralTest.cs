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
    /// A power below -1 of a cube root of a binomial beside a high power of x and a second binomial,
    /// divided by the root at the question asked. Rubi's 1.1.3.4 <c>1/(x^9 (a + b x^3)^(4/3) (c + d x^3))</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ByDividingByAHighPowerOfARootBesideAnotherBinomialFirstIntegralTest
    {
        [Theory]
        [InlineData("1/(x^9*(a + b*x^3)^(4/3)*(c + d*x^3))")]
        [InlineData("1/(x^6*(a + b*x^3)^(4/3)*(c + d*x^3))")]
        public void DividedByTheRootAtTheQuestionAsked(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 8000, $"{text.Length} characters of answer for {integrand}");
            // Exact pins: with decimals the answer's coefficients, up to c^45, came out NaN in the check.
            Entity Pinned(Entity e) => e.Substitute("a", "13/10".ToEntity()).Substitute("b", "7/10".ToEntity()).Substitute("c", "2/5".ToEntity()).Substitute("d", "3/10".ToEntity());
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
