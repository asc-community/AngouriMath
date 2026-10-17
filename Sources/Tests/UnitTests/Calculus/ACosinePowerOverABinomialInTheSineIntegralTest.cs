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
    /// A power of a cosine over a power of <c>k (1 +- sin)</c>, cancelled: Rubi's 4.1.10
    /// <c>cos(c + d x)^2/((e + f x)(a + a sin(c + d x)))</c>, which was declined.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ACosinePowerOverABinomialInTheSineIntegralTest
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

        // The linear's root kept off the sampled points.
        [Theory]
        [InlineData("cos(c+d*x)^2/((k+f*x)*(a+a*sin(c+d*x)))")]
        [InlineData("cos(c+d*x)^3/((k+f*x)^2*(a+a*sin(c+d*x)))")]
        [InlineData("sin(c+d*x)^2/((k+f*x)*(a-a*cos(c+d*x)))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("c", 0.7), ("d", 1.07), ("k", 5), ("f", 0.3));
            DifferentiatesBack(integrand, ("a", -0.5), ("c", -0.25), ("d", 1.5), ("k", -6), ("f", 0.4));
        }
    }
}
