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
    /// A rational function with a symbolic trinomial in x^2 below the bar, beside a root of a
    /// quadratic: taken apart over the trinomial's roots in x^2 where they are real, and over two
    /// real quadratics where they are a conjugate pair. Rubi's 1.2.2.4, written here with
    /// <c>k</c> for <c>e</c>, which is Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ATrinomialInTheSquareBesideARootIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            // Pinned before it is differentiated: the answer is a piecewise on the sign of the
            // discriminant, read where it is a number.
            var pinned = integral.Substitute("C", 0);
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                pinned = pinned.Substitute(name, value);
                original = original.Substitute(name, value);
            }
            var derivative = pinned.Differentiate("x");
            foreach (var at in new[] { -0.7, -0.3, 0.4, 0.9 })
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
        [InlineData("x^2*sqrt(d+k*x^2)/(a+b*x^2+c*x^4)")]
        [InlineData("sqrt(d+k*x^2)/(x^2*(a+b*x^2+c*x^4))")]
        [InlineData("sqrt(1-x^2)/(x^2*(a+b*x^2+c*x^4))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            // Two real roots in x^2, then a conjugate pair.
            DifferentiatesBack(integrand, ("a", 1), ("b", 5), ("c", 2), ("d", 1.5), ("k", 2));
            DifferentiatesBack(integrand, ("a", 2), ("b", 1), ("c", 1.5), ("d", 1.25), ("k", 3));
        }
    }
}
