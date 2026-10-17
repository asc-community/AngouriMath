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
    /// A rational function over the quadratic an inverse tangent's derivative is written over, beside
    /// that inverse tangent, split into partial fractions: Rubi's 5.3.4 <c>x^4 atan(a x)/(c + a^2 c x^2)</c>
    /// and <c>atan(a x)/(x^4 (c + a^2 c x^2))</c>, which timed out.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ARationalFunctionOverTheQuadraticOfAnArctangentIntegralTest
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

        // The quadratic's factor is q, not Rubi's c: the helper takes C, and c is too close.
        [Theory]
        [InlineData("x^4*atan(a*x)/(q+a^2*q*x^2)")]
        [InlineData("x^2*atan(a*x)/(q+a^2*q*x^2)")]
        [InlineData("atan(a*x)/(x^2*(q+a^2*q*x^2))")]
        [InlineData("atan(a*x)/(x^4*(q+a^2*q*x^2))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 1.3), ("q", 0.7));
            DifferentiatesBack(integrand, ("a", -0.4), ("q", -2));
        }
    }
}
