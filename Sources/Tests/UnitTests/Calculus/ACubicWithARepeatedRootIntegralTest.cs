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
    /// A cubic with symbolic coefficients and a repeated root below the bar, written over its
    /// roots: Rubi's 1.3.1 <c>1/(9x^3 - 9bx + 2 sqrt(3) b^(3/2))</c>, which was left unevaluated.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ACubicWithARepeatedRootIntegralTest
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
            // Away from the roots, at b > 0, where the coefficients are real.
            foreach (var at in new[] { -3.1, -0.4, 0.3, 2.7 })
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
        [InlineData("1/(-9*b*x+9*x^3+2*b^(3/2)*sqrt(3))")]
        [InlineData("x/(x^3-3*b*x+2*b^(3/2))")]
        [InlineData("1/(x^3+3*k*x^2+3*k^2*x+k^3)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("b", 1.3), ("k", 0.7));
            DifferentiatesBack(integrand, ("b", 2.2), ("k", -1.6));
        }
    }
}
