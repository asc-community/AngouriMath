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
    /// The indeterminate forms that are not quotients. Of the three powers, only 1^oo had a
    /// rule -- the second remarkable limit -- so 0^0 and oo^0 arrived at the descent, which
    /// substitutes each part's own limit and hands back 0^0, that is NaN. And a product of
    /// something vanishing with something diverging had no reading either unless one of the
    /// factors happened to be written as a reciprocal.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class IndeterminatePowerTest
    {
        private static Entity Limit(string expression, string destination, ApproachFrom side) =>
            expression.ToEntity().Limit("x", destination.ToEntity(), side).Simplify();

        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination, side).Evaled);

        /// <summary>
        /// 0^0. Over to the exponent, each of these is the limit of g * ln(f), which the rules
        /// for a vanishing-against-diverging product can take apart.
        /// </summary>
        [Theory]
        [InlineData("x ^ x", "0", ApproachFrom.Right, "1")]
        [InlineData("x ^ sin(x)", "0", ApproachFrom.Right, "1")]
        [InlineData("sin(x) ^ x", "0", ApproachFrom.Right, "1")]
        [InlineData("(1/x) ^ (1/x)", "+oo", ApproachFrom.Left, "1")]
        public void ZeroToTheZero(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// And 0^0 is not always 1, which is the whole reason it is indeterminate: the exponent
        /// decides, and here it decides on e.
        /// </summary>
        [Theory]
        [InlineData("x ^ (1 / ln(x))", "0", ApproachFrom.Right, "e")]
        [InlineData("x ^ (2 / ln(x))", "0", ApproachFrom.Right, "e ^ 2")]
        public void ZeroToTheZeroIsNotAlwaysOne(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        [Theory]
        [InlineData("x ^ (1/x)", "+oo", ApproachFrom.Left, "1")]
        [InlineData("x ^ (1 / ln(x))", "+oo", ApproachFrom.Left, "e")]
        [InlineData("(1/x) ^ x", "0", ApproachFrom.Right, "1")]
        public void InfinityToTheZero(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// A product of something vanishing with something diverging, written as the diverging
        /// factor over the reciprocal of the vanishing one. sin(x) * ln(x) is the one worth
        /// naming: it has no reciprocal factor at all, so the split that handles x * e^(-x) sees
        /// nothing in it.
        /// </summary>
        [Theory]
        [InlineData("sin(x) * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("x * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("sqrt(x) * ln(x)", "0", ApproachFrom.Right, "0")]
        [InlineData("x * cotan(x)", "0", ApproachFrom.Right, "1")]
        [InlineData("(1 - cos(x)) * cotan(x)", "0", ApproachFrom.Right, "0")]
        public void AVanishingFactorAgainstADivergingOne(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The forms that already had a reading must keep it. 1^oo belongs to the second
        /// remarkable limit, which answers it more directly than going through the exponent
        /// would; x * e^(-x) is read by taking the reciprocal factor out, which gives the
        /// tidier x / e^x than inverting the other half would; and a base that tends to
        /// anything else raised to a vanishing power is not indeterminate at all.
        /// </summary>
        [Theory]
        [InlineData("(1 + x) ^ (1/x)", "0", ApproachFrom.Right, "e")]
        [InlineData("(1 + 2 * x) ^ (1/x)", "0", ApproachFrom.Right, "e ^ 2")]
        [InlineData("(1 + 1/x) ^ x", "+oo", ApproachFrom.Left, "e")]
        [InlineData("x * e ^ (-x)", "+oo", ApproachFrom.Left, "0")]
        [InlineData("x ^ 4 * e ^ (-x)", "+oo", ApproachFrom.Left, "0")]
        [InlineData("(2 + x) ^ (1/x)", "0", ApproachFrom.Right, "+oo")]
        [InlineData("x ^ x", "0", ApproachFrom.Left, "1")]
        [InlineData("ln(x) / x", "+oo", ApproachFrom.Left, "0")]
        public void EstablishedLimitsAreUnaffected(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// At an infinite destination there is only one direction to come from, so these need no
        /// side of their own.
        /// </summary>
        [Theory]
        [InlineData("x ^ (1 / ln(x))", "+oo", "e")]
        [InlineData("x ^ (1/x)", "+oo", "1")]
        public void TheFormsAtInfinityNeedNoSide(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Simplify().Evaled);

        /// <summary>
        /// x^x at 0 is left with no two-sided limit, and that is right rather than a gap: x^x is
        /// not real for negative x, so there is no left-hand limit to agree with the right-hand
        /// one. The 1 that comes back from the left is the complex continuation, not a real
        /// limit, and the suite has pinned the two-sided answer as non-existent all along.
        /// </summary>
        [Fact]
        public void APowerThatIsNotRealOnOneSideHasNoTwoSidedLimit() =>
            Assert.Equal(MathS.NaN.Evaled,
                "x ^ x".ToEntity().Limit("x", 0).Simplify().Evaled);
    }
}
