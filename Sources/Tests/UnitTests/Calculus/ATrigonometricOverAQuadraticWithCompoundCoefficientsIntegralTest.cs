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
    /// A sine or a cosine over a quadratic with compound coefficients, split over the quadratic's
    /// roots with the coefficients named: <c>sin(a + b x)/(x ((d e - c f) x^2 + f))</c> was
    /// 11,876 characters, every coefficient expanded to the seventh degree; and one with a double
    /// root is a linear twice, where split over two equal roots it was NaN everywhere.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1877">#1877</a>,
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1879">#1879</a>
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ATrigonometricOverAQuadraticWithCompoundCoefficientsIntegralTest
    {
        private static void DifferentiatesBack(string integrand, double[] points, params (string, double)[] pins)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());
            Assert.DoesNotContain("NaN", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            Entity original = integrand.ToEntity();
            foreach (var (name, value) in pins)
            {
                derivative = derivative.Substitute(name, value);
                original = original.Substitute(name, value);
            }

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
        [InlineData("sin(a+b*x)/(x*((d*e-c*f)*x^2+f))", 1500)]
        [InlineData("x*cos(a+b*x)/((d*e-c*f)*x^2+f)", 1500)]
        [InlineData("sin(a+b*x)/((c+d)*x^2+(e-f)*x+1)", 1500)]
        [InlineData("sin(a+b*x)/((c+d)*x^2+2*(c+d)*x+(c+d))", 1500)]
        [InlineData("sin(x)/(a*x^2+2*a*x+a)", 1500)]
        public void IsShortAndDifferentiatesBack(string integrand, int longest)
        {
            var length = integrand.ToEntity().Integrate("x").Stringize().Length;
            Assert.True(length <= longest, $"the antiderivative of {integrand} is {length} characters");
            var points = new[] { -1.4, -0.3, 0.8, 2.25 };
            DifferentiatesBack(integrand, points, ("a", 1.0 / 3), ("b", 2), ("c", 1.5), ("d", 1.25), ("e", 2), ("f", 3));
            DifferentiatesBack(integrand, points, ("a", -0.4), ("b", -1.5), ("c", 2), ("d", -0.5), ("e", -1), ("f", 2));
        }
    }
}
