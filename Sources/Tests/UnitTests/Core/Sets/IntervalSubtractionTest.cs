//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// Subtracting an interval turns it round, so its ends swap — and their openness swaps with
    /// them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>5 - (0; 1)</c> came back as <c>(5; 4)</c>: an interval whose left end is above its right,
    /// which is empty. So <c>4.5 in (5 - (0; 1))</c> answered <b>False</b> — a wrong answer reached
    /// through the operation an interval exists for.
    /// </para>
    /// <para>
    /// The openness is the half that is easy to miss. <c>5 - (0; 1]</c> is <c>[4; 5)</c>: the
    /// <i>excluded</i> 1 becomes the excluded lower end 4, and the <i>included</i> 0 becomes the
    /// included upper end 5. Swapping the ends and leaving the flags where they were would give
    /// <c>(4; 5]</c>, which is wrong at both ends and right about the width — the shape of error
    /// that a test on <c>Stringize</c> alone would not catch, which is why membership is asserted
    /// here as well.
    /// </para>
    /// </remarks>
    [Trait("Area", "Sets")]
    public sealed class IntervalSubtractionTest
    {
        [Theory]
        [InlineData("5 - (0; 1)", "(4; 5)")]
        [InlineData("5 - [0; 1]", "[4; 5]")]
        [InlineData("5 - (0; 1]", "[4; 5)")]
        [InlineData("5 - [0; 1)", "(4; 5]")]
        [InlineData("1 - (0; 1)", "(0; 1)")]
        public void SubtractingAnIntervalTurnsItRound(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());

        /// <summary>
        /// <b>Zero goes the other way round, and it works for a different reason.</b> <c>0 - x</c>
        /// is negated by an earlier arm, so <c>0 - [0; 1)</c> becomes <c>-[0; 1)</c> — a
        /// multiplication rather than a subtraction, and it reaches <c>(-1; 0]</c> through
        /// <c>Mulf</c>'s interval case.
        /// </summary>
        /// <remarks>
        /// This test asserted the opposite until that case existed: multiplying an interval was
        /// <a href="https://github.com/asc-community/AngouriMath/issues/322">#322</a>'s remaining
        /// half, and the boundary of the subtraction fix was written down here so that it would be
        /// found rather than discovered. <c>IntervalScalingTest</c> is where scaling is held now.
        /// </remarks>
        [Fact]
        public void NegatingAnIntervalGoesThroughMultiplication()
        {
            Assert.Equal("(-1; 0]".ToEntity(), "0 - [0; 1)".ToEntity().Simplify());
            Assert.Equal("(0; 2)".ToEntity(), "(0; 1) * 2".ToEntity().Simplify());
        }

        /// <summary>
        /// An interval subtracted from a number keeps its width, which the reversed form did not:
        /// a left end above the right is the empty set.
        /// </summary>
        [Theory]
        [InlineData("4.5 in (5 - (0; 1))", true)]
        [InlineData("4.1 in (5 - (0; 1))", true)]
        [InlineData("0.5 in (5 - (0; 1))", false)]
        [InlineData("5 in (5 - (0; 1))", false)]
        [InlineData("4 in (5 - (0; 1))", false)]
        [InlineData("5 in (5 - [0; 1))", true)]
        [InlineData("4 in (5 - (0; 1])", true)]
        public void MembershipIsWhatTheOrderIsFor(string expression, bool expected)
            => Assert.Equal(
                expected ? Entity.Boolean.True : Entity.Boolean.False,
                expression.ToEntity().Simplify());

        /// <summary>
        /// The direction that was already right, asserted so that fixing the other one cannot
        /// break it: an interval <i>minus</i> a number just slides, and nothing turns round.
        /// </summary>
        [Theory]
        [InlineData("(0; 1) - 5", "(-5; -4)")]
        [InlineData("[0; 1) - 5", "[-5; -4)")]
        [InlineData("(0; 1] - 5", "(-5; -4]")]
        [InlineData("(0; 1) + 1", "(1; 2)")]
        [InlineData("[0; 1) + 1", "[1; 2)")]
        public void AnIntervalLessANumberOnlySlides(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), expression.ToEntity().Simplify());
    }
}
