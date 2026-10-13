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
    /// A sum with the square root of a quadratic below the bar, cleared by its conjugate.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Rubi's <c>1/(a c + b c x^2 + d sqrt(a + b x^2))</c> and its <c>x^2</c> and <c>1/x^2</c>
    /// neighbours had no antiderivative: Euler's substitution declines a symbolic leading
    /// coefficient. Each answer is differentiated back with the constants pinned on both signs of
    /// <c>b</c> and of <c>a c^2 - d^2</c>, at points where the root is real.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TheConjugateOfASumWithARootBelowTheBarIntegralTest
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
        [InlineData("1/(a*c+b*c*x^2+d*sqrt(a+b*x^2))")]
        [InlineData("x^2/(a*c+b*c*x^2+d*sqrt(a+b*x^2))")]
        [InlineData("1/(x^2*(a*c+b*c*x^2+d*sqrt(a+b*x^2)))")]
        [InlineData("1/(c*sqrt(a+b*x^2)+d)")]
        public void SymbolicConstants(string integrand)
        {
            var points = new[] { 0.3, 0.7, 1.1, 1.3 };
            DifferentiatesBack(integrand, points, ("a", 2), ("b", 3), ("c", 1.25), ("d", 1));
            DifferentiatesBack(integrand, points, ("a", 2), ("b", 3), ("c", 0.5), ("d", 3));
            DifferentiatesBack(integrand, points, ("a", 2), ("b", -1), ("c", 1), ("d", 0.5));
            DifferentiatesBack(integrand, points, ("a", 2), ("b", 3), ("c", 1), ("d", -0.5));
        }

        [Fact]
        public void ARootBesideXOverTheirDifference()
            => DifferentiatesBack("x*sqrt(2-x^2)/(x-sqrt(2-x^2))", new[] { -0.5, 0.3, 1.2, 1.3 });
    }
}
