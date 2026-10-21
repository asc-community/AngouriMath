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
    /// <c>x^m arsech(sqrt(x))</c> and <c>x^m arcsch(sqrt(x))</c> by parts onto <c>x^m/sqrt(1 -+ x)</c>,
    /// with <c>K = sqrt(1 -+ x)/(sqrt(x) sqrt(1/x -+ 1))</c>, which is 1 or -1, beside the remainder.
    /// <c>x^3 arsech(sqrt(x))</c> ran out of time, and the answer the substitution gave <c>x arcsch(sqrt(x))</c>
    /// was wrong for negative x, which the points here include. Rubi's 7.5.2 and 7.6.2.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnInverseHyperbolicSecantOfARootByPartsIntegralTest
    {
        [Theory]
        [InlineData("x^3*asech(sqrt(x))")]
        [InlineData("x^2*asech(sqrt(x))")]
        [InlineData("asech(sqrt(x))")]
        [InlineData("x^3*acsch(sqrt(x))")]
        [InlineData("x*acsch(sqrt(x))")]
        [InlineData("acsch(sqrt(x))")]
        public void OnEverySideOfZeroAndMinusOne(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e;
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.3, -0.4, 0.3, 0.8, 1.3, 2.9 })
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
