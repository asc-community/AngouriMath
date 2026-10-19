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
    /// A power of a quotient of two linears beside whole powers of them, under
    /// <c>t = (p + q x)/(r + s x)</c>: <c>e^(n acoth(a x))/(c - a^2 c x^2)^m</c> arrives as
    /// <c>((a x + 1)/(a x - 1))^(n/2)</c> over the power of the quadratic, and is a polynomial in
    /// <c>t</c> times a power of it. For <c>m</c> past 1 it was declined. Rubi's 7.4.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfAQuotientOfLinearsBesideThemIntegralTest
    {
        [Theory]
        [InlineData("e^(n*acoth(a*x))/(c - a^2*c*x^2)^2")]
        [InlineData("e^(n*acoth(a*x))/(c - a^2*c*x^2)^3")]
        [InlineData("e^(n*atanh(a*x))/(c - a^2*c*x^2)^2")]
        [InlineData("((a*x + 1)/(a*x - 1))^(n/2)/((a*x + 1)^2*(a*x - 1)^2)")]
        [InlineData("((x + 2)/(3*x - 1))^(1/3)/((x + 2)*(3*x - 1)^2)")]
        public void UnderTheQuotient(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
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
