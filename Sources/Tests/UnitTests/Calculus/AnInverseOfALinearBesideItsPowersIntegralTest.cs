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
    /// A function of an inverse function of <c>c + d x</c> beside powers of multiples of it, under
    /// <c>u = c + d x</c>: Rubi's 7.1.5 <c>(c e + d e x)^3 (a + b arsinh(c + d x))^2</c> and 7.2.5
    /// <c>(c e + d e x)(a + b arcosh(c + d x))^4</c>, which timed out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnInverseOfALinearBesideItsPowersIntegralTest
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

        // c + d x above 1 at every sampled x, where the inverse hyperbolic cosine is real.
        [Theory]
        [InlineData("(c*k+d*k*x)^3*(a+b*asinh(c+d*x))^2")]
        [InlineData("(c*k+d*k*x)^3*(a+b*asinh(c+d*x))^4")]
        [InlineData("(c*k+d*k*x)*(a+b*acosh(c+d*x))^4")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 3), ("d", 0.5), ("k", 1.5));
            DifferentiatesBack(integrand, ("a", -0.5), ("b", 2), ("c", 4), ("d", -0.6), ("k", -0.7));
        }
    }
}
