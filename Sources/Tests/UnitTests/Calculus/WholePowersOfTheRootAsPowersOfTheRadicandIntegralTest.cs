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
    /// Under <c>u = sqrt(x)</c>, a whole power of u that is a multiple of the root's order written as a
    /// power of the radicand, <c>(x^(1/2))^10 = x^5</c>: spelled <c>((x^(1/2))^5)^2</c>, the answer to Rubi's
    /// 1.1.3.2 <c>1/(x^(7/2) sqrt(a + b x^5))</c> could not be read at a negative x, its root on the cut,
    /// and kept <c>provided x &gt;= 0</c>, where the integrand is real beyond <c>-(a/b)^(1/5)</c> and the
    /// answer holds there.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class WholePowersOfTheRootAsPowersOfTheRadicandIntegralTest
    {
        [Theory]
        [InlineData("1/(x^(7/2)*sqrt(a + b*x^5))")]
        [InlineData("1/(x^(17/2)*sqrt(1 + x^5))")]
        [InlineData("sqrt(a*x^13)/sqrt(1 + x^5)")]
        [InlineData("1/(sqrt(x)*sqrt(-3 + 2*x))")]
        public void HoldsOnBothSides(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            // Beyond -(a/b)^(1/5), about -1.13, the integrand is real as a product of two imaginary roots,
            // and `1/(sqrt(x) sqrt(2 x - 3))` is real for every negative x the same way.
            foreach (var at in new[] { -2.3, -1.4, 0.4, 1.3, 2.2 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.False(got.IsNaN, $"d/dx of the antiderivative of {integrand} has no value at x = {at}, where the integrand is {want}");
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
