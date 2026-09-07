//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The rewrite graph's first production caller: the perfect-square collapse asks
    /// <see cref="Saturation.ProvesEqual"/> whether a cross term equals its candidate before it
    /// asks <c>Simplify</c>. These are the two facts that make that safe and worth doing.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Measured, per stage, on the inputs the rule fires on.</b> Where the graph proves, it is
    /// faster than the symbolic test it replaces — 0.55 ms against 1.7–5.1 ms for
    /// <c>2 * sqrt(2) * sqrt(3)</c> against <c>2 * sqrt(6)</c>, and nothing at all where the
    /// two terms are already one tree. Where it does not prove it spends 0.24 ms and the
    /// symbolic test runs as before. And at the level of a whole <c>Simplify</c> the change is
    /// inside the noise, because the collapse's cost is elsewhere: 3–163 ms per call, in the
    /// candidate's own <c>Simplify</c> and the numeric checks. So this is a production caller
    /// that meets the standing condition — it runs only after the numeric pre-filter, bounded —
    /// and not a speed-up to advertise.
    /// </para>
    /// <para>
    /// <b>The graph refuses the identity the site's remarks call false.</b>
    /// <c>sqrt(x) * sqrt(y) = sqrt(x * y)</c> fails on the branch cuts, and the one rule that
    /// could assert it is guarded to whole exponents and positive real bases. A proof engine that
    /// proved it would make the numeric disposer the only thing between this rule and a wrong
    /// answer; asserted here so that a loosened guard fails before it reaches the rule.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class PerfectSquareProofTest
    {
        /// <summary>
        /// The production caller's step ceiling with a wall that cannot fire: the first CI run
        /// of this test spent the caller's fifty milliseconds on a cold JIT three seconds into
        /// the process and reported the surd product unproved. Steps are what bound a proof;
        /// a wall in a test measures the runner.
        /// </summary>
        private static readonly WorkBudget Budget = new() { Steps = 2_000, Time = TimeSpan.FromSeconds(30) };

        [Theory]
        [InlineData("2 * sqrt(2) * sqrt(3)", "2 * sqrt(6)")]
        [InlineData("2 * sqrt(2) * sqrt(3)", "2 * (sqrt(2) * sqrt(3))")]
        public void TheSafeCeilingProvesTheCrossTerm(string cross, string candidate)
            => Assert.True(Saturation.ProvesEqual(cross.ToEntity(), candidate.ToEntity(), Saturation.SafeRules, Budget));

        /// <summary>
        /// And what it does not prove, which is why the symbolic test is a fallback rather than a
        /// memory: no rule at the safe ceiling commutes a product of two symbolic roots, so the
        /// same cross term regrouped is two trees the graph never joins. The constant case above
        /// joins only because the fold takes both to <c>2 * sqrt(6)</c>.
        /// </summary>
        [Fact]
        public void TheSafeCeilingDoesNotRegroupASymbolicProduct()
            => Assert.False(Saturation.ProvesEqual("2 * sqrt(a) * sqrt(b)".ToEntity(), "2 * (sqrt(a) * sqrt(b))".ToEntity(), Saturation.SafeRules, Budget));

        [Theory]
        [InlineData("2 * sqrt(x) * sqrt(y)", "2 * sqrt(x * y)")]
        [InlineData("sqrt(x) * sqrt(y)", "sqrt(x * y)")]
        public void TheSafeCeilingDoesNotProveTheBranchCutIdentity(string left, string right)
            => Assert.False(Saturation.ProvesEqual(left.ToEntity(), right.ToEntity(), Saturation.SafeRules, Budget));

        [Theory]
        [InlineData("2 + 2 * sqrt(2) * sqrt(3) + 3", "(sqrt(3) + sqrt(2)) ^ 2")]
        [InlineData("2 + 2 * sqrt(6) + 3", "(sqrt(3) + sqrt(2)) ^ 2")]
        [InlineData("a + 2 * sqrt(a) * sqrt(b) + b", "(sqrt(a) + sqrt(b)) ^ 2")]
        public void TheCollapseStillAnswersWhatItAnswered(string sum, string square)
            => Assert.Equal(square.ToEntity(), Patterns.CollapseToPerfectSquare(sum.ToEntity()));

        [Fact]
        public void TheCollapseStillDeclinesTheBranchCutShape()
            => Assert.Null(Patterns.CollapseToPerfectSquare("x + 2 * sqrt(x * y) + y".ToEntity()));
    }
}
