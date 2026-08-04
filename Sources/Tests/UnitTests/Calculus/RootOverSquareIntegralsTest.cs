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
    /// k / (x^2 * sqrt(ax^2 + c)), the shape a trigonometric substitution is usually
    /// taught for. It had no antiderivative at all. Named in the issue's own list of what
    /// is missing: https://github.com/asc-community/AngouriMath/issues/233.
    /// Each answer is checked by differentiating it back and comparing at points, since
    /// what matters is that it is an antiderivative and not what form it is written in.
    /// </summary>
    public sealed class RootOverSquareIntegralsTest
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

        // Differentiating sqrt(ax^2 + c)/x gives -c/(x^2 * sqrt(ax^2 + c)), so every sign
        // of a and c is the same formula: the root of a sum, of a difference, and the two
        // with the sign of x^2 the other way round.
        [Theory]
        [InlineData("1 / (x ^ 2 * sqrt(x ^ 2 - 1))", new[] { 1.4, 2.6, 4.1 })]
        [InlineData("1 / (x ^ 2 * sqrt(x ^ 2 + 1))", new[] { 0.4, 1.6, 3.1, -2.2 })]
        [InlineData("1 / (x ^ 2 * sqrt(1 - x ^ 2))", new[] { 0.4, 0.8, -0.6 })]
        [InlineData("1 / (x ^ 2 * sqrt(4 - x ^ 2))", new[] { 0.4, 1.6, -1.2 })]
        [InlineData("1 / (x ^ 2 * sqrt(2 * x ^ 2 + 3))", new[] { 0.4, 1.6, -2.2 })]
        public void EverySignOfTheQuadraticUnderTheRoot(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // A constant anywhere in the quotient is carried through rather than refused.
        [Theory]
        [InlineData("3 / (x ^ 2 * sqrt(x ^ 2 - 4))", new[] { 2.4, 3.6 })]
        [InlineData("1 / (2 * x ^ 2 * sqrt(x ^ 2 + 9))", new[] { 0.4, 1.6 })]
        [InlineData("(-1) / (x ^ 2 * sqrt(x ^ 2 + 1))", new[] { 0.4, 1.6 })]
        public void AConstantFactorIsCarriedThrough(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // The shapes this sits next to have to keep working.
        [Theory]
        [InlineData("1 / sqrt(x ^ 2 - 1)", new[] { 1.4, 2.6 })]
        [InlineData("1 / sqrt(1 - x ^ 2)", new[] { 0.4, 0.8 })]
        [InlineData("sqrt(x ^ 2 + 1)", new[] { 0.4, 1.6 })]
        [InlineData("1 / x ^ 2", new[] { 0.4, 1.6 })]
        [InlineData("1 / (x ^ 2 + 1)", new[] { 0.4, 1.6 })]
        [InlineData("x / sqrt(x ^ 2 + 1)", new[] { 0.4, 1.6 })]
        [InlineData("sin(x)", new[] { 0.4, 1.6 })]
        public void NeighbouringFormsAreUnaffected(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        /// <summary>
        /// Outside the family the formula does not apply and nothing is claimed: a power of
        /// x other than the square, and a radicand whose constant term is zero, which the
        /// formula would divide by.
        /// </summary>
        [Theory]
        [InlineData("1 / (x ^ 3 * sqrt(x ^ 2 - 1))")]
        [InlineData("1 / (x ^ 2 * sqrt(x ^ 2 + x + 1))")]
        public void OutsideTheFamilyNothingIsClaimed(string integrand) =>
            Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
