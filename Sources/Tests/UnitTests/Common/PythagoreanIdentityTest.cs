//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// The Pythagorean identity was one syntactic pattern -- an adjacent sum of the two squares,
    /// in either order -- so every other arrangement of the same fact was missed, including the
    /// two forms got by dividing it through by sin^2 and by cos^2.
    /// https://github.com/asc-community/AngouriMath/issues/725
    /// </summary>
    public sealed class PythagoreanIdentityTest
    {
        /// <summary>
        /// The value is what is asserted, not the condition attached to it. A tangent and a
        /// secant are undefined where the cosine vanishes, so `sec(t)^2 - tan(t)^2` answering
        /// `1 provided not cos(t) = 0` is a better answer than a bare 1 rather than a weaker
        /// one -- the bare 1 would claim a value at a point the expression does not reach.
        /// </summary>
        private static void AssertSimplifies(string input, string expected) =>
            Assert.Equal(WithoutCondition(expected.ToEntity().Simplify()),
                         WithoutCondition(input.ToEntity().Simplify()));

        private static Entity WithoutCondition(Entity expr)
        {
            while (expr is Entity.Providedf(var inner, _)) expr = inner;
            return expr;
        }

        /// <summary>The arrangement that already worked, which must keep working.</summary>
        [Theory]
        [InlineData("sin(t) ^ 2 + cos(t) ^ 2", "1")]
        [InlineData("cos(t) ^ 2 + sin(t) ^ 2", "1")]
        [InlineData("a + sin(t) ^ 2 + cos(t) ^ 2", "1 + a")]
        [InlineData("3 * sin(t) ^ 2 + 3 * cos(t) ^ 2", "3")]
        public void TheSumOfTheSquaresIsUnaffected(string input, string expected) =>
            AssertSimplifies(input, expected);

        /// <summary>The same identity solved for one square rather than for 1.</summary>
        [Theory]
        [InlineData("1 - sin(t) ^ 2", "cos(t) ^ 2")]
        [InlineData("1 - cos(t) ^ 2", "sin(t) ^ 2")]
        [InlineData("1 - sin(t) ^ 2 + a", "cos(t) ^ 2 + a")]
        [InlineData("1 - sin(t) ^ 2 - cos(t) ^ 2", "0")]
        [InlineData("1 - cos(t) ^ 2 - sin(t) ^ 2", "0")]
        public void SolvedForOneSquare(string input, string expected) =>
            AssertSimplifies(input, expected);

        /// <summary>
        /// The identity divided through by sin^2 and by cos^2. Neither was known in any form:
        /// not the sum, not the difference, and not with the reciprocal written out as a
        /// quotient rather than as a cosecant or a secant.
        /// </summary>
        [Theory]
        [InlineData("1 + tan(t) ^ 2", "sec(t) ^ 2")]
        [InlineData("1 + cotan(t) ^ 2", "csc(t) ^ 2")]
        [InlineData("sec(t) ^ 2 - tan(t) ^ 2", "1")]
        [InlineData("csc(t) ^ 2 - cotan(t) ^ 2", "1")]
        public void DividedThroughBySquareOfSineOrCosine(string input, string expected) =>
            AssertSimplifies(input, expected);

        /// <summary>
        /// The reporter's first expression in
        /// https://github.com/asc-community/AngouriMath/issues/557. The second is not here; see
        /// <see cref="TheThirdTermOfTheIdentityHasToBeAdjacent"/> for what it still wants.
        /// </summary>
        [Fact]
        public void TheFirstComplexTrigonometricStatementOf557() =>
            AssertSimplifies("(sin(2 * t) * csc(t)) ^ 2 / 4 - cos(2 * t) - sin(t) ^ 2", "0");

        /// <summary>
        /// What the rules above do not reach, recorded rather than claimed. Every rule here
        /// matches a pattern, so the two parts of the identity have to be *adjacent* in the
        /// tree for one to fire. Written on its own each of these reduces --
        /// <c>1 + cotan(t)^2</c> is <c>csc(t)^2</c> and <c>csc(t)^2 - 1/sin(t)^2</c> is 0 --
        /// but in a sum of three terms the sorting separates the pair before the rules are
        /// tried, and nothing puts it back.
        /// <para/>
        /// Reaching these wants the identity stated over the *gathered* terms of a sum rather
        /// than over an adjacent pair, which is a larger change than this one and is left for
        /// its own. The same gap is what still stands between
        /// https://github.com/asc-community/AngouriMath/issues/557's second expression and 0.
        /// Written in sines and cosines throughout, all four already reduce, so nothing here
        /// is a wrong answer -- only an unreduced one.
        /// </summary>
        [Theory(Skip = "Wants the identity over the gathered terms of a sum, not an adjacent pair")]
        [InlineData("1 + tan(t) ^ 2 - 1 / cos(t) ^ 2", "0")]
        [InlineData("1 + cotan(t) ^ 2 - 1 / sin(t) ^ 2", "0")]
        [InlineData("1 / sin(t) ^ 2 - (1 + cotan(t) ^ 2)", "0")]
        [InlineData("(cos(2 * t) * sin(t) ^ 6 * (-1) + cos(t) * sin(t) ^ 5 * sin(2 * t)"
                    + " - sin(2 * t) ^ 2 * sin(t) ^ 4 / 4) / sin(t) ^ 8 - 1"
                    + " + (sin(2 * t) * csc(t)) ^ 2 / 4 - cos(2 * t) - sin(t) ^ 2", "0")]
        public void TheThirdTermOfTheIdentityHasToBeAdjacent(string input, string expected) =>
            AssertSimplifies(input, expected);

        /// <summary>
        /// The same statements written in sines and cosines, which do reduce. That is what makes
        /// the gap above a matter of spelling rather than of the mathematics being out of reach.
        /// <para/>
        /// The cosine half of the first pair is not among them: `1 + sin(t)^2/cos(t)^2 -
        /// 1/cos(t)^2` collapses its quotient back to a tangent and stops at
        /// `1 - 1/cos(t)^2 + tan(t)^2`, where the sine half reduces. That asymmetry is the same
        /// adjacency gap seen from the other side and is recorded, not fixed, here.
        /// </summary>
        [Theory]
        [InlineData("1 + cos(t) ^ 2 / sin(t) ^ 2 - 1 / sin(t) ^ 2", "0")]
        [InlineData("1 / sin(t) ^ 2 - (1 + cos(t) ^ 2 / sin(t) ^ 2)", "0")]
        [InlineData("(1 - sin(t) ^ 2 - cos(t) ^ 2) / sin(t) ^ 2", "0")]
        [InlineData("(1 - sin(t) ^ 2 - cos(t) ^ 2) / cos(t) ^ 2", "0")]
        public void TheSameStatementsInSinesAndCosines(string input, string expected) =>
            AssertSimplifies(input, expected);
    }
}
