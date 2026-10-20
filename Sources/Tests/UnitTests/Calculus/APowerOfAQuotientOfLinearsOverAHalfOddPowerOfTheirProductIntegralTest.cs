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
    /// <c>x^j e^(n acoth(a x))/(c - a^2 c x^2)^(K + 1/2)</c>, closed by Liouville's ansatz
    /// <c>e^(n acoth(a x)) P(x)/(c - a^2 c x^2)^(K - 1/2)</c>: the first is Rubi's
    /// <c>e^(n acoth(a x)) (a x - n)/(a c (1 - n^2) sqrt(c - a^2 c x^2))</c>. They were declined.
    /// Rubi's 7.4.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAQuotientOfLinearsOverAHalfOddPowerOfTheirProductIntegralTest
    {
        [Fact]
        public void RubisAnswer()
        {
            var integral = "e^(n*acoth(a*x))/(c - a^2*c*x^2)^(3/2)".ToEntity().Integrate("x").Substitute("C", 0);
            var rubi = "e^(n*acoth(a*x))*(a*x - n)/(a*c*(1 - n^2)*sqrt(c - a^2*c*x^2))".ToEntity();
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.7).Substitute("n", 0.4);
            foreach (var at in new[] { -2.9, -1.4, 1.3, 2.9 })
            {
                var difference = (Pinned(integral) - Pinned(rubi)).Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)difference.RealPart) + Math.Abs((double)difference.ImaginaryPart) < 1e-9, $"{difference} at {at}");
            }
        }

        [Theory]
        [InlineData("e^(n*acoth(a*x))/(c - a^2*c*x^2)^(5/2)")]
        [InlineData("e^(n*acoth(a*x))*x/(c - a^2*c*x^2)^(3/2)")]
        [InlineData("e^(n*acoth(a*x))*x^3/(c - a^2*c*x^2)^(5/2)")]
        [InlineData("e^(n*atanh(a*x))/(c - a^2*c*x^2)^(3/2)")]
        public void ByTheAnsatz(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.7).Substitute("n", 0.4);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // Inside and outside the roots at -1/1.3 and 1/1.3.
            foreach (var at in new[] { -2.9, -1.4, -0.4, 0.3, 0.6, 1.3, 2.9 })
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
