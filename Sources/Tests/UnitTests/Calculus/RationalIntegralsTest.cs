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
    /// Quotients of polynomials of low degree. Only a constant numerator was recognised, so
    /// x/(x^2 + 2x + 5) and x/(x + 1) had no antiderivative at all. Each answer is checked by
    /// differentiating it back and comparing at points, since what matters is that it is an
    /// antiderivative and not what form it is written in.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class RationalIntegralsTest
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
                Assert.Equal(expected, actual, 8);
            }
        }

        // (px + q) / (ax^2 + bx + c). The numerator is written as a multiple of the
        // denominator's derivative plus a constant, which splits it into a logarithm and the
        // constant-numerator case that was already there.
        [Theory]
        [InlineData("x / (x ^ 2 + 2 * x + 5)", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("(x + 3) / (x ^ 2 + 3 * x + 2)", new[] { 0.3, 1.7, 4.2 })]
        [InlineData("(2 * x + 1) / (x ^ 2 + x + 1)", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("x / (x ^ 2 + 1)", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("(3 * x - 2) / (2 * x ^ 2 + 5)", new[] { 0.3, 1.7, -0.6 })]
        public void ALinearNumeratorOverAQuadratic(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // (px + q) / (bx + c), the same rewrite one degree down.
        [Theory]
        [InlineData("x / (x + 1)", new[] { 0.3, 1.7 })]
        [InlineData("(2 * x + 1) / (x - 3)", new[] { 0.3, 1.7 })]
        [InlineData("(3 * x) / (2 * x + 5)", new[] { 0.3, 1.7 })]
        public void ALinearNumeratorOverALinearDenominator(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // 1/cos(u)^2 and 1/sin(u)^2 are written that way at least as often as sec(u)^2 and
        // csc(u)^2, and neither of the four shapes was recognised.
        [Theory]
        [InlineData("1 / cos(x) ^ 2", new[] { 0.3, 1.1, -0.6 })]
        [InlineData("1 / sin(x) ^ 2", new[] { 0.3, 1.1, 2.2 })]
        [InlineData("2 / cos(3 * x) ^ 2", new[] { 0.3, 0.9 })]
        [InlineData("sec(x) ^ 2", new[] { 0.3, 1.1, -0.6 })]
        [InlineData("cosec(x) ^ 2", new[] { 0.3, 1.1, 2.2 })]
        public void ReciprocalSquaresOfTheWaves(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // The shapes these sit next to in the table have to keep working. A constant over a
        // quadratic in particular is matched by the earlier arm and must stay there.
        [Theory]
        [InlineData("1 / (x ^ 2 + 1)", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (x ^ 2 + 2 * x + 5)", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (2 * x + 3)", new[] { 0.3, 1.7 })]
        [InlineData("1 / x", new[] { 0.3, 1.7 })]
        [InlineData("x ^ 2", new[] { 0.3, 1.7 })]
        [InlineData("sin(x)", new[] { 0.3, 1.7 })]
        [InlineData("tan(x)", new[] { 0.3, 1.1 })]
        [InlineData("x * e ^ x", new[] { 0.3, 1.7 })]
        public void NeighbouringFormsAreUnaffected(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // A denominator whose rational root repeats. Splitting off one factor and giving up
        // if the quotient still vanished there left 1/(x^4 + x^2) with no antiderivative:
        // its only rational root is zero, twice. A root of multiplicity m contributes a term
        // over the m-th power, and taking all m out at once is what the single-root arm was
        // already doing for m = 1.
        [Theory]
        [InlineData("1 / (x ^ 4 + x ^ 2)", new[] { 0.7, 1.3, -1.7 })]
        [InlineData("1 / (x ^ 3 + x ^ 2)", new[] { 0.7, 1.3, -1.7 })]
        [InlineData("1 / (x ^ 2 * (x + 1))", new[] { 0.7, 1.3, -1.7 })]
        [InlineData("1 / ((x - 1) ^ 2 * (x + 2))", new[] { 0.7, 2.3, -1.7 })]
        [InlineData("x / ((x - 1) ^ 2 * (x ^ 2 + 1))", new[] { 0.7, 2.3, -1.7 })]
        [InlineData("1 / ((x + 1) ^ 3 * (x - 2))", new[] { 0.7, 2.3, -1.7 })]
        [InlineData("(x + 1) / (x ^ 3 - x ^ 2)", new[] { 0.7, 1.3, -1.7 })]
        public void ARepeatedRationalRootDecomposesToo(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // Denominators with no rational root at all are still out of reach: x^4 + 1 is
        // irreducible over Q and only factors once real coefficients are allowed. Recorded
        // so the boundary is visible rather than inferred from an absence.
        [Theory]
        [InlineData("x ^ 2 / (x ^ 4 + 1)")]
        [InlineData("1 / (x ^ 4 + 4)")]
        public void ADenominatorWithNoRationalRootIsStillDeclined(string integrand) =>
            Assert.Contains("integral(", integrand.ToEntity().Integrate("x").Stringize());
    }
}
