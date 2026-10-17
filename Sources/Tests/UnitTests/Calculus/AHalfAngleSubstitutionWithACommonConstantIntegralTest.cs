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
    /// The half-angle substitution with a constant common to the denominator's coefficients: it is
    /// divided out in lowest terms over the symbols, which the simplification does not do for
    /// <c>k^2/k^2</c>. Kept, it left a quadratic whose discriminant was not decidably zero, and
    /// <c>x/(k + k sin(x))^2</c> -- and Rubi's 5.1.4 <c>(a + b asin(c x))/((d + c d x)^(5/2) sqrt(f - c f x))</c>,
    /// which comes to it -- were answered <c>NaN</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AHalfAngleSubstitutionWithACommonConstantIntegralTest
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
        [InlineData("x/(k+k*sin(x))^2")]
        [InlineData("1/(d*q+q*d*sin(x))^2")]
        [InlineData("x/(k-k*cos(x))^2")]
        [InlineData("(a+b*asin(q*x))/((d+q*d*x)^(5/2)*sqrt(f-q*f*x))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            Assert.DoesNotContain("NaN", integrand.ToEntity().Integrate("x").Stringize());
            DifferentiatesBack(integrand, ("a", 0.6), ("b", 1.25), ("q", 0.3), ("d", 0.75), ("f", 1.4), ("k", 1.5));
            DifferentiatesBack(integrand, ("a", -0.75), ("b", 0.4), ("q", -0.4), ("d", -2), ("f", 0.5), ("k", -0.3));
        }
    }
}
