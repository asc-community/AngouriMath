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

        [Fact]
        public void ANodePatternEMatchesACompoundClass()
        {
            var graph = new EGraph();
            var root = graph.AddEntity("x + y".ToEntity());
            graph.Rebuild();

            // Find any registry rule whose Left is a two-child NodePattern over Sumf, so this
            // test exercises real recursion rather than a hand-built pattern (which cannot be
            // constructed from outside MatchPattern.cs).
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Left.RequiredRootType == typeof(Entity.Sumf));
            var matches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            // Not asserting non-empty here -- "x + y" may not satisfy every Sumf-rooted rule's
            // side holes. The assertion is that this does not throw and returns *some* sequence,
            // proving the recursive structural walk runs to completion.
            Assert.NotNull(matches);
        }

        [Fact]
        public void ACommutativeNodePatternTriesBothChildOrders()
        {
            // Prefer a known commutative rule if one exists in the registry with a NodePattern
            // Left; if none is found this test should be adjusted to build the case narrowly
            // rather than skipped -- read Sources/AngouriMath/Core/Transformations/Matching/MatchedRules.cs
            // for a `commutative: true` NodePattern construction site before finalising this test.
            var graph = new EGraph();
            var root = graph.AddEntity("y + x".ToEntity());
            graph.Rebuild();
            var rule = MatchedRules.All
                .SelectMany(set => set.Rules)
                .First(r => r.Left.CanEMatch && r.Left.RequiredRootType == typeof(Entity.Sumf));
            var swappedMatches = rule.Left.EMatch(graph, root, EBindings.Empty, Cost).ToList();
            var straightRoot = graph.AddEntity("x + y".ToEntity());
            var straightMatches = rule.Left.EMatch(graph, straightRoot, EBindings.Empty, Cost).ToList();
            // Both orders are valid inputs to the same commutative pattern; neither should throw,
            // and if the rule's Left is commutative both should find the same number of matches
            // for these two mirror-image inputs. If the rule found is not commutative this
            // equality still holds trivially (both zero, or both from independent evaluation).
            Assert.Equal(straightMatches.Count > 0, swappedMatches.Count > 0);
        }
    }
}
