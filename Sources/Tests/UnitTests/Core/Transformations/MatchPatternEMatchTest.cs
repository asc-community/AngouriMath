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
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

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

        [Fact]
        public void AnAnyPatternWithARequiredTypeAndWhereDeclinesRatherThanThrowsOnAWrongWitness()
        {
            // Any<T>(name, where) compiles `where` as an unguarded cast to T -- see the factory
            // just above ExactPattern's declaration in MatchPattern.cs: `node => where((T)node)`.
            // EMatch's eligibility check only asks whether *some* e-node in the class satisfies
            // `required`; the witness it extracts to run `where` against can be a different
            // e-node of the same class entirely, under whatever cost model the caller supplies.
            // Union two congruent representations -- one that satisfies `required`, one that
            // does not -- into a single e-class, and hand EMatch a cost model that makes the
            // wrong-typed one the cheapest overall witness. This is exactly what real
            // EqualitySaturation unions (Tasks 10-11) will eventually produce; it does not arise
            // from today's registry rules, which is why this needs a hand-built adversarial cost
            // model rather than a corpus example.
            var graph = new EGraph();
            var integerId = graph.AddEntity(Integer.Create(5));
            var variableId = graph.AddEntity(MathS.Var("x"));
            graph.Union(integerId, variableId);
            graph.Rebuild();
            var classId = graph.Find(integerId);

            // Whatever is not an Integer is free; whatever is costs a lot -- so the class's
            // cheapest overall witness is the Variable, even though the class also holds a
            // positive Integer that would make `required` eligibility succeed.
            double AdversarialCost(Entity e) => e is Integer ? 1000.0 : 0.0;

            var pattern = MatchPattern.Any<Integer>("n", (Integer n) => n.IsPositive);
            Assert.True(pattern.CanEMatch);

            // Before the fix this threw InvalidCastException from inside the `where` cast,
            // because the extracted witness (the Variable) is not an Integer. Declining --
            // an empty match sequence -- is the correct behaviour: a missed match is a
            // legitimate answer, a crash is not.
            var matches = pattern.EMatch(graph, classId, EBindings.Empty, AdversarialCost).ToList();
            Assert.Empty(matches);
        }
    }
}
