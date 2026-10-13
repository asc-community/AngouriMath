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
    /// A root of a product beside a root of one of its factors, the product split in front of a
    /// constant: <c>(a + b x^2)^(5/2)/sqrt(a^2 - b^2 x^4)</c> is <c>K (a + b x^2)^2/sqrt(a - b x^2)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's 1.2.2.3:282-293 were declined. Each answer is differentiated back with <c>b</c> of
    /// either sign, so that each factor of the radicand is negative somewhere it is sampled.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class ARootOfAProductSplitBesideARootOfAFactorIntegralTest
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
        [InlineData("(a+b*x^2)^(5/2)/sqrt(a^2-b^2*x^4)")]
        [InlineData("(a+b*x^2)^(1/2)/sqrt(a^2-b^2*x^4)")]
        [InlineData("1/((a+b*x^2)^(3/2)*sqrt(a^2-b^2*x^4))")]
        [InlineData("(a-b*x^2)^(3/2)/sqrt(a^2-b^2*x^4)")]
        [InlineData("1/((a-b*x^2)^(5/2)*sqrt(a^2-b^2*x^4))")]
        public void ARootOfADifferenceOfSquaresBesideARootOfAFactor(string integrand)
        {
            var points = new[] { -1.3, -0.4, 0.3, 0.7, 1.6 };
            DifferentiatesBack(integrand, points, ("a", 2), ("b", 3));
            DifferentiatesBack(integrand, points, ("a", 2), ("b", -3));
        }
    }
}
