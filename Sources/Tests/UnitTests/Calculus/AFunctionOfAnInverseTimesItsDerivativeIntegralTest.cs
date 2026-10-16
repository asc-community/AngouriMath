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
    /// A function of an inverse tangent of something not linear times its derivative, up to a factor
    /// constant though written with <c>x</c>: Rubi's 5.3.7 <c>atan(c x/sqrt(a - c^2 x^2))^m/sqrt(d - c^2 d x^2/a)</c>,
    /// where <c>sqrt(a - c^2 x^2)/sqrt(d - c^2 d x^2/a)</c> is that factor. Pinned on the real domain.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AFunctionOfAnInverseTimesItsDerivativeIntegralTest
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
        [InlineData("atan(c*x/sqrt(a-c^2*x^2))^m/sqrt(d-c^2*d*x^2/a)")]
        [InlineData("atan(c*x/sqrt(a-c^2*x^2))^2/sqrt(d-c^2*d*x^2/a)")]
        [InlineData("1/(atan(c*x/sqrt(a-c^2*x^2))*sqrt(d-c^2*d*x^2/a))")]
        [InlineData("1/(atan(k*x/sqrt(-a*k^2/b-k^2*x^2))^3*sqrt(a+b*x^2))")]
        [InlineData("atan(k*x/sqrt(-a*k^2/b-k^2*x^2))/sqrt(a+b*x^2)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 9), ("b", -1.25), ("c", 0.7), ("d", 1.4), ("k", 0.75), ("m", 2.5));
            DifferentiatesBack(integrand, ("a", 7), ("b", -2), ("c", -0.5), ("d", 0.6), ("k", -0.3), ("m", 1.0 / 3));
        }
    }
}
