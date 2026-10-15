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
    /// A constant over a linear with a symbolic slope beside the root of a quadratic with no
    /// linear term: the quadratic's value at the pole came back with the slope's condition on it,
    /// and the table read no quadratic through it. Rubi's 1.2.1.4
    /// <c>sqrt(a + c x^2)/(x (d + e x))</c>, written here with <c>k</c> for <c>e</c>, which is
    /// Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class APoleWithASymbolicSlopeBesideARootIntegralTest
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
        [InlineData("1/((d+k*x)*sqrt(2+3*x^2))")]
        [InlineData("sqrt(a+c*x^2)/(x*(d+k*x))")]
        [InlineData("1/(x^2*(d+k*x)*sqrt(a+c*x^2))")]
        [InlineData("1/(x*(d+k*x)^2*sqrt(a+c*x^2))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            DifferentiatesBack(integrand, ("a", 2), ("c", 1.5), ("d", 1.25), ("k", 2));
            DifferentiatesBack(integrand, ("a", -1), ("c", 2), ("d", -0.5), ("k", 1.5));
        }
    }
}
