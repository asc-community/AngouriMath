//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A rule's <see cref="RewriteRuleGrowth"/> is a claim about every expression it fires on:
    /// <c>Collects</c> says the replacement is smaller, <c>Rearranges</c> that it is the same
    /// size, <c>Expands</c> that it is larger. Here that claim is run against a generated corpus
    /// instead of being taken on trust.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It is worth checking because growth is <b>load-bearing</b> and not documentation:
    /// <c>Saturation.RulesUpTo</c> selects by it, so a rule declaring <c>Collects</c> or
    /// <c>Rearranges</c> is one equality saturation fires and a rule left at <c>Unknown</c> is
    /// one it does not. A declaration that is wrong in the shrinking direction tells the
    /// saturation a rewrite is safe to run when it is not.
    /// </para>
    /// <para>
    /// A corpus can only refute, never confirm: firing on fourteen hundred expressions without
    /// contradicting a claim is evidence for it and not proof of it, since the claim is about
    /// every expression there is. So this fails on a contradiction and says nothing about a rule
    /// it could not make fire.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleGrowthAgreesWithTheCorpusTest
    {
        private static readonly string[] Leaves =
            { "x", "y", "2", "-1", "-2", "1/2", "-1/2", "1", "0" };

        private static readonly string[] Unary =
            { "-({0})", "1 / ({0})", "({0}) ^ 2", "({0}) ^ (-2)", "sqrt({0})", "sin({0})", "abs({0})" };

        private static readonly string[] Binary =
            { "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})" };

        /// <summary>
        /// The same shape as the corpus <c>MatchedRulesAgreeWithTheSwitchTest</c> builds, and for
        /// the same reason: generated inputs reach arrangements nobody would think to write down.
        /// </summary>
        private static List<Entity> Corpus()
        {
            var level1 = new List<string>(Leaves);
            var level2 = new List<string>();
            foreach (var shape in Unary)
                foreach (var inner in level1)
                    level2.Add(string.Format(shape, inner));
            foreach (var shape in Binary)
                foreach (var left in level1)
                    foreach (var right in level1)
                        level2.Add(string.Format(shape, left, right));
            var level3 = new List<string>();
            foreach (var shape in Binary)
                foreach (var left in level2.Where((_, i) => i % 17 == 0))
                    foreach (var right in level2.Where((_, i) => i % 23 == 0))
                        level3.Add(string.Format(shape, left, right));

            var parsed = new List<Entity>();
            foreach (var source in level1.Concat(level2).Concat(level3))
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the generator makes some strings the parser declines */ }
            }
            return parsed;
        }

        private static int Size(Entity entity) => entity.Nodes.Count();

        /// <summary>
        /// Every firing of every rule over the corpus, as (rule, how the size moved). A rule that
        /// hands back what it was given has not fired and is not counted — the factorial sets
        /// have arms that match a shape and then decline.
        /// </summary>
        private static Dictionary<MatchedRule, List<(Entity Before, Entity After)>> Firings()
        {
            var corpus = Corpus();
            var firings = new Dictionary<MatchedRule, List<(Entity, Entity)>>();
            foreach (var set in MatchedRules.All)
                foreach (var rule in set.Rules)
                {
                    var seen = new List<(Entity, Entity)>();
                    foreach (var expr in corpus)
                    {
                        Entity? after;
                        try { after = rule.TryApply(expr); }
                        catch { continue; /* a rule declining loudly is not this test's subject */ }
                        if (after is not null && after != expr)
                            seen.Add((expr, after));
                    }
                    firings[rule] = seen;
                }
            return firings;
        }

        [Fact]
        public void NoDeclaredGrowthIsContradictedByTheCorpus()
        {
            var wrong = new List<string>();
            foreach (var (rule, firings) in Firings())
            {
                if (rule.Growth is RewriteRuleGrowth.Unknown || firings.Count == 0)
                    continue;
                foreach (var (before, after) in firings)
                {
                    var delta = Size(after) - Size(before);
                    var contradicts = rule.Growth switch
                    {
                        RewriteRuleGrowth.Collects => delta >= 0,
                        RewriteRuleGrowth.Rearranges => delta != 0,
                        RewriteRuleGrowth.Expands => delta <= 0,
                        _ => false
                    };
                    if (contradicts)
                    {
                        wrong.Add($"{rule.Name} declares {rule.Growth} but {before} -> {after} "
                                  + $"moves {Size(before)} nodes to {Size(after)}");
                        break;
                    }
                }
            }

            Assert.True(wrong.Count == 0,
                $"{wrong.Count} rules contradicted by the corpus:\n" + string.Join("\n", wrong));
        }

        /// <summary>
        /// The corpus has to actually exercise the declarations, or the test above passes by
        /// never running. Named as a count that moves rather than as "most of them".
        /// </summary>
        [Fact]
        public void TheCorpusExercisesTheDeclarationsItChecks()
        {
            var fired = Firings().Where(pair => pair.Value.Count > 0).ToList();
            var declared = fired.Count(pair => pair.Key.Growth is not RewriteRuleGrowth.Unknown);

            // Named as counts that move rather than as "most of them". 84 rules reach the corpus,
            // 18 of them carrying a declaration -- so the check above is exercised, and the other
            // 66 are the measured evidence for declaring more of them.
            Assert.True(fired.Count >= 80, $"only {fired.Count} rules fired at all");
            Assert.True(declared >= 15, $"only {declared} rules with a declared growth fired");
        }
    }
}
