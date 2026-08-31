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
        /// <b>Zero is not among them, and that is a different gap.</b> <c>0 - x</c> is negated by
        /// an earlier arm, so <c>0 - [0; 1)</c> becomes <c>-[0; 1)</c> and stops there: negating an
        /// interval is multiplying one, and <c>Mulf</c> has no interval case at all —
        /// <c>(0; 1) * 2</c> is left alone too.
        /// </summary>
        /// <remarks>
        /// Asserted as it is rather than left unmentioned, so that the boundary of this fix is
        /// written down. Multiplying an interval is
        /// <a href="https://github.com/asc-community/AngouriMath/issues/322">#322</a>'s remaining
        /// half and needs a sign analysis this does not: a negative multiplier turns the interval
        /// round exactly as subtraction does, and a zero one collapses it to a point.
        /// </remarks>
        [Fact]
        public void MultiplyingAnIntervalIsStillNotDone()
        {
            Assert.Equal("-[0; 1)".ToEntity().Simplify(), "0 - [0; 1)".ToEntity().Simplify());
            Assert.Equal("(0; 1) * 2".ToEntity(), "(0; 1) * 2".ToEntity().Simplify());
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
