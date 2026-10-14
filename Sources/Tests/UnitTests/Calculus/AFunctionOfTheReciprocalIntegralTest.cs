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
    /// A trigonometric function of <c>a + b/x</c> beside a rational function, under <c>x = 1/u</c>:
    /// <c>sin(c + d/x)/(e + f x)</c> is <c>-sin(c + d u)/(u (e u + f)) du</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's 4.1.12 <c>(a + b sin(c + d/x))^k/(e + f x)^m</c> were declined. Each answer is
    /// differentiated back on both sides of 0.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class AFunctionOfTheReciprocalIntegralTest
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
        [InlineData("sin(c+d/x)/(e+f*x)")]
        [InlineData("cos(c+d/x)/(e+f*x)^2")]
        [InlineData("(a+b*sin(c+d/x))/(e+f*x)^3")]
        [InlineData("(a+b*sin(c+d/x))^2/(e+f*x)")]
        public void AFunctionOfAPlusBOverX(string integrand)
        {
            var points = new[] { -1.4, -0.3, 0.8, 2.25 };
            DifferentiatesBack(integrand, points, ("a", 1.0 / 3), ("b", 2), ("c", 1.5), ("d", 1.25), ("e", 2), ("f", 3));
        }

        [Theory]
        [InlineData("(c*e+d*e*x)^(1/3)*sin(a+b/(c+d*x)^(1/3))")]
        [InlineData("sin(a+b/sqrt(c+d*x))/(e+f*x)")]
        public void AFunctionOfAPlusBOverARootOfALinear(string integrand)
        {
            var points = new[] { -0.3, 0.8, 2.25 };
            DifferentiatesBack(integrand, points, ("a", 1.0 / 3), ("b", 2), ("c", 1.5), ("d", 1.25), ("e", 2), ("f", 3));
        }
    }
}
