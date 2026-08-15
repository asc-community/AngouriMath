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
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The generated rules have to be the <c>switch</c> they were generated from, arm for arm.
    /// https://github.com/asc-community/AngouriMath/issues/825
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the check that makes the generator worth trusting, and it is why the design is a
    /// generator rather than a hand transcription: the arms are copied as syntax, so a rule
    /// <i>cannot</i> say something its arm does not — and this test says so out loud, over
    /// generated input, at every node rather than only at the root. Most rules match a shape that
    /// only ever occurs as a subexpression, so a root-only comparison would exercise a handful of
    /// them and report green.
    /// </para>
    /// <para>
    /// The comparison is per node and three-way: which rule the set says fired, what that rule
    /// produces, and what the <c>switch</c> produces. Agreeing on the answer while disagreeing on
    /// which rule gave it would make every derivation built on this wrong in a way no output
    /// comparison notices.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class AddressableRulesTest
    {
        /// <summary>Each addressable set, next to the method its arms were generated from.</summary>
        private static IEnumerable<(RewriteRuleSet Set, Func<Entity, Entity> Switch)> Addressable()
        {
            yield return (RewriteRules.Common, Patterns.CommonRules);
            yield return (RewriteRules.DivisionPreparing, Patterns.DivisionPreparingRules);
            yield return (RewriteRules.NumericNeat, Patterns.NumericNeatRules);
            yield return (RewriteRules.InvertNegativeMultipliers, Patterns.InvertNegativeMultipliers);
            yield return (RewriteRules.Power, Patterns.PowerRules);
            yield return (RewriteRules.Expansion, Patterns.ExpandRules);
            yield return (RewriteRules.Factorization, Patterns.FactorizeRules);
            yield return (RewriteRules.Trigonometric, Patterns.TrigonometricRules);
            yield return (RewriteRules.NormalTrigonometricForm, Patterns.NormalTrigonometricForm);
            yield return (RewriteRules.CollapseTrigonometricFunctions, Patterns.CollapseTrigonometricFunctions);
            yield return (RewriteRules.ExpandTrigonometric, Patterns.ExpandTrigonometricRules);
            yield return (RewriteRules.ExpandMultipleAngle, Patterns.ExpandMultipleAngleRules);
            yield return (RewriteRules.Boolean, Patterns.BooleanRules);
            yield return (RewriteRules.InequalityEquality, Patterns.InequalityEqualityRules);
            yield return (RewriteRules.SetOperator, Patterns.SetOperatorRules);
            yield return (RewriteRules.PhiFunction, Patterns.PhiFunctionRules);
        }

        public static IEnumerable<object[]> AddressableSets()
            => Addressable().Select(pair => new object[] { pair.Set.Name });

        private static Func<Entity, Entity> SwitchFor(string setName)
            => Addressable().First(pair => pair.Set.Name == setName).Switch;

        private static RewriteRuleSet SetFor(string setName)
            => Addressable().First(pair => pair.Set.Name == setName).Set;

        private static readonly string[] Leaves =
            { "x", "y", "2", "-1", "1/2", "1", "0", "pi", "x > 0", "true", "false", "{ 1, 2 }" };

        private static readonly string[] Unary =
        {
            "-({0})", "1 / ({0})", "({0}) ^ 2", "sqrt({0})", "sin({0})", "cos({0})", "tan({0})",
            "abs({0})", "ln({0})", "not ({0})", "({0})!", "sgn({0})", "phi({0})",
        };

        private static readonly string[] Binary =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
            "({0}) and ({1})", "({0}) or ({1})", "({0}) = ({1})", "({0}) >= ({1})",
            "({0}) unionwith ({1})", "log({0}, {1})",
        };

        /// <summary>
        /// Shapes a small grammar does not reach, one per rule set that the grammar alone leaves
        /// untouched. Generated input finds what nobody wrote down; a rule that only fires on
        /// <c>sin(x)*cos(x)</c> is found by writing <c>sin(x)*cos(x)</c> down. Neither subsumes
        /// the other, which is why both are here.
        /// </summary>
        private static readonly string[] Seeds =
        {
            // Trigonometric
            "sin(x) * cos(x)", "cos(x) * sin(x)", "arcsin(x) + arccos(x)", "arccos(x) + arcsin(x)",
            "arctan(x) + arccotan(x)", "arctan(1/2) + arctan(1/3)", "sin(x) / cos(x)",
            "sin(x) ^ 2 + cos(x) ^ 2", "cos(x) ^ 2 + sin(x) ^ 2", "tan(x) * cotan(x)",
            // ExpandRules, which is what RewriteRules.Expansion is
            "sin(x + y)", "sin(x - y)", "sin(2 + y)",
            // ExpandTrigonometric
            "1/2 * sin(2 * x)", "cos(2 * x)", "cos(2 * y)",
            // ExpandMultipleAngle
            "sin(2 * x)", "cos(3 * x)", "sin(-4 * y)",
            // SetOperator
            "{ 1, 2 } intersect { 1, 2 }", "{ 1, 2 } unionwith { 1, 2 }",
            "{ 1, 2 } setsubtract { 1, 2 }", "{ 1, 2 } intersect ({ 3 } unionwith { 4 })",
            "x in { 1 }", "x in [0; 1]", "[-oo; +oo]",
            // PhiFunction
            "phi(2 ^ x)", "phi(3 ^ y)",
            // NormalTrigonometricForm and CollapseTrigonometricFunctions
            "tan(x)", "cotan(x)", "sec(x)", "csc(x)", "1 / sin(x)", "cos(x) / sin(x)",
            // Factorization
            "x ^ 2 - y ^ 2", "x ^ 4 - y ^ 2", "x * y + x * 2",
            // Power
            "log(x, y) + log(x, 2)", "(x ^ 2) ^ 3", "sqrt(x) * sqrt(x)", "2 ^ x * 2 ^ y",
        };

        /// <summary>
        /// Every expression a small grammar makes, and every node of each. A written list would
        /// only prove the cases somebody pictured, and what a rule set does at a node nobody
        /// pictured is the whole subject.
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
                foreach (var left in level2.Where((_, i) => i % 29 == 0))
                    foreach (var right in level2.Where((_, i) => i % 31 == 0))
                        level3.Add(string.Format(shape, left, right));

            var parsed = new List<Entity>();
            foreach (var source in level1.Concat(level2).Concat(level3).Concat(Seeds))
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the grammar makes some strings the parser declines; not its subject */ }
            }
            return parsed;
        }

        private static readonly Lazy<List<Entity>> nodes = new(() =>
        {
            var seen = new HashSet<Entity>();
            var all = new List<Entity>();
            foreach (var expr in Corpus())
                foreach (var node in expr.Nodes)
                    if (seen.Add(node))
                        all.Add(node);
            return all;
        });

        private static List<Entity> Nodes() => nodes.Value;

        [Theory]
        [MemberData(nameof(AddressableSets))]
        public void EveryArmIsTheArmItWasGeneratedFrom(string setName)
        {
            var set = SetFor(setName);
            var bySwitch = SwitchFor(setName);
            var corpus = Nodes();
            Assert.True(corpus.Count > 2000, $"the corpus is only {corpus.Count} nodes");

            var disagreements = new List<string>();
            var fired = 0;
            foreach (var node in corpus)
            {
                Entity expected;
                try { expected = bySwitch(node); }
                catch { continue; /* an arm that throws is not a claim about the registry */ }

                RewriteRule? rule;
                try { rule = set.RuleFiringAt(node); }
                catch (Exception thrown)
                {
                    disagreements.Add($"{node.Stringize()}: no rule could be asked ({thrown.GetType().Name})");
                    continue;
                }

                if (expected.Equals(node))
                {
                    // The switch fell through to its discard arm, so no arm matched — and no rule
                    // may claim to have.
                    if (rule is not null && !rule.TryApply(node).Equals(node))
                        disagreements.Add($"{node.Stringize()}: the switch did nothing, "
                            + $"but '{rule.Name}' claims {rule.TryApply(node).Stringize()}");
                    continue;
                }

                fired++;
                if (rule is null)
                {
                    disagreements.Add($"{node.Stringize()}: the switch gave {expected.Stringize()}, "
                        + "no rule claims it");
                    continue;
                }
                var actual = rule.TryApply(node);
                if (!expected.Equals(actual))
                    disagreements.Add($"{node.Stringize()}: the switch gave {expected.Stringize()}, "
                        + $"'{rule.Name}' gave {actual?.Stringize() ?? "null"}");
            }

            Assert.True(disagreements.Count == 0,
                $"{setName}: {disagreements.Count} of {corpus.Count} nodes disagree\n"
                + string.Join("\n", disagreements.Take(20)));
            Assert.True(fired > 0, $"{setName} never fired on the corpus, so nothing was compared");
        }

        /// <summary>
        /// A rule whose pattern and guard repeat an earlier rule's can never fire: first match
        /// wins, so the earlier one always takes it.
        /// </summary>
        /// <remarks>
        /// The generator names a rule after its pattern, so a repeat is exactly a name it has had
        /// to suffix. That makes an unreachable arm a thing the registry states rather than a
        /// thing somebody has to notice while reading a hundred of them.
        /// </remarks>
        [Fact]
        public void NoArmIsShadowedByAnIdenticalOneAboveIt()
        {
            var shadowed = RewriteRules.All
                .SelectMany(set => set.Rules.Select(rule => (set, rule)))
                .Where(pair => pair.rule.Name.Contains(" #"))
                .Select(pair => $"{pair.set.Name}: line {pair.rule.SourceLine}, {pair.rule.Name}")
                .ToList();
            Assert.True(shadowed.Count == 0,
                $"{shadowed.Count} unreachable arms:\n" + string.Join("\n", shadowed));
        }

        /// <summary>
        /// How much of the registry is addressable at rule grain, as a number that moves rather
        /// than a claim that it is "mostly" done.
        /// </summary>
        /// <remarks>
        /// The sets that are not are the ones whose rewrites are not a <c>switch</c> over the
        /// expression — a sort, a polynomial division, a method with branches and locals. Each is
        /// named here so that making one addressable is a change to this list rather than a
        /// silent improvement nobody records.
        /// </remarks>
        [Fact]
        public void TheRegistryIsAddressableAsFarAsItSaysItIs()
        {
            var withRules = RewriteRules.All.Where(set => set.Rules.Count > 0).ToList();
            var without = RewriteRules.All.Where(set => set.Rules.Count == 0)
                .Select(set => set.Name).OrderBy(name => name, StringComparer.Ordinal).ToList();

            Assert.Equal(new[]
            {
                "CanonicalOrder",
                "CanonicalOrderCountingConstants",
                "CanonicalOrderExact",
                "CollapseMultipleFractions",
                "CommonDenominator",
                "CommonDenominatorCountingConstants",
                "CommonDenominatorExact",
                "ExpandFactorialDivisions",
                "FactorizeFactorialMultiplications",
                "InvertNegativePowers",
                "PerfectSquare",
                "PolynomialGcdCancellation",
                "PolynomialLongDivision",
                "RationalizeDenominator",
            }, without);

            Assert.Equal(16, withRules.Count);
            Assert.Equal(347, withRules.Sum(set => set.Rules.Count));
        }

        [Fact]
        public void EveryRuleSaysWhereItIsWrittenAndWhatItMatches()
        {
            foreach (var set in RewriteRules.All)
                foreach (var rule in set.Rules)
                {
                    Assert.False(string.IsNullOrWhiteSpace(rule.Name), $"{set.Name} has a nameless rule");
                    Assert.False(string.IsNullOrWhiteSpace(rule.PatternSource), $"{set.Name}/{rule.Name} has no pattern");
                    Assert.False(string.IsNullOrWhiteSpace(rule.ReplacementSource), $"{set.Name}/{rule.Name} builds nothing");
                    Assert.True(rule.SourceLine > 0, $"{set.Name}/{rule.Name} has no line");
                    Assert.All(rule.NodeTypes, type => Assert.True(typeof(Entity).IsAssignableFrom(type),
                        $"{set.Name}/{rule.Name} is filed under {type}, which is not a node"));
                }
        }

        /// <summary>
        /// A rule's node type has to be the type it actually fires on, since the point of
        /// recording it is that a scheduler may use it to skip the rule.
        /// </summary>
        [Fact]
        public void ARuleOnlyFiresOnTheNodeTypeItIsFiledUnder()
        {
            var wrong = new List<string>();
            foreach (var set in RewriteRules.All)
                foreach (var rule in set.Rules)
                    foreach (var node in Nodes().Where((_, i) => i % 3 == 0))
                    {
                        if (rule.NodeTypes.Count == 0
                            || rule.NodeTypes.Any(type => type.IsInstanceOfType(node))) continue;
                        Entity? applied;
                        try { applied = rule.TryApply(node); }
                        catch { continue; }
                        if (applied is not null)
                            wrong.Add($"{set.Name}/{rule.Name} is filed under {string.Join("/", rule.NodeTypes.Select(type => type.Name))} "
                                + $"but fired on {node.Stringize()}");
                    }
            Assert.True(wrong.Count == 0, string.Join("\n", wrong.Take(10)));
        }

        /// <summary>
        /// The growth field has to agree with what the rule sets are for: expansion opens
        /// expressions out and factorisation gathers them up.
        /// </summary>
        [Fact]
        public void GrowthSaysWhichWayTheSetMoves()
        {
            Assert.All(RewriteRules.Expansion.Rules,
                rule => Assert.Equal(RewriteRuleGrowth.Expands, rule.Growth));
            Assert.Contains(RewriteRules.Factorization.Rules, rule => rule.Growth == RewriteRuleGrowth.Collects);
            Assert.Contains(RewriteRules.ExpandMultipleAngle.Rules, rule => rule.Growth == RewriteRuleGrowth.Expands);
        }

        [Fact]
        public void ARecordedStepNamesTheRuleAndNotOnlyTheSet()
        {
            using var recording = RewriteRecording.Start();
            RewriteRules.Common.ApplyOnce("a / (b / c)".ToEntity());
            recording.Dispose();

            var step = Assert.Single(recording.Steps);
            Assert.Equal(RewriteRules.Common, step.RuleSet);
            Assert.NotNull(step.Rule);
            Assert.Equal("a / (b / c) = a * c / b", step.Rule.Description);
            Assert.Equal("Divf(var any1, Divf(var any2, var any3))", step.Rule.PatternSource);
            Assert.Equal(step.After, step.Rule.TryApply(step.Before));
        }

        [Fact]
        public void ASetWithNoAddressableRulesStillRecordsItsStep()
        {
            using var recording = RewriteRecording.Start();
            RewriteRules.CanonicalOrderExact.ApplyOnce("y + x".ToEntity());
            recording.Dispose();

            var step = Assert.Single(recording.Steps);
            Assert.Equal(RewriteRules.CanonicalOrderExact, step.RuleSet);
            Assert.Null(step.Rule);
        }
    }
}
