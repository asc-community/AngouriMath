//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AngouriMath.Core.Transformations;
using AngouriMath.Functions;
using AngouriMath;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The single-shot matcher against the enumerating one, on every rule in the registry and
    /// every expression to hand.
    /// </summary>
    /// <remarks>
    /// There are now two implementations of matching: <c>Match</c>, which yields every way a
    /// pattern fits, and <c>TryMatchOnce</c>, which skips the iterators for patterns that can
    /// only fit one way. The second exists because the first allocated a state machine per
    /// pattern node per attempt, which a rewrite pass pays at every node of the tree. Two
    /// implementations of one thing is how a matcher acquires a case where they differ, so this
    /// holds them together rather than trusting that they agree.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class DeterministicMatchingTest
    {
        /// <summary>
        /// Every set in <see cref="MatchedRules"/>, read off the class rather than listed.
        /// </summary>
        /// <remarks>
        /// It was a list of five, written when there were five, and it stayed five while the
        /// class grew past twenty — so the test whose subject is "every rule" was looking at a
        /// tenth of them. A list of names cannot notice that it is out of date; this can.
        /// </remarks>
        private static IEnumerable<MatchedRuleSet> AllSets
        {
            get
            {
                const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
                foreach (var property in typeof(MatchedRules).GetProperties(Any))
                    if (property.PropertyType == typeof(MatchedRuleSet))
                        yield return (MatchedRuleSet)property.GetValue(null)!;
                foreach (var factory in typeof(MatchedRules).GetMethods(Any))
                    if (factory.ReturnType == typeof(MatchedRuleSet)
                        && factory.GetParameters() is { Length: 1 } parameters
                        && parameters[0].ParameterType == typeof(TreeAnalyzer.SortLevel))
                        foreach (var level in Enum.GetValues(typeof(TreeAnalyzer.SortLevel)))
                            yield return (MatchedRuleSet)factory.Invoke(null, new[] { level })!;
            }
        }

        /// <summary>
        /// Shapes chosen to hit the rules and to miss them, including the node types they are
        /// about, so that the agreement below is over both answers rather than only over "no".
        /// </summary>
        private static readonly string[] Corpus =
        {
            "a * (1 / b)", "2 / x * y", "(2 * a) / b", "(2 / a) * b",
            "1 / (1 / x)", "x / y / z", "(x / y) / (z / w)", "1 / x + 1 / y",
            "(x ^ 2) ^ 3", "(x ^ a) ^ b", "x ^ 2", "2 ^ x",
            "a * x + a * y", "x * a + y * a", "a + b", "a - b",
            "sin(x) ^ 2 + cos(x) ^ 2", "a + sin(x) ^ 2 + cos(x) ^ 2",
            "sin(x)", "x", "17", "1/2", "x + y + z", "x * y * z",
            "sqrt(x)", "e ^ x", "log(2, x)", "x ^ 0", "0 ^ x", "-x",
        };

        /// <summary>
        /// Where a pattern says it can only fit one way, the two matchers must agree on whether
        /// it fits, and on what it bound when it did.
        /// </summary>
        [Fact]
        public void DeterministicMatchingAgreesWithEnumeration()
        {
            var checkedPairs = 0;
            foreach (var rule in AllSets.SelectMany(set => set.Rules))
            {
                if (!rule.Left.IsDeterministic) continue;
                foreach (var text in Corpus)
                {
                    var expr = text.ToEntity();
                    var enumerated = rule.Left.Match(expr, Bindings.Empty).ToList();
                    var single = rule.Left.TryMatchOnce(expr, Bindings.Empty, out var once);

                    Assert.True(enumerated.Count <= 1,
                        $"{rule.Name} calls itself deterministic but matched '{text}' "
                        + $"{enumerated.Count} ways");
                    Assert.True(single == (enumerated.Count == 1),
                        $"{rule.Name} disagreed on whether it matches '{text}'");

                    if (single)
                        foreach (var name in rule.Left.BoundNames.Distinct())
                        {
                            Assert.True(once.TryGet(name, out var fast));
                            Assert.True(enumerated[0].TryGet(name, out var slow));
                            Assert.Equal(slow, fast);
                        }
                    checkedPairs++;
                }
            }
            // A test that silently checked nothing would pass just as loudly.
            Assert.True(checkedPairs > 100, $"only {checkedPairs} pattern/expression pairs checked");
        }

        /// <summary>
        /// The same contract one step out: where a pattern can bound its candidates, walking them
        /// by index must produce exactly the sequence enumerating them produces — same bindings,
        /// same order, same count.
        /// </summary>
        /// <remarks>
        /// Order is asserted and not only membership. <c>TryApply</c> takes the first candidate
        /// that also satisfies the rule's <c>when</c>, so two implementations that agree on the
        /// <i>set</i> of matches and differ on which comes first are two different rewriters.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1079">#1079</a>
        /// </remarks>
        [Fact]
        public void BoundedMatchingAgreesWithEnumeration()
        {
            var checkedPairs = 0;
            var sawSeveral = 0;
            foreach (var rule in AllSets.SelectMany(set => set.Rules))
            {
                var choices = rule.Left.ChoiceCount;
                if (choices == MatchPattern.Unbounded) continue;
                foreach (var text in Corpus)
                {
                    var expr = text.ToEntity();
                    var enumerated = rule.Left.Match(expr, Bindings.Empty).ToList();

                    var byIndex = new List<Bindings>();
                    for (var choice = 0; choice < choices; choice++)
                        if (rule.Left.TryMatchChoice(expr, Bindings.Empty, choice, out var bound))
                            byIndex.Add(bound);

                    Assert.True(enumerated.Count == byIndex.Count,
                        $"{rule.Name} on '{text}': enumeration found {enumerated.Count} matches, "
                        + $"indexing found {byIndex.Count}");
                    for (var i = 0; i < enumerated.Count; i++)
                        foreach (var name in rule.Left.BoundNames.Distinct())
                        {
                            Assert.True(enumerated[i].TryGet(name, out var slow));
                            Assert.True(byIndex[i].TryGet(name, out var fast));
                            Assert.Equal(slow, fast);
                        }
                    if (enumerated.Count > 1) sawSeveral++;
                    checkedPairs++;
                }
            }
            Assert.True(checkedPairs > 500, $"only {checkedPairs} pattern/expression pairs checked");
            // The whole point is the pattern that matches more than one way. If none of them did,
            // this would be the deterministic test again under another name.
            Assert.True(sawSeveral > 0, "no bounded pattern matched an expression more than one way");
        }

        // That the sets still *rewrite* the same thing end to end is not restated here:
        // MatchedRulesAgreeWithTheSwitchTest already runs both the data sets and the `switch`
        // they mirror over generated expressions and requires them to agree, and it now goes
        // through the fast path to do it. A second copy of that check, with the right-hand
        // sides reached from a test, would mean opening them up for no gain in coverage.

        /// <summary>
        /// The classification itself: a pattern with a choice in it must not claim otherwise,
        /// and a node inherits the indeterminacy of anything below it.
        /// </summary>
        [Fact]
        public void AChoiceAnywhereMakesThePatternASearch()
        {
            Assert.True(MatchPattern.Any("x").IsDeterministic);
            Assert.True(MatchPattern.Node<Powf>(
                MatchPattern.Any("x"), MatchPattern.Any("y")).IsDeterministic);

            var commutative = MatchPattern.Commutative<Sumf>(
                MatchPattern.Any("x"), MatchPattern.Any("y"));
            Assert.False(commutative.IsDeterministic);

            // ...and buried one level down, which is the case a per-node flag would get wrong.
            Assert.False(MatchPattern.Node<Powf>(commutative, MatchPattern.Any("n"))
                .IsDeterministic);

            Assert.False(MatchPattern.Gathered<Sumf>("rest", MatchPattern.Any("x"),
                MatchPattern.Any("y")).IsDeterministic);
        }
    }
}
