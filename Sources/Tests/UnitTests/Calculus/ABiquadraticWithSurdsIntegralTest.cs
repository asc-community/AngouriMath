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
    /// An even polynomial over a biquadratic whose coefficients are numbers that are not rational,
    /// split by its roots in <c>x^2</c> as a symbolic one is: Rubi's 1.2.2.3
    /// <c>(sqrt(2) - x^2)/(1 - sqrt(2) x^2 + x^4)</c>. Roots in <c>x^2</c> complex, and real.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ABiquadraticWithSurdsIntegralTest
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
        [InlineData("(-x^2+sqrt(2))/(1+x^4-x^2*sqrt(2))")]
        [InlineData("(x^2+sqrt(2))/(1+x^4+x^2*sqrt(2))")]
        [InlineData("1/(1+x^4-x^2*sqrt(3))")]
        [InlineData("(1+x^2+x^4)/(1+x^4-x^2*sqrt(2))")]
        [InlineData("1/(1+x^4-3*x^2*sqrt(2))")]
        [InlineData("x^2/(1-x^4*sqrt(2)+x^2)")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
            => DifferentiatesBack(integrand);
    }
}
