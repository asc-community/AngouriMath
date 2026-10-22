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
    /// Two roots of linears written as the radicand an inverse hyperbolic cosine holds, asked without
    /// the condition the inner simplification writes over the rewriting: Rubi's 7.2.5
    /// <c>x/(acosh(x) sqrt(x - 1) sqrt(x + 1))</c> was asked as <c>... provided not x - 1 = 0</c> and ran
    /// past the budget, where it is a hyperbolic cosine integral.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class TheRadicandRewritingAskedWithoutItsConditionIntegralTest
    {
        [Theory]
        [InlineData("x/(acosh(x)*sqrt(-1 + x)*sqrt(1 + x))")]
        [InlineData("x^2/(acosh(x)*sqrt(-1 + x)*sqrt(1 + x))")]
        public void IsAnswered(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            foreach (var at in new[] { 1.2, 1.7, 2.3, 3.1, 4.4 })
            {
                var want = original.Substitute("x", at).EvalNumerical();
                var got = derivative.Substitute("x", at).EvalNumerical();
                Assert.True(Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart)
                            < 1e-9 * Math.Max(1, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart)),
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }
    }
}
