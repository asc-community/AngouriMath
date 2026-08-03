//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// Substituting with a variable that is already in the expression is silent and gives
    /// a wrong answer, so the fresh variables have to really be fresh. These go through
    /// integration, which is where the collision showed up, because the machinery that
    /// makes them is internal.
    /// </summary>
    public sealed class UniqueVariableTest
    {
        private static void AssertIsAntiderivative(string integrand)
        {
            var f = integrand.ToEntity();
            var derivative = f.Integrate("x").Differentiate("x");
            Assert.DoesNotContain("integral(", f.Integrate("x").Stringize());
            // Checked at points: the difference is zero but Simplify does not always
            // reduce it to zero, and what is under test is the value, not the form.
            foreach (var point in new[] { 0.7, 1.3, 2.1 })
            {
                var expected = f.Substitute("x", point).EvalNumerical();
                var actual = derivative.Substitute("x", point).Substitute("C", 0).EvalNumerical();
                Assert.True((expected - actual).Abs().EDecimal.ToDouble()
                            < 1e-6 * System.Math.Max(1, expected.Abs().EDecimal.ToDouble()),
                    $"at x = {point}, d/dx of the answer is {actual.Stringize()} "
                    + $"where the integrand is {expected.Stringize()}");
            }
        }

        // Each of these substitutes u = x^2 or u = x^3 and then substitutes again inside
        // that, which is where the second fresh variable used to come back as the first.
        [Theory]
        [InlineData("x * (x ^ 2 + 1) ^ 2")]       // was a wrong answer
        [InlineData("3 * x ^ 2 * (x ^ 3 + 2) ^ 2")] // was a wrong answer
        [InlineData("x * (x ^ 2 + 1) ^ 3")]
        [InlineData("x * (x ^ 2 + 2) ^ 2")]
        [InlineData("x * (x ^ 2 + 1) ^ 4")]
        public void NestedSubstitutionGivesAnAntiderivative(string integrand) =>
            AssertIsAntiderivative(integrand);

        /// <summary>
        /// These two used to come back unevaluated and are solved by this change. They are
        /// asserted only for correctness, not for being solved at all: the fix that stops
        /// integration re-entering itself closes the path they reach an answer by, so with
        /// that one also applied they go back to being unevaluated. Returning no answer is
        /// a limit; returning a wrong one is not, and that is what is pinned here.
        /// </summary>
        [Theory]
        [InlineData("x * (x ^ 2 + x + 1) ^ 2")]
        [InlineData("x ^ 2 * (x ^ 2 + 1) ^ 2")]
        public void AnyAnswerToThesePolynomialsIsAnAntiderivative(string integrand)
        {
            var f = integrand.ToEntity();
            var answer = f.Integrate("x");
            if (answer.Stringize().Contains("integral("))
                return;
            var derivative = answer.Differentiate("x");
            foreach (var point in new[] { 0.7, 1.3, 2.1 })
            {
                var expected = f.Substitute("x", point).EvalNumerical();
                var actual = derivative.Substitute("x", point).Substitute("C", 0).EvalNumerical();
                Assert.True((expected - actual).Abs().EDecimal.ToDouble()
                            < 1e-6 * System.Math.Max(1, expected.Abs().EDecimal.ToDouble()),
                    $"at x = {point}, d/dx of the answer is {actual.Stringize()} "
                    + $"where the integrand is {expected.Stringize()}");
            }
        }

        // The straightforward substitutions have to keep working.
        [Theory]
        [InlineData("cos(x ^ 2) * x")]
        [InlineData("x * e ^ (x ^ 2)")]
        [InlineData("sin(x) * cos(x)")]
        [InlineData("2 * x / (x ^ 2 + 1)")]
        public void PlainSubstitutionStillWorks(string integrand) =>
            AssertIsAntiderivative(integrand);

        // The closed forms the collision was hiding.
        [Theory]
        [InlineData("x * (x ^ 2 + 1) ^ 3", "C + (x ^ 2 + 1) ^ 4 / 8")]
        [InlineData("3 * x ^ 2 * (x ^ 3 + 2) ^ 2", "C + (x ^ 3 + 2) ^ 3 / 3")]
        [InlineData("x * (x ^ 2 + 1) ^ 10", "C + (x ^ 2 + 1) ^ 11 / 22")]
        public void SubstitutionReachesTheClosedForm(string integrand, string expected) =>
            Assert.Equal(MathS.Boolean.True,
                integrand.Integrate("x").Simplify()
                    .EqualTo(expected.ToEntity().Simplify()).Simplify());
    }
}
