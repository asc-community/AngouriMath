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
    /// A power of x times an inverse tangent and the logarithm of its quadratic, by parts with the two
    /// remainders that have no antiderivative apiece paired: Rubi's 5.3.4
    /// <c>x^k (a + b atan(c x))(d + e ln(1 + c^2 x^2))</c>, which timed out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APowerOfXTimesAnArctangentAndTheLogarithmOfItsQuadraticIntegralTest
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
        [InlineData("x^4*(a+b*atan(c*x))*(d+k*ln(1+c^2*x^2))")]
        [InlineData("x^3*(a+b*atan(c*x))*(d+k*ln(1+c^2*x^2))")]
        [InlineData("x*(a+b*atan(c*x))*(d+k*ln(1+c^2*x^2))")]
        [InlineData("(a+b*atan(c*x))*(d+k*ln(1+c^2*x^2))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("b", 0.7), ("c", 0.3), ("d", 2), ("k", -0.4));
            DifferentiatesBack(integrand, ("a", -2), ("b", 1.5), ("c", -1.2), ("d", 0.5), ("k", 3));
        }
    }
}
