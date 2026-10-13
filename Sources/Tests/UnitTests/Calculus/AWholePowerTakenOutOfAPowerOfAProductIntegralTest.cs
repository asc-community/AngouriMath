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
    /// A power of a product with a factor under a whole power, that power taken out in front of
    /// a constant: <c>(c sin(a + b x)^3)^(1/3)</c> is <c>K sin(a + b x)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's 4.1.12 and 1.3.2 were declined. Each answer is differentiated back with the constant
    /// under the root on both signs: an odd root is the real root, and a negative <c>c</c> used to
    /// be answered <c>provided c &gt; 0</c>.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class AWholePowerTakenOutOfAPowerOfAProductIntegralTest
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
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart));
                Assert.True(difference / scale < 1e-8,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3,
                $"only {compared} of {points.Length} points were comparable for {integrand}");
        }

        [Theory]
        [InlineData("(c*sin(a+b*x)^3)^(1/3)")]
        [InlineData("(c*sin(a+b*x)^3)^(2/3)")]
        [InlineData("(c*sin(a+b*x)^3)^(1/3)/x")]
        [InlineData("(c*sin(a+b*x)^3)^(2/3)/x^3")]
        [InlineData("(c*sin(a+b*x^2)^3)^(1/3)/x^3")]
        public void AConstantOfEitherSignUnderAnOddRoot(string integrand)
        {
            var points = new[] { -1.4, -0.3, 0.8, 2.25 };
            DifferentiatesBack(integrand, points, ("a", 1.0 / 3), ("b", 2), ("c", 2));
            DifferentiatesBack(integrand, points, ("a", 1.0 / 3), ("b", 2), ("c", -3));
        }

        [Theory]
        [InlineData("sqrt((-1+x^2)^2/(x*(1+x^2)))/(1+x^2)", new[] { 0.5, 0.8, 2.0, 3.0 })]
        [InlineData("1/sqrt((a^2+x^2)^3)", new[] { -2.0, 0.3, 2.5 })]
        [InlineData("1/sqrt((a+b*x^2)^3)", new[] { -1.3, 0.3, 1.1 })]
        public void ARootOfAPowerOfAPolynomial(string integrand, double[] points)
            => DifferentiatesBack(integrand, points, ("a", 2), ("b", -1));
    }
}
