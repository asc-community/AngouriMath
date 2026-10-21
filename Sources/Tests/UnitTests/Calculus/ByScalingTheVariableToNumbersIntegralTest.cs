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
    /// An algebraic function whose symbols a scaling of the variable takes out, integrated as a
    /// constant times one with numbers for coefficients. Rubi's 1.3.2 and 1.1.3.3.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ByScalingTheVariableToNumbersIntegralTest
    {
        [Theory]
        [InlineData("1/((c + d*x)*(-c^3 + d^3*x^3)^(1/3))")]
        [InlineData("1/((c + d*x)*(2*c^3 + d^3*x^3)^(1/3))")]
        [InlineData("1/((c + d*x)*(2*c^3 + d^3*x^3)^(2/3))")]
        [InlineData("(k + f*x)/((c + d*x)*(-c^3 + d^3*x^3)^(1/3))")]
        [InlineData("(a + b*x^3)^(1/3)/(a - b*x^3)")]
        [InlineData("1/((1 + 2^(1/3)*x)*(1 + x^3)^(2/3))")]
        public void AConstantTimesAnIntegralWithNumbers(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7).Substitute("c", 0.4).Substitute("d", 0.3).Substitute("k", 0.9).Substitute("f", 1.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -1.2, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
