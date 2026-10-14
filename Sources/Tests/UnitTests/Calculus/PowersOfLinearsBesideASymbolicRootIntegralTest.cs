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
    /// A rational function with powers of linears below the bar, beside a half-odd power of a
    /// quadratic with symbols in it, taken apart over the linears: Euler's substitution, which
    /// the split left these to, writes the root of a symbolic leading coefficient into every
    /// coefficient. Rubi's 1.2.2.4 <c>sqrt(a + b x^2 + c x^4)/(x^3 (d + e x^2))</c> is one under
    /// <c>u = x^2</c>, written here with <c>k</c> for <c>e</c>, which is Euler's number.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PowersOfLinearsBesideASymbolicRootIntegralTest
    {
        private static void DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

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

            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart)
                               + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("sqrt(a+b*x+c*x^2)/(x^2*(d+k*x))")]
        [InlineData("1/(x^2*(d+k*x)*sqrt(a+b*x+c*x^2))")]
        [InlineData("sqrt(a+b*x^2+c*x^4)/(x^3*(d+k*x^2))")]
        [InlineData("1/((d+k*x)*(f+g*x)^2*sqrt(a+b*x+c*x^2))")]
        public void IsAnsweredAndDifferentiatesBack(string integrand)
        {
            var points = new[] { -1.4, -0.3, 0.8, 2.25 };
            DifferentiatesBack(integrand, points, ("a", 2), ("b", 1.0 / 3), ("c", 1.5), ("d", 1.25), ("k", 3), ("f", 7.0 / 3), ("g", -0.4));
            DifferentiatesBack(integrand, points, ("a", -1), ("b", 3), ("c", 2), ("d", -2), ("k", 1.5), ("f", 0.5), ("g", 5.0 / 3));
        }
    }
}
