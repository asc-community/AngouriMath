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
    /// Fractional powers of two different linears in the variable, rationalised by the
    /// quotient of their roots, <c>t = (a x + b)^(1/q) / (c x + d)^(1/q)</c>; and a root of a
    /// product of powers of linears -- Timofeev's spelling, <c>((x - 1)^4 (x + 1)^2)^(1/3)</c>
    /// -- written apart into those powers where that holds on the reals.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/718">#718</a>
    /// </summary>
    /// <remarks>
    /// Checked by differentiating back at points where the integrand is real, never against a
    /// printed form. The answers are rational functions and logarithms of the quotient of the
    /// two roots, whose shape says nothing about whether they differentiate back.
    /// </remarks>
    [Trait("Area", "Calculus")]
    public sealed class TwoLinearRadicalsIntegralTest
    {
        private static void DifferentiatesBack(string integrand, params double[] points)
        {
            var integral = integrand.ToEntity().Integrate("x");
            Assert.DoesNotContain("integral(", integral.Stringize());

            var derivative = integral.Substitute("C", 0).Differentiate("x");
            var original = integrand.ToEntity();
            var compared = 0;
            foreach (var at in points)
            {
                var got = derivative.Substitute("x", at).EvalNumerical();
                var want = original.Substitute("x", at).EvalNumerical();
                if (got.IsNaN || want.IsNaN)
                    continue;
                compared++;
                var difference = Math.Abs((double)(got - want).RealPart) + Math.Abs((double)(got - want).ImaginaryPart);
                var scale = Math.Max(1.0, Math.Abs((double)want.RealPart) + Math.Abs((double)want.ImaginaryPart));
                Assert.True(difference / scale < 1e-9,
                    $"d/dx of the antiderivative of {integrand} is {got} at x = {at}, "
                    + $"where the integrand is {want}");
            }
            Assert.True(compared >= 3, $"only {compared} points could be compared for {integrand}");
        }

        /// <summary>Two linears under roots of the same order, above and below 1 where both roots are real.</summary>
        [Theory]
        [InlineData("(x - 1)^(-4/3) * (x + 1)^(-2/3)")]
        [InlineData("(x + 2)^(1/3) / (x - 1)^(4/3)")]
        [InlineData("1/(x * (x - 1)^(1/3) * (x + 1)^(2/3))")]
        [InlineData("(x - 1)^(2/3) * (x + 1)^(1/3)")]
        public void TwoRootsOfTheSameOrder(string integrand)
            => DifferentiatesBack(integrand, 1.3, 1.7, 2.4, 3.1, -1.5, -2.7);

        /// <summary>Roots of different orders sharing a common one: Timofeev's 315, a sixth root of the quotient.</summary>
        [Fact]
        public void RootsOfDifferentOrdersInASum()
            => DifferentiatesBack("x*(1+x)^(2/3)*sqrt(1-x)/(-(1-x)^(5/6)*(1+x)^(1/3)+(1-x)^(2/3)*sqrt(1+x))", 0.31, 0.57, 0.83, -0.2, -0.6);

        /// <summary>
        /// The root of a product of powers of linears, written apart: an odd root exactly, by
        /// the real root a negative number has here; an even one where the phases agree on
        /// every real interval. Timofeev's 318, 319 and 320.
        /// </summary>
        [Theory]
        [InlineData("1/((-1+x)^4*(1+x)^2)^(1/3)", new[] { 0.31, 0.57, 0.83, 1.29, 1.77, -1.5, -2.7 })]
        [InlineData("1/((-1+x)^7*(1+x)^2)^(1/3)", new[] { 0.31, 0.57, 0.83, 1.29, 1.77, -1.5, -2.7 })]
        [InlineData("1/((-1+x)^3*(2+x)^5)^(1/4)", new[] { 1.29, 1.77, 2.41, -2.5, -3.2 })]
        public void ARootOfAProductOfPowersOfLinears(string integrand, double[] points)
            => DifferentiatesBack(integrand, points);
    }
}
