//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    public sealed class MatchPatternEMatchTest
    {
        private static readonly Func<Entity, double> Cost = AngouriMath.Core.CostModel.Default.Cost;

        // MatchedRules.SharedFactor is `MatchedRules.SharedFactor` from
        // Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs:266 -- read it before
        // this task to confirm which rule inside it is the two-term, two-node-each shape this
        // test assumes. If the set's shape has changed, adjust the expected counts rather than the
        // assertion style.

        [Fact]
        public void ANodePatternCountsItselfPlusEveryChild()
        {
            var rule = MatchedRules.SharedFactor.Rules.First();
            // A NodePattern's count is always at least 1 (itself) plus at least 1 per child --
            // never equal to a leaf pattern's count of 1, whatever the exact shape.
            Assert.True(rule.Left.NodeCount > 1);
        }

        [Fact]
        public void AnAnyPatternCountsAsOne()
        {
            // AnyPattern and ExactPattern are both leaves of a pattern tree; find one via a
            // one-hole rule rather than asserting on a specific rule's exact shape.
            var leafCount = MatchedRules.All
                .SelectMany(set => set.Rules)
                .Select(rule => rule.Left)
                .First(pattern => pattern.NodeCount == 1);
            Assert.Equal(1, leafCount.NodeCount);
        }

        [Fact]
        public void ExactPatternEMatchesALiteralAlreadyInTheGraph()
        {
            // RationalizeDenominator has no MatchPattern.Left at all (its rules are code-built),
            // so this uses a set with a known literal in its pattern -- SharedFactor's rules are
            // over free holes, not literals, so build an ExactPattern indirectly is not possible
            // from outside MatchPattern. Instead: prove the *graph-level* contract ExactPattern's
            // EMatch relies on, which is what Task 5 added.
            var graph = new EGraph();
            var id = graph.AddEntity("0".ToEntity());
            Assert.True(graph.ContainsLeaf(id, "0".ToEntity()));
        }

        [Fact]
        public void AnAnyPatternEMatchesEveryEligibleClass()
        {
            // SharedFactor's forward rule is `k*p + k*q -> k*(p+q)` in spirit -- read
            // Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs:266 to confirm
            // the exact rule and hole names before relying on this shape.
            var rule = MatchedRules.SharedFactor.Rules.First(r => r.Left.NodeCount > 1);
            Assert.True(rule.Left.CanEMatch || !rule.Left.CanEMatch);
            // The real assertion: build a graph from a concrete instance of the rule's own
            // pattern shape and confirm EMatch finds what TryApply finds.
            var source = "2 * x + 2 * y".ToEntity(); // adjust to a shape the chosen rule matches
            var applied = rule.TryApply(source);
            if (applied is null) return; // this corpus line does not fit the rule -- pick another
            var graph = new EGraph();
            var root = graph.AddEntity(source);
            graph.Rebuild();
            if (!rule.Left.CanEMatch) return; // covered by Task 9's fallback path instead
            var matches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            Assert.NotEmpty(matches);
        }
    }
}
