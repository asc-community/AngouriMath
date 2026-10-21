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
    /// The half-angle tangent makes of <c>1/(1 + sin(x)^5)</c> a rational function whose logarithms
    /// want a residue of degree eight, which only the second pass writes, as a sum over roots; the
    /// first pass searched every split of it for thirty seconds before declining. Rubi's 4.1.7.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumOverRootsLeftToTheSecondPassIntegralTest
    {
        private static string AnsweredAndCheckedBack(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            // b > c, so that the root is real.
            Entity Pinned(Entity e) => e.Substitute("b", 2.7).Substitute("c", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.9, -0.7, 0.3, 0.8, 1.3, 2.1 })
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
            return text;
        }

        // A residue of degree above two, by a factor no closed form reads: the second pass's sum over
        // roots, as before.
        [Theory]
        [InlineData("1/(1 + sin(x)^5)")]
        [InlineData("1/(1 - sin(x)^5)")]
        [InlineData("sin(x)/(1 + sin(x)^5)")]
        [InlineData("1/(1 + sin(x)^3)")]
        [InlineData("1/(1 + sin(2*x)^5)")]
        public void LeavesTheSumOverRootsToTheSecondPass(string integrand)
            => Assert.Contains("sum(", AnsweredAndCheckedBack(integrand));

        // In the half-angle tangent `1/(1 + cos(x)^3)` is `(1 + t^2)^2/(1 + 3 t^4)`, over a binomial
        // the first pass writes at its roots: it is still answered in closed form.
        [Theory]
        [InlineData("1/(1 + cos(x)^3)")]
        public void KeepsTheClosedFormOfAnEvenBlock(string integrand)
            => Assert.DoesNotContain("sum(", AnsweredAndCheckedBack(integrand));
    }
}
