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
    /// A half-odd power of <c>L + sqrt(Q)</c> where <c>Q - L^2</c> is a square, in Euler's
    /// <c>u = (sqrt(Q) - L)/(x - r)</c>, under which the nested root is the root of a quadratic in
    /// <c>u</c>. Bondarenko's two were declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/233">#233</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHalfOddPowerOfALinearPlusTheRootOfAQuadraticIntegralTest
    {
        [Theory]
        [InlineData("sqrt(1+sqrt(x)+sqrt(1+2*x+2*sqrt(x)))")]
        [InlineData("sqrt(sqrt(2)+sqrt(x)+sqrt(2+2*x+sqrt(8)*sqrt(x)))")]
        [InlineData("x*sqrt(1+x+sqrt(1+2*x+2*x^2))")]
        [InlineData("1/sqrt(1+x+sqrt(1+2*x+2*x^2))")]
        public void InEulersSubstitution(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Assert.True(text.Length < 5000, $"{text.Length} characters of answer for {integrand}");
            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
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
