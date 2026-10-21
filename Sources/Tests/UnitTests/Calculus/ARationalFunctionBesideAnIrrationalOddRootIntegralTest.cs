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
    /// A rational function with numbers beside a cube root of a linear, split over the rationals
    /// first where a linear below meets the root at an irrational value: Rubi's 1.1.2.4 and 1.1.3.4
    /// under the substitution of a power of x, which ran past the budget over the cube root of 4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionBesideAnIrrationalOddRootIntegralTest
    {
        [Theory]
        [InlineData("1/(x^3*(1 - x^2)^(1/3)*(3 + x^2))")]
        [InlineData("1/(x^5*(1 - x^2)^(1/3)*(3 + x^2))")]
        [InlineData("1/(x^3*(1 - x^2)^(1/3)*(3 + x^2)^2)")]
        [InlineData("1/(x^5*(1 - x^2)^(1/3)*(3 + x^2)^2)")]
        [InlineData("1/(x^4*(1 - x^3)^(4/3)*(1 + x^3))")]
        public void SplitOverTheRationalsFirst(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e;
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -0.9, -0.4, 0.3, 0.6, 0.8, 1.7, 2.9 })
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
