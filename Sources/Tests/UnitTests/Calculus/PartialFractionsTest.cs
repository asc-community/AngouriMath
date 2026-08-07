//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Quotients of polynomials whose denominator is of degree three or more. A linear or
    /// quadratic denominator was answered in one piece, but nothing read anything above
    /// that, so 1/(x^3 + 1) had no antiderivative at all. Named in the issue's own list of
    /// what is missing: https://github.com/asc-community/AngouriMath/issues/233.
    /// Each answer is checked by differentiating it back and comparing at points, since
    /// what matters is that it is an antiderivative and not what form it is written in.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class PartialFractionsTest
    {
        private static void AssertIsAntiderivative(string integrand, params double[] points)
        {
            var f = integrand.ToEntity();
            var antiderivative = f.Integrate("x");
            Assert.DoesNotContain("integral(", antiderivative.Stringize());
            var derivative = antiderivative.Substitute("C", 0).Differentiate("x");
            foreach (var point in points)
            {
                var expected = f.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                var actual = derivative.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(expected, actual, 7);
            }
        }

        // The denominator is split at one rational root at a time, and what is left over is
        // a smaller problem of the same kind until its denominator is a quadratic.
        [Theory]
        [InlineData("1 / (x ^ 3 + 1)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        [InlineData("x / (x ^ 3 + 1)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        [InlineData("1 / (x ^ 3 - x)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        [InlineData("1 / (x ^ 3 + x)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        [InlineData("1 / (x ^ 4 - 1)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        [InlineData("x / (x ^ 4 - 1)", new[] { 0.3, 1.7, 3.2 })]
        [InlineData("(x ^ 2 + 1) / (x ^ 3 - x)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        public void ADenominatorOfDegreeThreeOrMore(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // Whether the denominator arrives factored or multiplied out makes no difference:
        // it is read as a polynomial either way.
        [Theory]
        [InlineData("1 / ((x - 1) * (x - 2) * (x - 3))", new[] { 0.3, 1.7, 3.6, 5.2 })]
        [InlineData("1 / (x ^ 3 - 6 * x ^ 2 + 11 * x - 6)", new[] { 0.3, 1.7, 3.6 })]
        [InlineData("(x + 1) / (x ^ 3 - x ^ 2 - 2 * x)", new[] { 0.3, 1.7, 3.2, -2.4 })]
        public void FactoredOrMultipliedOutAlike(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // The shapes this sits next to have to keep working, in particular the linear and
        // quadratic denominators that are answered in one piece and must not be split.
        [Theory]
        [InlineData("1 / (x ^ 2 + 1)", new[] { 0.3, 1.7 })]
        [InlineData("1 / (x ^ 2 - 1)", new[] { 0.3, 3.2 })]
        [InlineData("1 / (x + 1)", new[] { 0.3, 1.7 })]
        [InlineData("x / (x ^ 2 + 1)", new[] { 0.3, 1.7 })]
        [InlineData("x / (x ^ 2 + 2 * x + 5)", new[] { 0.3, 1.7 })]
        [InlineData("1 / x", new[] { 0.3, 1.7 })]
        [InlineData("x ^ 2", new[] { 0.3, 1.7 })]
        [InlineData("sin(x)", new[] { 0.3, 1.7 })]
        [InlineData("x * e ^ x", new[] { 0.3, 1.7 })]
        public void NeighbouringFormsAreUnaffected(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        /// <summary>
        /// A denominator with no rational root is not split, and neither is one whose root
        /// repeats -- that needs a term over the square as well, which is not produced here.
        /// Both are left unevaluated rather than answered wrongly.
        /// </summary>
        [Theory]
        [InlineData("x ^ 2 / (x ^ 4 + 1)")]
        [InlineData("1 / (x ^ 3 + x ^ 2 + x + 2)")]
        public void WhatCannotBeSplitIsLeftAlone(string integrand) =>
            Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
