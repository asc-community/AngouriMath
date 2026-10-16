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
    /// A power of <c>x</c> over a sextic that is a sum of two cubes,
    /// <c>(3 a + b x^2)^3 + 27 a^2 c x^3</c>, split over the cube roots of unity into three
    /// quadratics. Rubi's 1.3.1 rows; the cube root of <c>27 a^2 c/b^3</c> is the principal one,
    /// complex for a negative <c>c</c>, and the three quadratics are still the factors.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASumOfTwoCubesIntegralTest
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
        [InlineData("x^4/(27*a^3+27*a^2*b*x^2+27*a^2*c*x^3+9*a*b^2*x^4+b^3*x^6)")]
        [InlineData("x^2/(27*a^3+27*a^2*b*x^2+27*a^2*c*x^3+9*a*b^2*x^4+b^3*x^6)")]
        [InlineData("1/(27*a^3+27*a^2*b*x^2+27*a^2*c*x^3+9*a*b^2*x^4+b^3*x^6)")]
        [InlineData("1/(x^2*(27*a^3+27*a^2*b*x^2+27*a^2*c*x^3+9*a*b^2*x^4+b^3*x^6))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("b", 3), ("c", 5));
            DifferentiatesBack(integrand, ("a", -1), ("b", 2), ("c", -3));
        }

        // A difference of two cubes, the second cube's coefficient negative: `1/(a - b cosh(x)^3)` is
        // `8 u^2/(8 a u^3 - b (u^2 + 1)^3)` under `u = e^x`. The real cube root makes all three
        // quadratics complex at sampled values, and the answer was refused for want of points.
        [Theory]
        [InlineData("8*x^2/(8*a*x^3-b*(x^2+1)^3)")]
        [InlineData("1/(a-b*cosh(x)^3)")]
        [InlineData("x/(27*a^3+27*a^2*b*x^2-27*a^2*c*x^3+9*a*b^2*x^4+b^3*x^6)")]
        public void ADifferenceIsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("b", 3), ("c", 5));
            DifferentiatesBack(integrand, ("a", -1), ("b", 2), ("c", -3));
        }
    }
}
