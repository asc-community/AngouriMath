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
    /// An exponential of a whole multiple of an inverse hyperbolic tangent beside powers of its
    /// quadratic, written as powers of the two linears: Rubi's 7.3.6
    /// <c>e^(n artanh(a x)) x^m (c - a^2 c x^2)^p</c>, which timed out. Pinned where <c>|a x| &lt; 1</c>,
    /// the only place the integrand is real.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class AnExponentialOfAnInverseHyperbolicTangentIntegralTest
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
        [InlineData("e^atanh(a*x)*x^2/(q-a^2*q*x^2)^2")]
        [InlineData("e^(4*atanh(a*x))/(q-a^2*q*x^2)^2")]
        [InlineData("1/(e^(2*atanh(a*x))*(q-a^2*q*x^2)^3)")]
        [InlineData("x^2/(e^(5*atanh(a*x))*(q-a^2*q*x^2)^(27/2))")]
        [InlineData("e^atanh(a+b*x)/(x*(1-a^2-2*a*b*x-b^2*x^2))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 0.2), ("b", 0.15), ("q", 1.3));
            DifferentiatesBack(integrand, ("a", -0.3), ("b", 0.1), ("q", 0.7));
        }
    }
}
