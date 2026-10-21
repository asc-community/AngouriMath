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
    /// A root over the root of a quadratic, or over the roots of two linears with one slope,
    /// integrated in the variable that rationalises the roots inside it: Rubi's 1.3.2
    /// <c>sqrt(1 - x^2 + x sqrt(x^2 - 1))</c> under <c>t = x + sqrt(x^2 - 1)</c>, and
    /// <c>sqrt(-x + sqrt(x) sqrt(1 + x))/sqrt(1 + x)</c>, which Rubi does not answer. Compared where
    /// the integrand is real, which is where the answer is claimed.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ANestedRootByRationalisingTheRootsInItIntegralTest
    {
        [Theory]
        [InlineData("sqrt(1 - x^2 + x*sqrt(x^2 - 1))", new[] { 1.1, 1.43, 1.9, 2.3, 3.17, 4.6 })]
        [InlineData("sqrt(-x + sqrt(x)*sqrt(1 + x))/sqrt(1 + x)", new[] { 0.1, 0.29, 0.83, 1.43, 3.17, 4.6 })]
        [InlineData("sqrt(1 + x^2 + x*sqrt(1 + x^2))", new[] { -3.17, -1.43, -0.61, 0.29, 1.43, 3.17 })]
        [InlineData("1/sqrt(1 + x^2 + x*sqrt(1 + x^2))", new[] { -3.17, -1.43, -0.61, 0.29, 1.43, 3.17 })]
        [InlineData("sqrt(x^2 + x*sqrt(x^2 - 1))", new[] { -3.17, -2.2, -1.43, 1.43, 2.2, 3.17 })]
        [InlineData("sqrt(x + sqrt(x)*sqrt(1 + x))", new[] { 0.1, 0.29, 0.83, 1.43, 3.17, 4.6 })]
        public void InTheVariableThatRationalisesTheRootsInside(string integrand, double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 2000, $"{text.Length} characters of answer for {integrand}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in points)
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                if (want.IsNaN || Math.Abs((double)want.ImaginaryPart) > 1e-12)
                    continue;
                compared++;
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
            Assert.True(compared >= 5, $"only {compared} points could be compared for {integrand}");
        }
    }
}
