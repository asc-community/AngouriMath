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
    /// <c>(x + A - 2 r)/((x - A) sqrt(C))</c> for a symbolic cubic <c>C</c> with a root <c>r</c> midway
    /// between the pole and the zero, where <c>C + m (x - r)^2</c> is a multiple of
    /// <c>(x - r) (x - A)^2</c>: one arctangent of <c>sqrt(m) (x - r)/sqrt(C)</c>. Welz's, which the
    /// search ran out of time on with the symbol in them.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back at points on either side of every root and of the pole,
    /// where the radicand is positive and where it is negative, at two values of <c>a</c>.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class PseudoEllipticQuotientOverTheRootOfACubicWithARootMidwayIntegralTest
    {
        [Theory]
        // m = (1 - a)^2, r = 0.
        [InlineData("(a + x)/((-a + x)*sqrt(a^2*x - (1 + a^2)*x^2 + x^3))")]
        [InlineData("3*(a + x)/((-a + x)*sqrt(a^2*x - (1 + a^2)*x^2 + x^3))")]
        // m = -a^2, r = 1.
        [InlineData("(-2 + a + x)/((-a + x)*sqrt((2 - a)*a*x + (-1 - 2*a + a^2)*x^2 + x^3))")]
        // m = -(2 a - 1)^2, r = a^2/(2 a - 1), the leading coefficient 2 a - 1.
        [InlineData("(-a + (-1 + 2*a)*x)/((-a + x)*sqrt(a^2*x - (-1 + 2*a + a^2)*x^2 + (-1 + 2*a)*x^3))")]
        // m = 2 sqrt(1 + a^2) - 2 a, r = a, the cubic written as a product.
        [InlineData("(-a + x - sqrt(1 + a^2))/((-a + x + sqrt(1 + a^2))*sqrt((-a + x)*(1 + x^2)))")]
        public void IsIntegrated(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            foreach (var a in new[] { 1.7, -0.8 })
            {
                var derivative = integral.Substitute("C", 0).Substitute("a", a).Differentiate("x");
                var original = integrand.ToEntity().Substitute("a", a);
                var compared = 0;
                foreach (var at in new[] { -2.3, -0.6, 0.3, 0.8, 1.4, 2.2, 3.9 })
                {
                    var want = original.Substitute("x", at).EvalNumerical();
                    var got = derivative.Substitute("x", at).EvalNumerical();
                    if (want.IsNaN)
                        continue;
                    compared++;
                    Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                                < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                        $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, a = {a}, where the integrand is {want}");
                }
                Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand} at a = {a}");
            }
        }
    }
}
