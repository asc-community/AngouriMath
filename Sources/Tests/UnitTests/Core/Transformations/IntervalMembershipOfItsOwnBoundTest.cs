//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// <c>a in (a / n; m * a)</c> is <c>a &gt; 0</c>, for every whole <c>n</c> and <c>m</c> above
    /// one — and it was answered for <c>n = 2</c> alone.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Not a rule about intervals. <c>ParaphraseInterval</c> writes the membership out as two
    /// comparisons with zero, and the difference it compares was left as a two-term sum:
    /// <c>a - a / 3</c> came back as <c>a + -a / 3</c>, which no rule about a sign can read.
    /// Collecting it gives <c>2/3 * a</c>, and a positive rational multiple of something is
    /// positive exactly when that something is.
    /// </para>
    /// <para>
    /// Collecting alone was not enough, and the reason is worth writing down: <c>Simplify</c>
    /// prunes candidates by <c>SimplifiedRate</c>, and <c>2/3 * a &gt; 0 and 2 * a &gt; 0</c>
    /// rates <b>26</b> against the membership's <b>25</b> — one point worse, so it was thrown away
    /// before anything could take it to <c>a &gt; 0</c>, which rates <b>8</b>. The <c>n = 2</c>
    /// case answered because <c>1/2 * a &gt; 0 and a &gt; 0</c> happens to rate <b>24</b>. So the
    /// positive factor is divided out where the comparison is built, and the candidate is born at
    /// its best rate rather than having to survive on the way there.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class IntervalMembershipOfItsOwnBoundTest
    {
        /// <summary>
        /// Every denominator, because the one that worked worked by coincidence. Three through
        /// eight were all left as written.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void MembershipBetweenItsOwnFractionAndItsOwnMultipleIsPositivity(int n)
            => Assert.Equal(
                "a > 0".ToEntity(),
                $"a in (a / {n}; {n} * a)".ToEntity().Simplify());

        /// <summary>
        /// And the two bounds need not agree, which is what says the answer comes from the
        /// mathematics rather than from the pair cancelling.
        /// </summary>
        [Theory]
        [InlineData("a in (a / 2; 3 * a)")]
        [InlineData("a in (a / 3; 2 * a)")]
        [InlineData("a in (a / 7; 5 * a)")]
        [InlineData("a in (0; 2 * a)")]
        public void TheTwoBoundsNeedNotAgree(string source)
            => Assert.Equal("a > 0".ToEntity(), source.ToEntity().Simplify());

        /// <summary>
        /// An interval that demands both signs at once is <b>False</b>, not merely unsimplified.
        /// Both of these were left as written before, so this is an answer where there was none.
        /// </summary>
        /// <remarks>
        /// <c>a / 2 &lt; a &lt; 0</c> wants <c>a &gt; 0</c> and <c>a &lt; 0</c>; <c>-2a &lt; a</c>
        /// wants <c>a &gt; 0</c> and <c>a &lt; -a/2</c> wants <c>a &lt; 0</c>. The condition is
        /// there because the ordering is a claim about reals.
        /// </remarks>
        [Theory]
        [InlineData("a in (a / 2; 0)")]
        [InlineData("a in (-2 * a; -a / 2)")]
        public void AnIntervalDemandingBothSignsIsEmpty(string source)
            => Assert.Equal("false provided a in RR".ToEntity(), source.ToEntity().Simplify());

        /// <summary>
        /// A bound that is not a multiple of the element keeps its own condition rather than
        /// being folded into the sign one.
        /// </summary>
        [Fact]
        public void AnUnrelatedBoundKeepsItsOwnCondition()
            => Assert.Equal(
                "a > 0 and 1 + a > 0".ToEntity(),
                "a in (a / 2; 2 * a + 1)".ToEntity().Simplify());

        /// <summary>
        /// And nothing that was already right has moved: a numeric membership is decided, and one
        /// with nothing to decide is left alone rather than guessed at.
        /// </summary>
        [Theory]
        [InlineData("3 in (1; 5)", "true")]
        [InlineData("a in (1; 2)", "a in (1; 2)")]
        [InlineData("x in [0; 1]", "x in [0; 1]")]
        [InlineData("a in (b; c)", "a in (b; c)")]
        public void TheOrdinaryCasesAreUnchanged(string source, string expected)
            => Assert.Equal(expected.ToEntity(), source.ToEntity().Simplify());
    }
}
