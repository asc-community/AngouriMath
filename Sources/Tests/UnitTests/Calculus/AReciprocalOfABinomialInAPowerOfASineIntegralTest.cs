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
    /// The reciprocal of <c>a + b s^n</c>, <c>s</c> a sine or a cosine and <c>n</c> past 4, or a
    /// hyperbolic sine or cosine, over the roots of the binomial: Rubi's 4.1.7, 4.2.7, 6.1.7 and 6.2.7. Pinned with <c>-a/b</c> of either sign, so the
    /// real root among them is positive or negative.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AReciprocalOfABinomialInAPowerOfASineIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -2.3, -0.7, 0.4, 1.9 })
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, where the integrand is {want}");
            }
        }

        [Theory]
        [InlineData("1/(a+b*sin(x)^5)")]
        [InlineData("1/(a-b*sin(x)^6)")]
        [InlineData("1/(a+b*sin(x)^8)")]
        [InlineData("1/(a-b*cos(x)^5)")]
        [InlineData("1/(a+b*cos(x)^6)")]
        [InlineData("3/(a+b*sin(2*x+1)^7)")]
        [InlineData("1/(a+b*cosh(x)^7)")]
        [InlineData("1/(a-b*cosh(x)^5)")]
        [InlineData("1/(a+b*sinh(x)^6)")]
        [InlineData("1/(a+b*sinh(2*x+1)^5)")]
        [InlineData("1/(a-b*cosh(x)^8)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.25));
            DifferentiatesBack(integrand, ("a", -0.75), ("b", 0.4));
        }
    }
}
