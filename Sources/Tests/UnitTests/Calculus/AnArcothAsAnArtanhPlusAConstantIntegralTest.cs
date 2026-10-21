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
    /// An inverse hyperbolic cotangent of a linear as its tangent plus <c>K = acoth(L) - atanh(L)</c>,
    /// which is <c>i pi/2</c> inside <c>(-1, 1)</c> and <c>-i pi/2</c> outside it, constant on each side:
    /// <c>acoth(x)^2/(1 - x^2)^2</c> was declined where <c>(atanh(x) + k)^2/(1 - x^2)^2</c> is answered.
    /// Rubi's 7.4.1. The points are on both sides of <c>c x = 1</c> and of <c>c x = -1</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnArcothAsAnArtanhPlusAConstantIntegralTest
    {
        [Theory]
        [InlineData("acoth(x)^2/(1-x^2)^2")]
        [InlineData("acoth(c*x)^2/(1-c^2*x^2)^2")]
        [InlineData("acoth(x)^3/(1-x^2)^2")]
        [InlineData("acoth(x)^2/(1-x^2)^2+acoth(x)/(1-x^2)")]
        public void OnBothSidesOfOne(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            Entity Pinned(Entity e) => e.Substitute("c", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            var compared = 0;
            foreach (var at in new[] { -2.9, -1.3, -0.7, 0.3, 0.8, 1.3, 2.9 })
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
