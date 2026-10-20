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
    /// Cube roots of two linears whose powers add up to a whole number beside a whole power of a
    /// third linear, under <c>t = (a + b x)^(1/3)/(c + d x)^(1/3)</c> with the coefficients of the
    /// rational function named: written as the substitution leaves them they ran past the budget.
    /// Rubi's 1.1.1.3. Points on both sides of the roots' zeros, where the cube roots are complex.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class CubeRootsOfTwoLinearsBesideAWholePowerOfAThirdIntegralTest
    {
        [Theory]
        [InlineData("(a + b*x)^(1/3)*(c + d*x)^(2/3)/(k + f*x)")]
        [InlineData("(a + b*x)^(1/3)*(c + d*x)^(2/3)/(k + f*x)^2")]
        [InlineData("(a + b*x)^(1/3)*(c + d*x)^(2/3)/(k + f*x)^4")]
        [InlineData("(a + b*x)^(1/3)/((c + d*x)^(1/3)*(k + f*x)^3)")]
        [InlineData("1/((a + b*x)^(1/3)*(c + d*x)^(2/3)*(k + f*x)^2)")]
        [InlineData("1/((a + b*x)^(1/3)*(c + d*x)^(2/3)*(k + f*x)^3)")]
        [InlineData("(a + b*x)^(4/3)/((c + d*x)^(4/3)*(k + f*x)^2)")]
        [InlineData("(1 + x)^(1/3)*(2 - x)^(2/3)/(k + f*x)^2")]
        [InlineData("(a + b*x)^(1/3)*(c + d*x)^(5/3)/(k + f*x)^3")]
        public void UnderTheQuotientOfTheRoots(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 8000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4)
                .Substitute("d", 0.3).Substitute("k", 0.9).Substitute("f", 1.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -3.1, -1.7, -0.3, 0.4, 1.3, 2.9 })
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
