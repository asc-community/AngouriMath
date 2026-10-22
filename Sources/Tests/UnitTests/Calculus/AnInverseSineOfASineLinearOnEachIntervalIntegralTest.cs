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
    /// An inverse sine of a sine of a linear, or an inverse cosine of a cosine, is linear on each interval
    /// where the cosine, or the sine, keeps its sign: <c>asin(sin(x))</c> is <c>s x + c</c> there, with
    /// <c>s = cos(x)/sqrt(cos(x)^2)</c>. Rubi's 5.1.5 <c>x/asin(sin(x))</c> was declined, and is
    /// <c>asin(sin(x)) + (s x - asin(sin(x))) ln(asin(sin(x)))</c> up to a constant on each interval.
    /// Checked on both sides of a zero of the cosine, and of the sine.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnInverseSineOfASineLinearOnEachIntervalIntegralTest
    {
        [Theory]
        // Rubi 5.1.5:619.
        [InlineData("x/asin(sin(x))")]
        [InlineData("1/asin(sin(x))")]
        [InlineData("asin(sin(x))/x")]
        [InlineData("x/acos(cos(a + b*x))")]
        public void IsAnsweredOnEveryInterval(string integrand)
        {
            var integral = integrand.ToEntity().Integrate("x");
            var text = integral.Stringize();
            Assert.DoesNotContain("integral(", text);
            Entity Pinned(Entity e) => e.Substitute("a", 0.4).Substitute("b", 0.7);
            var derivative = Pinned(integral.Substitute("C", 0)).Differentiate("x");
            var original = Pinned(integrand.ToEntity());
            // Past pi/2 and past -pi/2 the cosine of x changes sign, and past 3.9 the sine of 0.4 + 0.7 x.
            foreach (var at in new[] { -2.3, -0.9, 0.4, 1.3, 2.2, 2.9, 4.6 })
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
