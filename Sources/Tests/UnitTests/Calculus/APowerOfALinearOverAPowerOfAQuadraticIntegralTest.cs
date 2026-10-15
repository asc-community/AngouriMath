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
    /// A half-odd power of a linear over a whole power of a quadratic, the power of the quadratic
    /// lowered one at a time. Rubi's 1.2.1.2 <c>sqrt(d + e x)/(a + b x + c x^2)^3</c>, written here
    /// with <c>k</c> for <c>e</c>, which is Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfALinearOverAPowerOfAQuadraticIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            // Pinned before it is differentiated: what the integrator answers over the first power
            // is piecewise on signs read where they are numbers.
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
        [InlineData("sqrt(d+k*x)/(a+b*x+c*x^2)^3")]
        [InlineData("(d+k*x)^(7/2)/(a+b*x+c*x^2)^2")]
        [InlineData("1/((d+k*x)^(3/2)*(a+b*x+c*x^2)^2)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("b", 1.5), ("c", 1.25), ("d", 1.25), ("k", 2));
            DifferentiatesBack(integrand, ("a", -1), ("b", 2), ("c", 2), ("d", -0.5), ("k", 1.5));
        }
    }
}
