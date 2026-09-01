//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// oo - oo, and the roots of polynomials that so often produce it. Every solver reads
    /// either a polynomial or a substitution of +oo, and a root of a sum is neither, so a
    /// difference of two of them was left unevaluated however plainly it converged.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DifferenceAtInfinityTest
    {
        // Compared as numbers, since "1/2" parses as a division of two integers rather than as
        // the rational the limit answers with.
        private static void AssertLimit(string expression, string destination, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Evaled);

        // One part outgrowing the other decides the answer, which is what the ratio of the
        // two says.
        [Theory]
        [InlineData("e ^ x - x", "+oo", "+oo")]
        [InlineData("x - e ^ x", "+oo", "-oo")]
        [InlineData("x - ln(x)", "+oo", "+oo")]
        [InlineData("ln(x) - x", "+oo", "-oo")]
        [InlineData("x ^ 2 - x", "+oo", "+oo")]
        [InlineData("x - x ^ 2", "+oo", "-oo")]
        [InlineData("sqrt(x) - ln(x)", "+oo", "+oo")]
        [InlineData("e ^ x - x ^ 10", "+oo", "+oo")]
        public void TheFasterGrowingPartDecides(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        // a - b = (a^2 - b^2) / (a + b) removes the roots from the numerator, and what is
        // left of it after the leading terms cancel is an ordinary quotient.
        [Theory]
        [InlineData("sqrt(x ^ 2 + x) - x", "+oo", "1/2")]
        [InlineData("sqrt(x ^ 2 + 1) - x", "+oo", "0")]
        [InlineData("x - sqrt(x ^ 2 - x)", "+oo", "1/2")]
        [InlineData("sqrt(x + 1) - sqrt(x)", "+oo", "0")]
        [InlineData("sqrt(x ^ 2 + 4 * x) - x", "+oo", "2")]
        [InlineData("sqrt(x ^ 2 + x) + x", "-oo", "-1/2")]
        public void ADifferenceOfRootsGoesThroughItsConjugate(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        // sqrt(x^2 + x) written as x * sqrt(1 + 1/x) says its own growth, and then +oo can
        // simply be substituted into what is left. An odd degree is not covered: the growth of
        // sqrt(x^3 + x) is x^(3/2), and a quotient with that in it is one l'Hopital's rule is
        // stopped from working on, so sqrt(x^3 + x) / x^2 is still left unevaluated.
        [Theory]
        [InlineData("sqrt(x ^ 2 + x) / x", "+oo", "1")]
        [InlineData("x / sqrt(x ^ 2 + 1)", "+oo", "1")]
        [InlineData("sqrt(4 * x ^ 2 + 1) / x", "+oo", "2")]
        [InlineData("sqrt(x ^ 2 + 1) / sqrt(x ^ 2 + 3 * x)", "+oo", "1")]
        [InlineData("sqrt(x ^ 2 - x) / x", "+oo", "1")]
        [InlineData("sqrt(x ^ 4 + x) / x ^ 2", "+oo", "1")]
        [InlineData("sqrt(x ^ 6 + 1) / x ^ 3", "+oo", "1")]
        public void ARootOfAPolynomialSaysHowFastItGrows(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        // The forms that already had an answer must keep it, including the ones where the two
        // parts tend to the same infinity and so are not indeterminate at all.
        [Theory]
        [InlineData("x + sqrt(x)", "+oo", "+oo")]
        [InlineData("sqrt(x ^ 2 + x) + x", "+oo", "+oo")]
        [InlineData("x - x", "+oo", "0")]
        [InlineData("(x ^ 2 + 1) / (x ^ 2 - 1)", "+oo", "1")]
        [InlineData("1 / x - 1 / x ^ 2", "+oo", "0")]
        [InlineData("(1 + 1/x) ^ x", "+oo", "e")]
        public void EstablishedLimitsAreUnaffected(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        /// <summary>
        /// The conjugate reading cannot settle this one -- the two parts grow alike, so the
        /// ratio says nothing, and the conjugate is a quotient the solvers cannot read either.
        /// The series can: the two radicals differ at the first order in 1/x, and the
        /// difference of their leading terms is (3x - 1) / (2x), which tends to 3/2.
        /// </summary>
        /// <remarks>
        /// This asserted an unevaluated limit until the series landed, and was correct to while
        /// the conjugate was the only reading of it. It is the one test the two changes disagree
        /// about, and the disagreement is the series being right.
        /// </remarks>
        [Theory]
        [InlineData("sqrt(x ^ 2 + 3 * x) - sqrt(x ^ 2 + 1)", "3/2")]
        [InlineData("sqrt(x ^ 2 + 3 * x) - sqrt(x ^ 2 + 5 * x)", "-1")]
        public void ADifferenceOfLikeRadicalsIsSettledByTheSeries(string expression, string expected)
        {
            var task = Task.Run(() => expression.ToEntity().Limit("x", "+oo".ToEntity()).Simplify());
            Assert.True(task.Wait(LimitTermination.Guard), "the limit did not terminate");
            Assert.Equal(expected.ToEntity().Evaled, task.Result.Evaled);
        }
    }
}
