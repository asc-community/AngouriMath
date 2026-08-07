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
    /// Quotients whose denominator is a quadratic with a symbolic leading coefficient and no
    /// x term, such as k/(a x^2 + c). These answered NaN: the a = 0 arm of the piecewise is
    /// written as (k/b) ln|bx + c|, which divides by zero when the denominator has no x term,
    /// and the resulting NaN propagates out of the whole piecewise as soon as a is symbolic
    /// and so the arm cannot be dropped for being unreachable. With a numeric leading
    /// coefficient a = 0 is decidably false, the arm is dropped, and the same integrals were
    /// answered correctly -- which is why this was invisible to every test using numbers.
    ///
    /// No issue is filed for this; it was found by work/intbench against Rubi's suite, where
    /// it accounted for six of the seven wrong answers.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class SymbolicCoefficientIntegralsTest
    {
        /// <summary>
        /// Integrates with the coefficients left symbolic, then substitutes values into the
        /// answer and differentiates it back. Specialising after integrating is the point:
        /// substituting first would take the numeric path, which never had the defect.
        /// </summary>
        private static void AssertIsAntiderivative(
            string integrand, string coefficients, params double[] points)
        {
            var f = integrand.ToEntity();
            var antiderivative = f.Integrate("x");
            Assert.DoesNotContain("integral(", antiderivative.Stringize());
            Assert.DoesNotContain("NaN", antiderivative.Stringize());
            // The root-of-a-quadratic arm only collapses once the *symbolic* answer is
            // simplified: the raw form still carries an undivided .../0 that has not been
            // turned into NaN yet, so checking the raw form alone would miss it.
            Assert.DoesNotContain("NaN", antiderivative.Simplify().Stringize());

            Entity specialisedIntegrand = f;
            Entity specialisedAnswer = antiderivative.Substitute("C", 0);
            foreach (var assignment in coefficients.Split(','))
            {
                var parts = assignment.Split('=');
                var name = parts[0].Trim();
                var value = double.Parse(parts[1].Trim(),
                    System.Globalization.CultureInfo.InvariantCulture);
                specialisedIntegrand = specialisedIntegrand.Substitute(name, value);
                specialisedAnswer = specialisedAnswer.Substitute(name, value);
            }

            // Simplify collapses the piecewise once the conditions are decidable; the
            // derivative of an undecided piecewise would say nothing.
            var derivative = specialisedAnswer.Simplify().Differentiate("x");
            foreach (var point in points)
            {
                var expected = specialisedIntegrand.Substitute("x", point)
                    .EvalNumerical().RealPart.EDecimal.ToDouble();
                var actual = derivative.Substitute("x", point)
                    .EvalNumerical().RealPart.EDecimal.ToDouble();
                Assert.Equal(expected, actual, 8);
            }
        }

        // The six wrong answers intbench found, in the form Rubi states them. Every one is a
        // denominator with a symbolic leading coefficient and no x term.
        [Theory]
        [InlineData("1 / (a ^ 2 + b ^ 2 * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (a ^ 2 - b ^ 2 * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (a + b * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (a * x ^ 2 + c)", "a = 3, c = 2", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (A ^ 4 - A ^ 2 * B ^ 2 + (-A ^ 2 + B ^ 2) * x ^ 2)",
            "A = 3, B = 2", new[] { 0.3, 1.7, -0.6 })]
        public void ASymbolicLeadingCoefficientWithNoXTerm(
            string integrand, string coefficients, double[] points) =>
            AssertIsAntiderivative(integrand, coefficients, points);

        // A linear numerator over the same shape goes through the same arm, one rewrite later.
        [Theory]
        [InlineData("x / (a ^ 2 + b ^ 2 * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("(2 * x + 1) / (a + b * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 1.7, -0.6 })]
        public void ALinearNumeratorOverTheSameShape(
            string integrand, string coefficients, double[] points) =>
            AssertIsAntiderivative(integrand, coefficients, points);

        // The neighbouring shapes must keep working: a symbolic leading coefficient *with* an
        // x term never had the defect, because the a = 0 arm divides by a b that is not zero.
        [Theory]
        [InlineData("1 / (a * x ^ 2 + b * x + c)", "a = 1, b = 2, c = 5", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (a * x ^ 2 + 2 * x + 3)", "a = 1", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / (1 + b * x + a * x ^ 2)", "a = 1, b = 2", new[] { 0.3, 1.7, -0.6 })]
        public void ASymbolicLeadingCoefficientWithAnXTerm(
            string integrand, string coefficients, double[] points) =>
            AssertIsAntiderivative(integrand, coefficients, points);

        // k/sqrt(a x^2 + c) has the same arm with the same division by zero in it, and it is
        // the last of intbench's seven wrong answers: 1/sqrt(-alpha^2 + 2 h r^2) came back as
        // sqrt(-alpha^2)/0 and Simplify turned the lot into NaN.
        [Theory]
        [InlineData("1 / sqrt(a * x ^ 2 + c)", "a = 2, c = 3", new[] { 0.3, 1.7, -0.6 })]
        [InlineData("1 / sqrt(a * x ^ 2 + c)", "a = -9, c = 4", new[] { 0.3, 0.6, -0.5 })]
        [InlineData("1 / sqrt(-alpha ^ 2 + 2 * h * x ^ 2)", "alpha = 1, h = 2", new[] { 1.3, 2.4 })]
        [InlineData("1 / sqrt(a ^ 2 - b ^ 2 * x ^ 2)", "a = 2, b = 3", new[] { 0.3, 0.6, -0.5 })]
        public void ARootOfAQuadraticWithNoXTerm(
            string integrand, string coefficients, double[] points) =>
            AssertIsAntiderivative(integrand, coefficients, points);

        // And the shapes beside it, which have an x term and never had the defect.
        [Theory]
        [InlineData("1 / sqrt(a * x ^ 2 + b * x + c)", "a = 1, b = 2, c = 5", new[] { 0.3, 1.7 })]
        [InlineData("1 / sqrt(2 * x + 3)", "a = 1", new[] { 0.3, 1.7 })]
        public void ARootOfAQuadraticWithAnXTerm(
            string integrand, string coefficients, double[] points) =>
            AssertIsAntiderivative(integrand, coefficients, points);

        /// <summary>
        /// The degenerate arm is not merely non-NaN, it is the right answer: where the
        /// leading coefficient really is zero and there is no x term, the integrand is the
        /// constant k/c and its antiderivative is kx/c. The old code claimed (k/b) ln|bx + c|
        /// there, which is wrong quite apart from dividing by zero.
        /// </summary>
        [Fact]
        public void TheDegenerateArmIsTheConstantIntegral()
        {
            var antiderivative = "1 / (a * x ^ 2 + 5)".ToEntity().Integrate("x")
                .Substitute("C", 0).Substitute("a", 0).Simplify();
            var derivative = antiderivative.Differentiate("x").Simplify();
            foreach (var point in new[] { 0.3, 1.7, -0.6 })
                Assert.Equal(0.2,
                    derivative.Substitute("x", point).EvalNumerical().RealPart.EDecimal.ToDouble(), 8);
        }
    }
}
