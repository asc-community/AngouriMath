//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Budgets
{
    /// <summary>
    /// <c>Solve</c> on a system is bounded whichever internal path takes it.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/896">#896</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// The triangularising path bounded itself and then handed what it declined to an
    /// elimination in radicals that had no budget at all — so the same call finished or did
    /// not depending on which internal path accepted it, which is worse to debug than either
    /// extreme.
    /// </para>
    /// <para>
    /// <b>What is shared between the two stages is the clock, not the ledger.</b> The Gröbner
    /// path uses one mechanism for two different things — a genuine resource ceiling, and a
    /// structural refusal like "not polynomial" — and a ledger that has recorded any ceiling
    /// refuses every later spend. Sharing the ledger itself therefore reads "declined in 8 ms
    /// because the system is uncoupled" as "the budget is gone", and the 1024-solution system
    /// below stops being answered. That was measured, not reasoned about: it is why the
    /// whole-call ledger is opened separately and only its elapsed time carries across.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class SolveIsBoundedEndToEndTest
    {
        private static Entity.Matrix? Solve(string[] equations, string[] variables)
            => MathS.Equations(equations.Select(equation => equation.ToEntity()).ToArray())
                .Solve(variables.Select(name => (Entity.Variable)MathS.Var(name)).ToArray());

        private static readonly string[] UncoupledFive =
            { "a4 - 1", "b4 - 1", "c4 - 1", "d4 - 1", "e4 - 1" };


        /// <summary>
        /// The case the issue names as the one a bound must not cost. Its quotient dimension is
        /// 1024, above the Gröbner cap of 512, so that path declines it — and the elimination
        /// answers it cheaply, because the system is uncoupled. "Gröbner declined" does not
        /// imply "hopeless", and this is what says so.
        /// </summary>
        [Fact]
        public void AnUncoupledSystemTheGroebnerPathDeclinesIsStillAnswered()
        {
            var solutions = Solve(UncoupledFive, new[] { "a", "b", "c", "d", "e" });
            Assert.NotNull(solutions);
            Assert.Equal(1024, solutions!.RowCount);
        }

        /// <summary>
        /// The half that had no budget at all: a system the Gröbner path declines reaches the
        /// elimination, and the elimination now asks before it explores.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>x + y = a, x - y = b</c> is not polynomial over the rationals in <c>x</c> and
        /// <c>y</c> alone, so the triangularising path refuses it and the elimination answers
        /// it — in two candidate solutions, measured. A ceiling of one therefore stops it, and
        /// stops it in milliseconds, which is what makes this a test rather than a wait.
        /// </para>
        /// <para>
        /// <b>The system the issue is about is deliberately not here.</b> Cyclic-6 under the
        /// default budget returns in about two minutes and declines, where before it did not
        /// return at all — but two minutes is two minutes added to every suite run, and the
        /// mechanism it would demonstrate is the one below. Its measurement is recorded in
        /// <c>BREAKING-CHANGES.md</c> instead.
        /// </para>
        /// <para>
        /// <b>The bound is cooperative and checked once per branch</b>, so a call can overshoot
        /// by the cost of the branch that was running when the budget ran out — cyclic-6's
        /// branches are individually enormous, which is why even a ceiling of twenty takes
        /// half an hour on it. That is <see cref="BudgetLedger"/>'s stated design: an algorithm
        /// that does not ask cannot be bounded, and a bound enforced from outside is a thread
        /// abort.
        /// </para>
        /// </remarks>
        [Fact]
        public void TheEliminationConsultsTheBudgetBeforeExploring()
        {
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            Assert.Throws<NotSufficientlySupportedException>(
                () => Solve(new[] { "x + y - a", "x - y - b" }, new[] { "x", "y" }));
        }

        /// <summary>
        /// The message says which of the two paths did what, because a caller who has to guess
        /// cannot act on it.
        /// </summary>
        [Fact]
        public void TheRefusalSaysWhatWasTried()
        {
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            var thrown = Assert.Throws<NotSufficientlySupportedException>(
                () => Solve(new[] { "x + y - a", "x - y - b" }, new[] { "x", "y" }));

            Assert.Contains("budget", thrown.Message);
            Assert.Contains("triangularising", thrown.Message);
            Assert.Contains("radicals", thrown.Message);
            // And what to do about it, both ways: raise the ceiling, or bound it yourself.
            Assert.Contains(nameof(MathS.Settings.Budget), thrown.Message);
            Assert.Contains("SetLocalCancellationToken", thrown.Message);
        }

        /// <summary>
        /// Nothing that answered before stops answering. These take both paths — the first two
        /// are triangularised, the third is not polynomial over the rationals and the fourth is
        /// uncoupled — and none is near any ceiling.
        /// </summary>
        [Theory]
        [InlineData(new[] { "x + y - 3", "x - y - 1" }, new[] { "x", "y" })]
        [InlineData(new[] { "x2 - 2", "y - x" }, new[] { "x", "y" })]
        [InlineData(new[] { "x + y - a", "x - y - b" }, new[] { "x", "y" })]
        [InlineData(new[] { "a4 - 1", "b4 - 1", "c4 - 1" }, new[] { "a", "b", "c" })]
        public void AnOrdinarySystemIsUnaffected(string[] equations, string[] variables)
            => Assert.NotNull(Solve(equations, variables));

        /// <summary>
        /// A caller who wants more may have it, which is what makes the bound a default rather
        /// than a limit — and is half of what the refusal's message says to do.
        /// </summary>
        [Fact]
        public void TheBoundIsACallersToRaise()
        {
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            Assert.Throws<NotSufficientlySupportedException>(
                () => Solve(UncoupledFive, new[] { "a", "b", "c", "d", "e" }));
            // And the same system answers under the default, which is the other half of
            // "a default rather than a limit" -- asserted above, in
            // AnUncoupledSystemTheGroebnerPathDeclinesIsStillAnswered.
        }
    }
}
