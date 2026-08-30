//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// Where two arms of one rule set both fire at a node, do they agree?
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 asks for
    /// "rule priorities and conflict resolution, with confluence and termination checked by tooling
    /// rather than asserted by authors". This is the confluence half, at rule grain.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <c>switch</c> takes the first arm that matches. Where a later arm would also have fired
    /// and would have produced something else, the <b>order of the arms is load-bearing</b> — a
    /// decision somebody made, and one that inserting an arm above it silently reverses. Nothing
    /// in the source says which orderings are load-bearing and which are free, so this finds them
    /// and the list below is where they are written down.
    /// </para>
    /// <para>
    /// Only askable at all because the registry carries individual arms
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>): the same
    /// question at set grain — whether applying A then B lands where B then A does — is a
    /// different and coarser one.
    /// </para>
    /// <para>
    /// <b>A sample, not a proof.</b> Two arms that never overlap on the generated input say
    /// nothing either way, and are not recorded as agreeing.
    /// </para>
    /// <para>
    /// <b>And a shallower sample than it reads as.</b> The third level below is grown with unary
    /// shapes only, so this corpus never builds a quotient of quotients or a product of quotients —
    /// which is where a special rule and the general rule that would swallow it meet.
    /// <c>RulePriorityTest</c> asks the same question of the same rules written as data, over a
    /// corpus grown with binary shapes at every level, and finds <b>45</b> conflicts where this
    /// finds three. It also names them as the rules they are between rather than as the indices
    /// below, which is what the note on
    /// <see cref="AConflictIsReportableAsThePatternsItIsBetween"/> asks for; a data rule has a name
    /// and a <c>switch</c> arm does not.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleConfluenceTest
    {
        private static readonly string[] Leaves = { "x", "y", "2", "-1", "1/2", "0", "1", "3" };

        private static readonly string[] Unary =
        {
            "-({0})", "sqrt({0})", "abs({0})", "ln({0})", "e ^ ({0})",
            "sin({0})", "cos({0})", "tan({0})", "sgn({0})", "1 / ({0})",
            "({0}) ^ 2", "({0}) ^ (-1)", "({0}) ^ (1/2)", "({0})!",
        };

        private static readonly string[] Binary =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
        };

        private static List<string> Grow(IReadOnlyList<string> below, bool binary)
        {
            var grown = new List<string>();
            foreach (var shape in Unary)
                foreach (var inner in below)
                    grown.Add(string.Format(shape, inner));
            if (binary)
                foreach (var shape in Binary)
                    foreach (var left in below)
                        foreach (var right in below)
                            grown.Add(string.Format(shape, left, right));
            return grown;
        }

        /// <summary>
        /// Generated rather than listed, because the overlaps worth finding are the ones nobody
        /// thought to write down. Deterministic, so a failure reproduces.
        /// </summary>
        private static List<Entity> Expressions()
        {
            var level1 = new List<string>(Leaves);
            var level2 = Grow(level1, binary: true);
            var level3 = Grow(level2.Where((_, i) => i % 11 == 0).ToList(), binary: false);
            var parsed = new List<Entity>();
            foreach (var source in level1.Concat(level2).Concat(level3))
            {
                // Not every generated string parses, and that is the generator's business.
                try { parsed.Add(source.ToEntity()); }
                catch (Exception) { }
            }
            return parsed;
        }

        /// <summary>
        /// The arms that <i>change</i> this node. An arm that matches and hands the node back has
        /// not fired — the factorial arms do that deliberately, on a quotient too far apart to be
        /// worth writing out — so an unchanged result is not an overlap.
        /// </summary>
        private static List<int> FiringAt(RewriteRuleSet set, Entity node)
        {
            var firing = new List<int>();
            for (var i = 0; i < set.Rules.Count; i++)
            {
                Entity? applied;
                try { applied = set.Rules[i].TryApply(node); }
                catch (Exception) { continue; }
                if (applied is not null && !applied.Equals(node))
                    firing.Add(i);
            }
            return firing;
        }

        /// <summary>
        /// How an arm is referred to: <b>by name where it has one</b>, and by its index where the
        /// only name it has is its own rendered pattern.
        /// </summary>
        /// <remarks>
        /// This is what the note on <see cref="AConflictIsReportableAsThePatternsItIsBetween"/>
        /// asks for — "an index moves whenever somebody edits the <c>switch</c>, which is exactly
        /// when this test fires" — and it became possible one set at a time, as the registry was
        /// repointed at the rules it runs. It is not hypothetical: repointing <c>Power</c> moved
        /// its arms from 35 to 31 and invalidated two recorded orderings that named nothing but
        /// numbers. A set still described by <c>RuleRegistryGenerator</c> has no name to give, so
        /// it keeps the index and will stop needing to when it is repointed.
        /// </remarks>
        private static string Ident(RewriteRuleSet set, int index)
            => Explanation.IsProse(set.Rules[index].Name) ? set.Rules[index].Name : index.ToString();

        /// <summary>
        /// Every pair of arms of one set observed to fire at the same node and disagree about the
        /// result, as <c>Set[earlier,later]</c>.
        /// </summary>
        private static SortedSet<string> ConflictingPairs(out Dictionary<string, string> examples)
        {
            var conflicts = new SortedSet<string>(StringComparer.Ordinal);
            examples = new Dictionary<string, string>(StringComparer.Ordinal);
            var expressions = Expressions();
            foreach (var set in RewriteRules.All.Where(set => set.Rules.Count > 0))
                foreach (var expression in expressions)
                    foreach (var node in expression.Nodes)
                    {
                        var firing = FiringAt(set, node);
                        if (firing.Count < 2)
                            continue;
                        // Compared after normalisation, so that two arms writing one answer two
                        // ways are not called a conflict.
                        var settled = firing.ToDictionary(i => i, i => set.Rules[i].TryApply(node)!.InnerSimplified);
                        foreach (var i in firing)
                            foreach (var j in firing)
                            {
                                if (i >= j || settled[i].Equals(settled[j]))
                                    continue;
                                var key = $"{set.Name}[{Ident(set, i)},{Ident(set, j)}]";
                                conflicts.Add(key);
                                if (!examples.ContainsKey(key))
                                    examples[key] = $"{node.Stringize()} -> {settled[i].Stringize()} "
                                        + $"vs {settled[j].Stringize()}";
                            }
                    }
            return conflicts;
        }

        /// <summary>
        /// The orderings that are load-bearing, by name. Each is a pair of arms that both fire and
        /// disagree, so the earlier one wins and the choice is real.
        /// </summary>
        /// <remarks>
        /// None of the three changes what <see cref="Entity.Simplify(int)"/> returns — the
        /// normalisation and the passes after it converge, so <c>x * 1/2</c> is <c>x / 2</c> either
        /// way. They are recorded because that is a fact about the current arms and not a
        /// guarantee: an arm inserted above one of these changes an answer, and without this list
        /// nothing would notice.
        /// <para/>
        /// <b>All three name their arms now.</b> They read <c>Common[12,89]</c>,
        /// <c>Power[18,19]</c> and <c>Power[6,19]</c> until each set was described by the rules it
        /// runs rather than by the <c>switch</c> it had stopped running
        /// (<a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a>) — and
        /// repointing <c>Power</c> moved its arms from 35 to 31 and invalidated two of them on the
        /// spot, which is exactly the failure the note on
        /// <see cref="AConflictIsReportableAsThePatternsItIsBetween"/> predicted and could not do
        /// anything about while the only name an arm had was its own rendered pattern.
        /// </remarks>
        [Fact]
        public void OnlyTheRecordedArmOrderingsAreLoadBearing()
        {
            var conflicts = ConflictingPairs(out var examples);

            var recorded = new[]
            {
                // x * 1/2 -> 1/2 * x (the sort) rather than x / 2 (the quotient). Both settle to
                // x / 2 once InnerSimplified has run.
                "Common[a-variable-times-a-number-puts-the-number-first,a-reciprocal-rational-factor-is-a-division]",
                // (-x) ^ (-1) -> -1 / x rather than 1 / (-x).
                "Power[a-numeric-factor-comes-out-of-a-power-of-a-product,a-reciprocal-power-is-a-quotient]",
                // (e ^ y) ^ (-1) -> e ^ (y * (-1)) rather than 1 / e ^ y.
                "Power[a-power-of-a-power-multiplies-the-exponents,a-reciprocal-power-is-a-quotient]",
            };

            Assert.Equal(
                recorded.OrderBy(name => name, StringComparer.Ordinal),
                conflicts);
            // Not an assertion about the list above -- an assertion that the list is worth having.
            // A change that made every arm pair agree would pass the comparison above only by
            // being recorded here too, and this says so out loud.
            Assert.All(recorded, name => Assert.True(examples.ContainsKey(name),
                $"{name} is recorded as load-bearing and did not overlap on the generated input"));
        }

        /// <summary>
        /// The failure message has to name the arms, not their indices — an index moves whenever
        /// somebody edits the <c>switch</c>, which is exactly when this test fires.
        /// </summary>
        [Fact]
        public void AConflictIsReportableAsThePatternsItIsBetween()
        {
            var conflicts = ConflictingPairs(out var examples);
            foreach (var conflict in conflicts)
            {
                var name = conflict.Substring(0, conflict.IndexOf('['));
                var arms = conflict.Substring(conflict.IndexOf('[') + 1).TrimEnd(']').Split(',');
                var set = Assert.Single(RewriteRules.All.Where(s => s.Name == name));
                foreach (var arm in arms)
                {
                    var rule = int.TryParse(arm, out var index)
                        ? set.Rules[index]
                        : Assert.Single(set.Rules.Where(r => r.Name == arm));
                    Assert.NotEmpty(rule.PatternSource);
                }
                Assert.NotEmpty(examples[conflict]);
            }
        }
    }
}
