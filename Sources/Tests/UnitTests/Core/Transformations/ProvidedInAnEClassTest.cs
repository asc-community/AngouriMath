//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// What an e-class does with a <see cref="Entity.Providedf"/>, which is the one question
    /// <c>Docs/Contributing/EqualitySaturationReviewFindings.md</c> leaves open and the thing
    /// standing between the rewrite graph and a production caller.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ProvidedInAnEClassTest
    {
        private static Entity? Saturate(string source, RewriteRuleGrowth ceiling)
        {
            var graph = new EGraph();
            var root = graph.AddEntity(source.ToEntity());
            var ledger = BudgetLedger.For(nameof(ProvidedInAnEClassTest),
                new WorkBudget { Steps = 200_000, Time = TimeSpan.FromSeconds(20) });
            Saturation.Run(graph, Saturation.RulesUpTo(ceiling), ledger,
                node => node.Complexity);
            return graph.Extract(root, node => node.Complexity);
        }

        /// <summary>
        /// <b>A condition is not a decoration on a value, and a union says it is.</b> Two nodes
        /// join an e-class when the graph is told they are equal. <c>p</c> and
        /// <c>p provided c</c> are <i>not</i> equal — they differ exactly where <c>c</c> is false,
        /// which is the only place the condition was written for — so a rule producing the second
        /// from the first is asking the graph to record something untrue, and extraction may then
        /// hand back either.
        /// </summary>
        [Fact]
        public void AConditionedValueIsNotTheValue()
        {
            var bare = "1".ToEntity();
            var conditioned = "1 provided not x = 0".ToEntity();

            Assert.NotEqual(bare, conditioned);
            Assert.Equal("1", bare.Substitute("x", 0).Evaled.Stringize());
            Assert.Equal("NaN", conditioned.Substitute("x", 0).Evaled.Stringize());
        }

        /// <summary>
        /// <b>No condition can enter the graph through a written pattern.</b> Which narrows the
        /// open question considerably: a <c>Providedf</c> reaches an e-class only along the
        /// code-built path, where a rule extracts a witness term, runs code on it and puts the
        /// result back.
        /// </summary>
        /// <remarks>
        /// Only 35 of the 324 rules have a right-hand side written as a pattern at all, and none
        /// of those mentions a condition — so the case
        /// <c>EqualitySaturationReviewFindings.md</c> leaves open cannot arise from e-matching as
        /// it stands. It arises from the other 289. Asserted at zero so that writing the first
        /// two-sided conditional rule fails here rather than quietly making the question live.
        /// </remarks>
        [Fact]
        public void NoWrittenPatternIntroducesACondition()
        {
            var withCondition = Saturation.RulesUpTo(RewriteRuleGrowth.Unknown)
                .Where(rule => rule.Right is not null
                               && rule.Right.ToString()!.Contains("provided"))
                .ToList();

            Assert.True(withCondition.Count == 0,
                $"{withCondition.Count} two-sided rules now mention a condition — "
                + "the e-class question is live for them, and what a union of `p` with "
                + "`p provided c` is supposed to mean has to be decided before they fire: "
                + string.Join(", ", withCondition.Select(r => r.Name)));
        }

        /// <summary>
        /// <b>The safe ceiling is nearly inert on ordinary input.</b> Four of these five come back
        /// exactly as they went in; only the Pythagorean identity moves. That is not a defect in
        /// the expressions — it is what the graph can do today with the rules it is allowed to
        /// use, and it is the measurement
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 needs
        /// before the graph is wired into anything.
        /// </summary>
        /// <remarks>
        /// <c>SafeRules</c> is <c>RulesUpTo(Rearranges)</c>, 142 of 324 rules. The 152 sitting at
        /// <c>Unknown</c> are excluded by design, since their growth was never judged — so the
        /// ceiling that is safe to run finds little, and the ceiling that finds things admits
        /// rewrites nobody measured. That trade is the open part of tier 2, and this pins where it
        /// currently sits rather than describing it.
        /// </remarks>
        [Fact]
        public void TheSafeCeilingIsNearlyInertOnOrdinaryInput()
        {
            var moved = new List<string>();
            var still = new List<string>();
            foreach (var source in new[]
            {
                "(a + b) / (a * b)", "x / x", "(x + 1) / (x + 1)",
                "sin(x) ^ 2 + cos(x) ^ 2", "(x + 1) * (x - 1)",
            })
            {
                var extracted = Saturate(source, RewriteRuleGrowth.Rearranges);
                if (extracted is null || extracted == source.ToEntity())
                    still.Add(source);
                else
                    moved.Add($"{source} -> {extracted.Stringize()}");
            }

            Assert.True(still.Count == 4 && moved.Count == 1,
                $"the safe ceiling moved {moved.Count} of 5 and left {still.Count}; it was 1 and 4.\n"
                + "moved:\n" + string.Join("\n", moved)
                + "\nunchanged:\n" + string.Join("\n", still));
        }
    }
}
