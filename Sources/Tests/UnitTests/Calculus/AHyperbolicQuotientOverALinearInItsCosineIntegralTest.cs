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
    /// A rational function of the hyperbolic cosine alone over a sum linear in it, or in the secant,
    /// asked by Osborn's rule as one of the cosine before the tangent of the half argument: under that
    /// tangent Rubi's 6.2.5 <c>sech(x)^3/(a + b cosh(x))</c> and 6.5.3 <c>sech(x)^4/(a + b sech(x))</c>
    /// are a cube of <c>1 + t^2</c> beside a symbolic quadratic, and were answered in 118,366 and
    /// 118,032 characters, past the corpus's budget.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHyperbolicQuotientOverALinearInItsCosineIntegralTest
    {
        [Theory]
        // Rubi 6.2.5:89.
        [InlineData("sech(x)^3/(a + b*cosh(x))")]
        // Rubi 6.5.3:139.
        [InlineData("sech(x)^4/(a + b*sech(x))")]
        [InlineData("cosh(x)^2/(a + b*sech(x))")]
        public void IsAnsweredInTheCosine(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{integrand} came back in {text.Length} characters");
            Entity Pinned(Entity e) => e.Substitute("a", 1.3).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            foreach (var at in new[] { -2.2, -0.9, 0.3, 1.4, 2.1 })
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
