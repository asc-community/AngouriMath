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
    /// A symbolic power of a product of two linears beside a symbolic power of one of them:
    /// <c>(P Q)^p</c> is a constant times <c>P^p Q^p</c>, and the powers of <c>P</c> gather.
    /// Rubi's 1.2.1.2 <c>(d + e x)^(-3 - 2 p) (a d e + (c d^2 + a e^2) x + c d e x^2)^p</c>, written
    /// here with <c>k</c> for <c>e</c>, which is Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ASymbolicPowerOfAProductBesideAPowerOfAFactorIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            // Pinned before it is differentiated, with exponents whose denominators are even, so
            // that every power of a negative base is read on the principal branch
            // (https://github.com/asc-community/AngouriMath/issues/204).
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
        [InlineData("(d+k*x)^(-3-2*p)*(a*d*k+(c*d^2+a*k^2)*x+c*d*k*x^2)^p")]
        [InlineData("(d+k*x)^(-5-2*p)*(a*d*k+(c*d^2+a*k^2)*x+c*d*k*x^2)^p")]
        [InlineData("(a*d*k+(c*d^2+a*k^2)*x+c*d*k*x^2)^p/(d+k*x)^p")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("c", 1.5), ("d", 1.25), ("k", 2), ("p", 0.25));
            DifferentiatesBack(integrand, ("a", -1), ("c", 2), ("d", -0.5), ("k", 1.5), ("p", -1.75));
        }
    }
}
