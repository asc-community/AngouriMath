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
    /// A power of a perfect square whose leading coefficient is a symbol of no known sign:
    /// <c>(c (d + k x)^2)^r</c> is <c>(c k^2)^r |x + d/k|^(2r)</c> for a negative <c>c</c> as for a
    /// positive one, since <c>(x + d/k)^2</c> is positive. Rubi's 1.2.1.2
    /// <c>sqrt(c d^2 + 2 c d e x + c e^2 x^2)</c>, written here with <c>k</c> for <c>e</c>, which is
    /// Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APerfectSquareWithASymbolicLeadingCoefficientIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            // Pinned before it is differentiated: the answer carries `sgn(x + d/k)`, whose
            // derivative is read where its argument is a number.
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
        [InlineData("sqrt(c*d^2+2*c*d*k*x+c*k^2*x^2)")]
        [InlineData("(c*d^2+2*c*d*k*x+c*k^2*x^2)^(3/2)")]
        [InlineData("1/sqrt(c*d^2+2*c*d*k*x+c*k^2*x^2)")]
        [InlineData("1/(c*d^2+2*c*d*k*x+c*k^2*x^2)^(5/2)")]
        [InlineData("(d+k*x)^(-1-2*p)*(c*d^2+2*c*d*k*x+c*k^2*x^2)^p")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("c", 3), ("d", 1.25), ("k", 2), ("p", 0.3));
            DifferentiatesBack(integrand, ("c", -2), ("d", -0.5), ("k", 1.5), ("p", -1.7));
        }
    }
}
