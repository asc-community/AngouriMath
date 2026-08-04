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
    /// Integrals added to the table rather than reached by the general solvers. Each is
    /// checked by differentiating the answer back and comparing at points, since what
    /// matters is that it is an antiderivative, not what form it is written in.
    /// </summary>
    public sealed class StandardIntegralsTest
    {
        /// <param name="points">
        /// Chosen inside the integrand's own domain. 1/sqrt(x^2 - 1) is imaginary on
        /// (-1, 1), so testing it there says nothing about whether the answer is right.
        /// </param>
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

        // k / sqrt(a x^2 + b x + c), which is an arcsine where a < 0 and a logarithm where
        // a > 0. Nothing integrated 1/sqrt(1 - x^2) at all before.
        [Theory]
        [InlineData("1 / sqrt(1 - x ^ 2)", new[] { 0.31, 0.72, -0.4 })]
        [InlineData("1 / sqrt(4 - x ^ 2)", new[] { 0.31, 1.7, -1.2 })]
        [InlineData("2 / sqrt(9 - x ^ 2)", new[] { 0.5, 2.2, -2.5 })]
        [InlineData("1 / sqrt(x ^ 2 + 1)", new[] { 0.31, 2.4, -1.9 })]
        [InlineData("1 / sqrt(x ^ 2 - 1)", new[] { 1.4, 2.7, 5.1 })]
        [InlineData("1 / sqrt(2 * x + 3)", new[] { 0.5, 2.2 })]
        public void RootOfAQuadraticInTheDenominator(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // An exponential times a sine returns the integral it started from after being
        // integrated by parts twice, so the general solver cycles. Solving that equation
        // once gives a closed form, which is what the table holds.
        [Theory]
        [InlineData("e ^ x * sin(x)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("e ^ x * cos(x)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) * e ^ x", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("cos(x) * e ^ x", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("e ^ (2 * x) * sin(3 * x)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("e ^ (-x) * sin(x)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("2 ^ x * cos(x)", new[] { 0.31, 1.4, -0.8 })]
        public void ExponentialTimesAWave(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // The inverse trigonometric functions are integration by parts against 1, and the
        // by-parts solver looks for a product to split, so it never sees them.
        [Theory]
        [InlineData("arcsin(x)", new[] { 0.21, 0.44, -0.33 })]
        [InlineData("arccos(x)", new[] { 0.21, 0.44, -0.33 })]
        [InlineData("arctan(x)", new[] { 0.21, 0.44, -0.33 })]
        [InlineData("arccotan(x)", new[] { 0.21, 0.44, -0.33 })]
        [InlineData("arcsin(2 * x)", new[] { 0.21, 0.44, -0.33 })]
        [InlineData("arctan(3 * x + 1)", new[] { 0.21, 0.44, -0.33 })]
        public void InverseTrigonometricFunctions(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // The shapes these sit next to in the table have to keep working.
        [Theory]
        [InlineData("1 / (x ^ 2 + 1)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("e ^ x", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x)", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("x * e ^ x", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 2", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("1 / x", new[] { 0.31, 1.4 })]
        public void NeighbouringFormsAreUnaffected(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // sin(u)^n * cos(u)^m for whole n and m. An odd power gives a factor to peel off
        // as the differential, which turns the integral into a polynomial; with both even
        // the halved-angle identities go in and the result is integrated again.
        [Theory]
        [InlineData("sin(x) ^ 3", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("cos(x) ^ 3", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 4", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("cos(x) ^ 5", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 6", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 2 * cos(x) ^ 2", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 3 * cos(x) ^ 2", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(x) ^ 2 * cos(x) ^ 3", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(2 * x) ^ 2", new[] { 0.31, 1.4, -0.8 })]
        [InlineData("sin(3 * x + 1) ^ 3", new[] { 0.31, 1.4, -0.8 })]
        public void PowersOfSineAndCosine(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // Where both powers are odd either substitution works, and they differ by a
        // constant. The sine one is the form everyone writes, and two existing tests
        // assert it, so it is the one tried first.
        [Fact]
        public void BothPowersOddGivesTheSineForm() =>
            Assert.Equal(MathS.Boolean.True,
                "sin(x) * cos(x)".Integrate("x").InnerSimplified
                    .EqualTo("sin(x) ^ 2 / 2 + C".ToEntity().InnerSimplified).Simplify());

        // sqrt(a x^2 + b x + c), which is one integration by parts away from the
        // reciprocal form above and is written in terms of it.
        [Theory]
        [InlineData("sqrt(1 - x ^ 2)", new[] { 0.31, 0.72, -0.4 })]
        [InlineData("sqrt(4 - x ^ 2)", new[] { 0.31, 1.7, -1.2 })]
        [InlineData("sqrt(x ^ 2 + 1)", new[] { 0.31, 2.4, -1.9 })]
        [InlineData("sqrt(x ^ 2 - 1)", new[] { 1.4, 2.7, 5.1 })]
        [InlineData("sqrt(2 * x ^ 2 + 3 * x + 5)", new[] { 0.31, 1.4 })]
        public void RootOfAQuadratic(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);

        // With no quadratic term this is the square root of something linear, which the
        // ordinary power rule already integrates -- and dividing by the leading
        // coefficient would not be allowed. It must not be taken over.
        [Theory]
        [InlineData("sqrt(x)", new[] { 0.31, 1.4 })]
        [InlineData("sqrt(2 * x + 3)", new[] { 0.5, 2.2 })]
        public void RootOfSomethingLinearIsLeftToThePowerRule(string integrand, double[] points) =>
            AssertIsAntiderivative(integrand, points);
    }
}
