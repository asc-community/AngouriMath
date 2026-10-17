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
    /// <c>x^m (a + b atan(c x))^p</c> over a power of <c>d (1 +- i c x)</c>, under <c>x = tan(u)/c</c>, where
    /// <c>1 + i tan(u)</c> is <c>e^(i u)/cos(u)</c>: Rubi's 5.3.4.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnArctangentOverAPowerOfOnePlusICXIntegralTest
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

        // d with the sign that keeps the argument real at every sampled x, and |d| below 2/2.3^2
        // so that the inverse sine and cosine stay inside [-1, 1]; off it the exact checks hold too.
        [Theory]
        [InlineData("(a+b*atan(c*x))^2/(d+i*c*d*x)^2")]
        [InlineData("x*(a+b*atan(c*x))^2/(d+i*c*d*x)^3")]
        [InlineData("(a+b*atan(c*x))^3/(d+i*c*d*x)^4")]
        [InlineData("(a+b*atan(c*x))/(d-i*c*d*x)^3")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.25), ("c", 0.4), ("d", 1.5));
            DifferentiatesBack(integrand, ("a", -1.5), ("b", -0.25), ("c", -2), ("d", 0.4));
        }
    }
}
