//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Budgets;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Budgets
{
    /// <summary>
    /// The budget object — <a href="https://github.com/asc-community/AngouriMath/issues/373">#373</a>
    /// — and the reason surviving to the caller
    /// — <a href="https://github.com/asc-community/AngouriMath/issues/896">#896</a>.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class BudgetTest
    {
        static Entity[] Eqs(params string[] equations)
            => equations.Select(equation => (Entity)equation).ToArray();

        static Variable[] Vars(params string[] names)
            => names.Select(name => (Variable)name).ToArray();

        static BudgetOutcome Solve(string[] equations, string[] variables)
        {
            using var recording = BudgetRecording.Start();
            try
            {
                _ = MathS.Equations(Eqs(equations)).Solve(Vars(variables));
            }
            catch (Exception exception) when (exception is not Xunit.Sdk.XunitException)
            {
                // What the fall-through does with a system the bounded path declined is not
                // what these assert; the outcome is recorded before it is reached either way.
            }
            return Assert.Single(recording.Outcomes);
        }

        [Fact]
        public void ABudgetIsAValue()
        {
            Assert.Equal(new WorkBudget { Steps = 10 }, new WorkBudget { Steps = 10 });
            Assert.NotEqual(new WorkBudget { Steps = 10 }, new WorkBudget { Steps = 11 });
            var relaxed = WorkBudget.Unlimited with { Time = TimeSpan.FromSeconds(1) };
            Assert.Equal(long.MaxValue, relaxed.Steps);
            Assert.Null(WorkBudget.Unlimited.Time);
        }

        /// <summary>
        /// Leaving the setting alone leaves each algorithm on the budget it chose for itself,
        /// which is not the same as an unlimited one.
        /// </summary>
        [Fact]
        public void AnUnsetBudgetLeavesTheAlgorithmsOwn()
        {
            Assert.False(MathS.Settings.Budget.IsOverriden);
            var outcome = Solve(new[] { "x - y + 3", "y + 2" }, new[] { "x", "y" });
            Assert.True(outcome.Completed);
            Assert.True(outcome.IsDeterministic);
        }

        /// <summary>
        /// The defect in <a href="https://github.com/asc-community/AngouriMath/issues/896">#896</a>:
        /// declining because a system is not polynomial and declining because a ceiling was
        /// reached were the same <see langword="false"/>. They are now different words.
        /// </summary>
        [Theory]
        [InlineData("not polynomial", new[] { "sin(x) + y", "y - 1" }, new[] { "x", "y" })]
        [InlineData("quotient dimension",
            new[] { "a^4-1", "b^4-1", "c^4-1", "d^4-1", "e^4-1" }, new[] { "a", "b", "c", "d", "e" })]
        public void TheReasonForDecliningIsNamed(string reason, string[] equations, string[] variables)
        {
            var outcome = Solve(equations, variables);
            Assert.Equal(reason, outcome.Reason);
            Assert.False(outcome.Completed);
            Assert.True(outcome.IsDeterministic);
        }

        /// <summary>
        /// A budget in work units is honoured, and it is the ceiling that is reported rather
        /// than the clock.
        /// </summary>
        [Fact]
        public void AStepCeilingIsHonoured()
        {
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            var outcome = Solve(new[] { "x - y + 3", "y + 2" }, new[] { "x", "y" });
            Assert.Equal("steps", outcome.Reason);
            Assert.True(outcome.IsDeterministic);
            Assert.Equal(2, outcome.Steps);
        }

        /// <summary>
        /// The whole point of counting work rather than reading a clock: the same input costs
        /// the same, so where it stops does not depend on the machine or on what else is
        /// running on it.
        /// </summary>
        [Fact]
        public void TheStepCountIsAFunctionOfTheInput()
        {
            var first = Solve(new[] { "x^2 - 2", "y - x" }, new[] { "x", "y" });
            var second = Solve(new[] { "x^2 - 2", "y - x" }, new[] { "x", "y" });
            Assert.Equal(first.Steps, second.Steps);
            Assert.Equal(first.Reason, second.Reason);
        }

        /// <summary>
        /// Exhausting the bounded path is not an answer — the caller carries on and gets the
        /// same solutions it always did.
        /// </summary>
        [Fact]
        public void ExhaustionDoesNotChangeTheAnswer()
        {
            var withoutBudget = MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y"));
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            var withBudget = MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y"));
            Assert.Equal(withoutBudget, withBudget);
        }

        /// <summary>
        /// The first ceiling to fire is the one reported: whatever the algorithm asks on its
        /// way out cannot overwrite the reason it is leaving.
        /// </summary>
        [Fact]
        public void TheFirstReasonIsTheOneKept()
        {
            using var _ = MathS.Settings.Budget.Set(new WorkBudget { Steps = 1 });
            var outcome = Solve(
                new[] { "a^4-1", "b^4-1", "c^4-1", "d^4-1", "e^4-1" }, new[] { "a", "b", "c", "d", "e" });
            Assert.Equal("steps", outcome.Reason);
        }

        /// <summary>
        /// Recordings nest, so a caller who records a subcomputation does not silently add its
        /// outcomes to somebody else's list.
        /// </summary>
        [Fact]
        public void RecordingsNest()
        {
            using var outer = BudgetRecording.Start();
            using (var inner = BudgetRecording.Start())
            {
                _ = MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y"));
                Assert.NotEmpty(inner.Outcomes);
            }
            Assert.Empty(outer.Outcomes);
        }

        /// <summary>
        /// A closed recording collects nothing, so a scope that has been disposed cannot go on
        /// gathering another caller's work.
        /// </summary>
        [Fact]
        public void AClosedRecordingCollectsNothing()
        {
            var recording = BudgetRecording.Start();
            recording.Dispose();
            recording.Dispose();
            _ = MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y"));
            Assert.Empty(recording.Outcomes);
        }

        /// <summary>
        /// With no recording open nothing is collected and nothing throws — the path a caller
        /// who never mentions budgets takes.
        /// </summary>
        [Fact]
        public void NoRecordingIsTheOrdinaryCase()
            => Assert.NotNull(MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y")));

        [Fact]
        public void ExhaustedListsOnlyWhatStopped()
        {
            using var recording = BudgetRecording.Start();
            _ = MathS.Equations(Eqs("x - y + 3", "y + 2")).Solve(Vars("x", "y"));
            try { _ = MathS.Equations(Eqs("sin(x) + y", "y - 1")).Solve(Vars("x", "y")); }
            catch (Exception exception) when (exception is not Xunit.Sdk.XunitException) { }
            Assert.Equal(2, recording.Outcomes.Count);
            Assert.Equal("not polynomial", Assert.Single(recording.Exhausted).Reason);
        }
    }
}
