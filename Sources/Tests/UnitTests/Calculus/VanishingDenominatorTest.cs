//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A quotient whose divisor vanishes while its dividend does not. The descent puts each
    /// part's own limit in place of the part, and 1 / 0 says nothing about which side the
    /// divisor vanishes from, so cos(x) / sin(x) at 0 came out as NaN -- the claim that the
    /// limit does not exist -- where on the right it is +oo and on the left -oo. Only 1 / x and
    /// the other quotients of polynomials escaped it, because those the solvers read outright
    /// once x has been moved out to infinity.
    /// </summary>
    public sealed class VanishingDenominatorTest
    {
        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity(), side).Evaled);

        /// <summary>
        /// A divisor whose first derivative does not vanish at the point takes the sign of that
        /// derivative on the right and the opposite on the left.
        /// </summary>
        [Theory]
        [InlineData("1 / sin(x)", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / sin(x)", "0", ApproachFrom.Left, "-oo")]
        [InlineData("-1 / sin(x)", "0", ApproachFrom.Right, "-oo")]
        [InlineData("-1 / sin(x)", "0", ApproachFrom.Left, "+oo")]
        [InlineData("cos(x) / sin(x)", "0", ApproachFrom.Right, "+oo")]
        [InlineData("cos(x) / sin(x)", "0", ApproachFrom.Left, "-oo")]
        [InlineData("1 / tan(x)", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / tan(x)", "0", ApproachFrom.Left, "-oo")]
        [InlineData("-2 / (e ^ x - 1)", "0", ApproachFrom.Right, "-oo")]
        [InlineData("-2 / (e ^ x - 1)", "0", ApproachFrom.Left, "+oo")]
        [InlineData("2 / ln(x)", "1", ApproachFrom.Right, "+oo")]
        [InlineData("2 / ln(x)", "1", ApproachFrom.Left, "-oo")]
        public void AFirstOrderZeroTurnsTheSignAroundOnTheLeft(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// An even order does not: (x - a)^k is positive on both sides of a. 1 - cos(x) is flat
        /// at 0 and curves upward, so 1 / (1 - cos(x)) is +oo whichever way 0 is approached --
        /// and so the two-sided limit exists as well.
        /// </summary>
        [Theory]
        [InlineData("1 / (1 - cos(x))", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / (1 - cos(x))", "0", ApproachFrom.Left, "+oo")]
        [InlineData("3 / (sin(x) - 1)", "pi / 2", ApproachFrom.Right, "-oo")]
        [InlineData("3 / (sin(x) - 1)", "pi / 2", ApproachFrom.Left, "-oo")]
        public void AnEvenOrderZeroLooksTheSameFromEitherSide(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// And an odd order beyond the first turns it around again.
        /// </summary>
        [Theory]
        [InlineData("1 / x ^ 3", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / x ^ 3", "0", ApproachFrom.Left, "-oo")]
        [InlineData("cos(x) / (x - 1) ^ 3", "1", ApproachFrom.Right, "+oo")]
        [InlineData("cos(x) / (x - 1) ^ 3", "1", ApproachFrom.Left, "-oo")]
        public void AnOddOrderTurnsItAroundAgain(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        [Theory]
        [InlineData("1 / (1 - cos(x))", "0", "+oo")]
        [InlineData("1 / x ^ 2", "0", "+oo")]
        public void ATwoSidedLimitFollowsWhereBothSidesAgree(string expression, string destination, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Evaled);

        /// <summary>
        /// Where the two sides disagree there is no two-sided limit, and NaN is the right answer
        /// rather than the absence of one. These have to keep saying so.
        /// </summary>
        [Theory]
        [InlineData("1 / sin(x)", "0")]
        [InlineData("cos(x) / sin(x)", "0")]
        [InlineData("1 / x", "0")]
        [InlineData("1 / x ^ 3", "0")]
        public void TwoSidesThatDisagreeStillHaveNoLimit(string expression, string destination) =>
            Assert.Equal(
                MathS.NaN.Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Evaled);

        /// <summary>
        /// Nothing is claimed where the shape is not this one. A dividend that vanishes too
        /// makes the quotient indeterminate rather than divergent, and a divisor that is not
        /// differentiable at the point -- or whose derivative diverges there, as sqrt(x)'s does
        /// -- is left alone.
        /// </summary>
        [Theory]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "1", ApproachFrom.Left, "2")]
        [InlineData("1 / x", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / x", "0", ApproachFrom.Left, "-oo")]
        [InlineData("1 / (x - 2)", "2", ApproachFrom.Right, "+oo")]
        [InlineData("1 / (x - 2)", "2", ApproachFrom.Left, "-oo")]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "1", ApproachFrom.Right, "2")]
        [InlineData("x / (x + 1)", "0", ApproachFrom.Right, "0")]
        [InlineData("1 / x", "+oo", ApproachFrom.Left, "0")]
        [InlineData("x ^ 2 / (x ^ 2 + 1)", "+oo", ApproachFrom.Left, "1")]
        public void EstablishedLimitsAreUnaffected(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);
    }
}
