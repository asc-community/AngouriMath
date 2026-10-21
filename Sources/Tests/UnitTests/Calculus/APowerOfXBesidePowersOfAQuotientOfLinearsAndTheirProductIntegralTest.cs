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
    /// <c>x^j T^alpha Q^beta</c>, a power of a quotient of linears beside a power of a multiple of
    /// their product, where the antiderivative is <c>T^alpha Q^(beta + 1)</c> times a polynomial of
    /// degree <c>j - 1</c>: <c>e^(n artanh(a x)) x^2 (c - a^2 c x^2)^(-1 - n^2/2)</c> was left
    /// unevaluated, and the rest ran past the budget. Rubi's 7.3.6.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXBesidePowersOfAQuotientOfLinearsAndTheirProductIntegralTest
    {
        [Theory]
        [InlineData("e^(n*atanh(a*x))*x^2*(c - a^2*c*x^2)^(-1 - n^2/2)")]
        [InlineData("e^(atanh(a*x)/2)*x^2/(c - a^2*c*x^2)^(9/8)")]
        [InlineData("e^(6*atanh(a*x))*x^2/(c - a^2*c*x^2)^19")]
        [InlineData("e^(5*atanh(a*x))*x^2/(c - a^2*c*x^2)^(27/2)")]
        [InlineData("x^2/(e^(5*atanh(a*x))*(c - a^2*c*x^2)^(27/2))")]
        [InlineData("x*((x + 2)/(3*x - 1))^(1/3)*(3*x^2 + 5*x - 2)^(-8/15)")]
        public void AsTheQuotientAndTheQuadraticTimesAPolynomial(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.7).Substitute("n", 0.9);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            // On both sides of the roots, -0.77 and 0.77 for a = 1.3, and of 1/3 and -2.
            foreach (var at in new[] { -2.9, -1.4, -0.4, 0.2, 0.6, 1.3, 2.9 })
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
