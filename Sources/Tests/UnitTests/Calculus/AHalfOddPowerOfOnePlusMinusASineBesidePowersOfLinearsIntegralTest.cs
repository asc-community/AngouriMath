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
    /// A half-odd power of <c>a ± a sin(y)</c> beside whole or half-odd powers of other linears in
    /// the sine, by the half angle at which <c>1 ± sin(y)</c> is a square, asked before the
    /// polynomial term: beneath it the substitution search ran past the budget on Rubi's 4.1.2.1
    /// <c>1/(sqrt(a + a sin(e + f x)) (c + d sin(e + f x))^2)</c> and its kin. The cosine's the same.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHalfOddPowerOfOnePlusMinusASineBesidePowersOfLinearsIntegralTest
    {
        [Theory]
        [InlineData("1/(sqrt(a + a*sin(g + f*x))*(c + d*sin(g + f*x))^2)")]
        [InlineData("1/((a + a*sin(g + f*x))^(3/2)*(c + d*sin(g + f*x))^3)")]
        [InlineData("1/((a + a*sin(x))^(5/2)*(c + d*sin(x)))")]
        [InlineData("(a + a*sin(g + f*x))^(5/2)/(c + d*sin(g + f*x))^2")]
        [InlineData("1/((a + a*sin(g + f*x))^2*sqrt(c - c*sin(g + f*x)))")]
        [InlineData("1/((a + a*sin(x))^3*(c - c*sin(x))^(5/2))")]
        [InlineData("1/((a + a*sin(g + f*x))^(3/2)*(c - c*sin(g + f*x))^(3/2))")]
        [InlineData("1/((a + a*cos(x))^2*sqrt(c - c*cos(x)))")]
        [InlineData("(a + a*cos(x))^(5/2)/(c + d*cos(x))^2")]
        [InlineData("1/((3 + 3*sin(x))^(1/2)*(2 + sin(x))^2)")]
        public void ByTheHalfAngle(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 20000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("c", 0.9).Substitute("d", 0.4)
                .Substitute("g", 0.2).Substitute("f", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9, 4.1 })
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
