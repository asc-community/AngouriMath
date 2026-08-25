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
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A rule read backwards: what an expression could have <b>come from</b>, rather than what it
    /// becomes. https://github.com/asc-community/AngouriMath/issues/746 tier 2 names this as its
    /// first missing piece, and nothing in the library answers it today — the 405 addressable
    /// rules in <see cref="RewriteRules"/> carry their replacement as C# source text, which is
    /// something to read and not something to match against.
    /// </summary>
    /// <remarks>
    /// Two claims are under test and they are different. That a rule <i>can</i> be reversed is
    /// structural, decided from the two sides, and asserted rule by rule below. That the reversal
    /// is <i>right</i> is mathematics, and is checked by value rather than by shape: the forward
    /// rewrite and the backward one have to name the same number at the same point.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class ReversibleRuleTest
    {
        /// <summary>
        /// Every set in <see cref="MatchedRules"/>, read off the type rather than listed here.
        /// </summary>
        /// <remarks>
        /// This was a hand-written list of five, and it stopped covering the file the moment a
        /// sixth set was added — so <c>EveryDataRuleIsBuildableOnBothSides</c>, whose whole
        /// purpose is to catch a template naming a node type the matcher can match but not
        /// build, was silently not looking at the new sets. It missed exactly that: a rule
        /// building a <c>Cosecantf</c>, which matched, built nothing, and was indistinguishable
        /// at run time from a rule that did not apply.
        /// </remarks>
        private static IEnumerable<MatchedRuleSet> DataRuleSets()
            => typeof(MatchedRules)
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static)
                .Where(property => property.PropertyType == typeof(MatchedRuleSet))
                .Select(property => (MatchedRuleSet)property.GetValue(null)!)
                .OrderBy(set => set.Name, StringComparer.Ordinal);

        private static readonly string[] Leaves = { "x", "y", "z", "2", "-1", "1/2", "1", "0" };

        private static readonly string[] Binary =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
        };

        private static List<Entity> Corpus()
        {
            var level1 = new List<string>(Leaves);
            var level2 = new List<string>();
            foreach (var shape in Binary)
                foreach (var left in level1)
                    foreach (var right in level1)
                        level2.Add(string.Format(shape, left, right));
            var level3 = new List<string>();
            foreach (var shape in Binary)
                foreach (var left in level2.Where((_, i) => i % 13 == 0))
                    foreach (var right in level2.Where((_, i) => i % 19 == 0))
                        level3.Add(string.Format(shape, left, right));

            var parsed = new List<Entity>();
            foreach (var source in level1.Concat(level2).Concat(level3))
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the generator makes some strings the parser declines; not its subject */ }
            }
            return parsed;
        }

        /// <summary>
        /// Which of the rules written as data have two directions, listed one by one rather than
        /// counted — a count agrees with itself after a rule changes shape and a list does not.
        /// </summary>
        [Fact]
        public void EveryDataRuleIsClassifiedAsWritten()
        {
            var actual = DataRuleSets()
                .SelectMany(set => set.Rules)
                .ToDictionary(rule => rule.Name, rule => rule.Reversal);

            var oneWay = actual.Where(pair => pair.Value is not RuleReversal.Reversible)
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .Select(pair => $"{pair.Key}: {pair.Value}")
                .ToArray();

            // Named one by one and with the reason, so that a rule losing or gaining a direction
            // fails rather than moving a number. Almost all of these are one-way because their
            // replacement is arithmetic on a binding rather than a tree built around it -- see
            // RuleReversal.ReplacementIsCode -- and exactly one is one-way for a mathematical
            // reason: 1 does not say which angle it came from.
            Assert.Equal(
                new[]
                {
                    "a-difference-of-two-negatives-turns-round: ReplacementIsCode",
                    "a-negative-added-is-subtracted: ReplacementIsCode",
                    "a-negative-factor-in-a-denominator-comes-out: ReplacementIsCode",
                    "a-negative-factor-in-a-left-product-comes-out: ReplacementIsCode",
                    "a-negative-factor-in-a-numerator-comes-out: ReplacementIsCode",
                    "a-negative-factor-in-a-right-product-comes-out: ReplacementIsCode",
                    "a-negative-factor-in-a-sum-becomes-a-difference: ReplacementIsCode",
                    "a-negative-integer-power-becomes-a-reciprocal: ReplacementIsCode",
                    "a-negative-minuend-comes-out-in-front: ReplacementIsCode",
                    "a-negative-subtracted-is-added: ReplacementIsCode",
                    "a-numeric-coefficient-is-gathered-over-a-surd: ReplacementIsCode",
                    "a-plain-factorial-times-the-next-term: ReplacementIsCode",
                    "a-product-of-two-negatives-is-positive: ReplacementIsCode",
                    "a-quotient-of-a-plain-factorial-by-a-shifted-one: ReplacementIsCode",
                    "a-quotient-of-a-shifted-factorial-by-a-plain-one: ReplacementIsCode",
                    "a-quotient-of-polynomials-is-divided-out: ReplacementIsCode",
                    "a-quotient-of-polynomials-is-put-in-lowest-terms: ReplacementIsCode",
                    "a-quotient-of-shifted-factorials: ReplacementIsCode",
                    "a-quotient-of-two-negatives-is-positive: ReplacementIsCode",
                    "a-shifted-factorial-times-a-bare-term: ReplacementIsCode",
                    "a-shifted-factorial-times-the-next-term: ReplacementIsCode",
                    "a-sum-of-two-negatives-is-a-negated-sum: ReplacementIsCode",
                    "a-sum-or-difference-that-is-a-perfect-square: ReplacementIsCode",
                    "a-two-term-denominator-is-multiplied-by-its-conjugate: ReplacementIsCode",
                    "cosine-of-a-whole-multiple-of-an-angle: ReplacementIsCode",
                    "eulers-totient-of-a-prime-power: ReplacementIsCode",
                    "sine-of-a-whole-multiple-of-an-angle: ReplacementIsCode",
                    "squared-sine-and-cosine-of-one-argument-sum-to-one: ReplacementDropsHoles",
                },
                oneWay);
        }

        /// <summary>
        /// <b>The question no existing API answers.</b> Every entry point in the library runs
        /// forwards: <c>Simplify</c>, <c>Expand</c> and <c>Factorize</c> all take an expression to
        /// another one. This asks the other way — given an expression, which expression does one
        /// rule of this set turn into it.
        /// </summary>
        /// <remarks>
        /// <c>k*p + k*q -&gt; k*(p + q)</c> read backwards is the distributive law, and reading it
        /// backwards is the only way the library has of knowing that the two are the same fact.
        /// The forward rule is written four times in <c>Patterns.CommonRules</c> and nine times in
        /// <c>Patterns.FactorizeRules</c>, none of which knows about the expansion that undoes it.
        /// </remarks>
        [Fact]
        public void AReversedRuleSaysWhatAnExpressionCameFrom()
        {
            var factoring = MatchedRules.SharedFactor.Rules.Single();
            Assert.Equal("x * (a + b)".ToEntity(), factoring.TryApply("x * a + x * b".ToEntity()));

            var expanding = factoring.Reversed;
            Assert.NotNull(expanding);
            Assert.Equal("x * a + x * b".ToEntity(), expanding!.TryApply("x * (a + b)".ToEntity()));

            // And the set, so that the question can be asked of a whole set of rules at once.
            Assert.Equal(
                "x * a + x * b".ToEntity(),
                MatchedRules.SharedFactor.Reversed.ApplyHere("x * (a + b)".ToEntity()));
        }

        /// <summary>
        /// A rule that throws information away has no backwards reading, and the type says so
        /// rather than a comment saying so.
        /// </summary>
        [Fact]
        public void ARuleThatForgetsAHoleHasNoBackwardsReading()
        {
            var pythagoras = MatchedRules.PythagoreanIdentity.Rules.Single();
            Assert.Equal(RuleReversal.ReplacementDropsHoles, pythagoras.Reversal);
            Assert.Null(pythagoras.Reversed);

            // And the reversed set is empty rather than absent, which is the difference being
            // reported rather than hidden.
            Assert.Empty(MatchedRules.PythagoreanIdentity.Reversed.Rules);
            Assert.Equal(
                "1 + a".ToEntity(),
                MatchedRules.PythagoreanIdentity.Reversed.ApplyHere("1 + a".ToEntity()));
        }

        /// <summary>
        /// The reversal of a reversal is the rule it came from — not merely classified the same
        /// way, but rewriting every expression in the corpus to the same thing.
        /// </summary>
        [Fact]
        public void ReversingTwiceGivesTheRuleBack()
        {
            var corpus = Corpus();
            Assert.True(corpus.Count > 500, $"the corpus is only {corpus.Count} expressions");

            foreach (var rule in DataRuleSets().SelectMany(set => set.Rules))
            {
                if (rule.Reversed is not { } once) continue;
                var twice = once.Reversed;
                Assert.NotNull(twice);
                foreach (var expr in corpus)
                    Assert.Equal(rule.TryApply(expr), twice!.TryApply(expr));
            }
        }

        /// <summary>
        /// A reversed rule undoes the rule it came from, <b>by value</b>. Not by shape: reversing
        /// a rule whose pattern is commutative writes the operands in the order the rule was
        /// written in, so <c>a*x + b*x</c> comes back as <c>x*a + x*b</c>, which is the same
        /// number written differently and would fail a comparison of trees.
        /// </summary>
        [Fact]
        public void AReversedRuleUndoesTheRuleItCameFrom()
        {
            var corpus = Corpus();
            var checkedPairs = 0;
            var rewritten = 0;
            var failures = new List<string>();

            foreach (var rule in DataRuleSets().SelectMany(set => set.Rules))
            {
                if (rule.Reversed is not { } backwards) continue;
                foreach (var expr in corpus)
                {
                    if (rule.TryApply(expr) is not { } forward) continue;
                    if (backwards.TryApply(forward) is not { } back) continue;
                    checkedPairs++;
                    // The tree came back unchanged, which is the strongest answer there is and
                    // needs no arithmetic -- and asking for arithmetic anyway would fail on the
                    // corpus's expressions that have no value, such as anything over a literal 0.
                    if (expr.Equals(back)) continue;
                    rewritten++;
                    if ((expr - back).Simplify() != Integer.Zero)
                        failures.Add($"{rule.Name}: {expr.Stringize()} -> {forward.Stringize()} "
                            + $"-> {back.Stringize()}");
                }
            }

            Assert.True(checkedPairs > 100, $"only {checkedPairs} round trips were available");
            // Without one of these the value check is a check of nothing: every round trip
            // returning the same tree would pass it without ever comparing two numbers.
            Assert.True(rewritten > 0, "no round trip came back written differently");
            Assert.True(failures.Count == 0,
                $"{failures.Count} of {rewritten} rewritten round trips changed the value:\n"
                + string.Join("\n", failures.Take(10)));
        }

        /// <summary>
        /// <b>A constraint on a hole is written once and holds in both directions.</b>
        /// <c>(a/b)^c = a^c/b^c</c> needs a positive whole <c>c</c>, and the forward rule is where
        /// that is written. Read backwards the rule matches <c>a^c/b^c</c> for any <c>c</c> at
        /// all — and then refuses to build the quotient-to-a-power unless <c>c</c> passes the
        /// constraint on the side that states it.
        /// </summary>
        [Theory]
        [InlineData("x ^ 2 / y ^ 2", "(x / y) ^ 2")]
        [InlineData("x ^ 3 / y ^ 3", "(x / y) ^ 3")]
        [InlineData("x ^ n / y ^ n", null)]          // n is not known to be a positive integer
        [InlineData("x ^ (-2) / y ^ (-2)", null)]    // negative, so the forward rule refuses it
        [InlineData("x ^ (1/2) / y ^ (1/2)", null)]  // and this one is the branch-cut case
        public void AHoleConstraintSurvivesBeingReadBackwards(string from, string? expected)
        {
            var rule = MatchedRules.CollapseMultipleFractions.Rules
                .Single(one => one.Name == "positive-power-of-a-quotient-distributes");
            var backwards = rule.Reversed;
            Assert.NotNull(backwards);

            var actual = backwards!.TryApply(from.ToEntity());
            if (expected is null)
                Assert.Null(actual);
            else
                Assert.Equal(expected.ToEntity(), actual);
        }

        /// <summary>
        /// A pattern over a node this cannot construct is matchable and not writable, so a rule
        /// matching one is one-way — reported as its own reason rather than folded into the
        /// others, because it is a gap in the mechanism where the rest are facts about the
        /// mathematics.
        /// </summary>
        /// <remarks>
        /// The unbuildable node is on the <b>left</b>, which is the case this is about: the rule
        /// fires, and only its reverse is impossible. An unbuildable node on the right is a
        /// different thing entirely and is rejected outright — see
        /// <see cref="AReplacementCannotNameANodeThisCannotBuild"/>.
        /// </remarks>
        [Fact]
        public void APatternOverAnUnbuildableNodeIsMatchableAndNotReversible()
        {
            var overMod = new MatchedRule(
                "modulus",
                MatchPattern.Node<Modf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                MatchPattern.Node<Mulf>(MatchPattern.Any("b"), MatchPattern.Any("a")),
                Soundness.Heuristic);

            // It still matches, which is what "matchable and not writable" means.
            Assert.True(overMod.Left.Matches("x mod y".ToEntity()));
            Assert.Equal("y * x", overMod.TryApply("x mod y".ToEntity())!.Stringize());
            Assert.Equal(RuleReversal.PatternCannotBeBuilt, overMod.Reversal);
            Assert.Null(overMod.Reversed);
        }

        /// <summary>
        /// A replacement naming a node type the matcher can match but not construct is not a
        /// one-way rule — it is a rule that <b>never fires</b>, since the match succeeds and the
        /// build then returns nothing, which <c>TryApply</c> reports as "did not apply".
        /// </summary>
        /// <remarks>
        /// Indistinguishable at run time from a correct rule on an expression it declines, so
        /// nothing downstream can catch it. It cost an afternoon and a twenty-eight-row agreement
        /// failure — a set of four rules building a <c>Cosecantf</c>, which was not in
        /// <c>MatchPattern.Construct</c> — before the constructor was made to say so.
        /// </remarks>
        [Fact]
        public void AReplacementCannotNameANodeThisCannotBuild()
        {
            var thrown = Assert.Throws<ArgumentException>(() => new MatchedRule(
                "unbuildable",
                MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                MatchPattern.Node<Modf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                Soundness.Heuristic));
            Assert.Contains("cannot build", thrown.Message);
        }

        /// <summary>
        /// A replacement naming a hole the pattern never binds is a typo, and a typo that would
        /// otherwise show as a rule that silently never fires. Only a right-hand side written as
        /// data can be checked for it at all — a builder over the bindings throws at run time on
        /// the first expression that reaches it, or not at all.
        /// </summary>
        [Fact]
        public void AReplacementCannotNameAHoleThePatternDoesNotBind()
        {
            var thrown = Assert.Throws<ArgumentException>(() => new MatchedRule(
                "typo",
                MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                MatchPattern.Node<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("c")),
                Soundness.Sound));
            Assert.Contains("'c'", thrown.Message);
        }

        /// <summary>
        /// Both sides of every rule written as data can be built, so the node types the rules use
        /// are all ones <c>MatchPattern.Construct</c> knows. A rule that adds a node type to a
        /// right-hand side and not to that method fails here rather than becoming quietly one-way.
        /// </summary>
        [Fact]
        public void EveryDataRuleIsBuildableOnBothSides()
        {
            foreach (var set in DataRuleSets())
                foreach (var rule in set.Rules)
                {
                    // A rule whose replacement is code has no right-hand pattern to check, and
                    // its left need not be buildable either: it is one-way whatever the nodes.
                    if (rule.Right is null)
                    {
                        Assert.Equal(RuleReversal.ReplacementIsCode, rule.Reversal);
                        continue;
                    }
                    Assert.True(rule.Right.IsBuildable, $"{set.Name}/{rule.Name}: right");
                    // The left decides only whether it reads backwards, and the type says which.
                    Assert.Equal(rule.Left.IsBuildable,
                        rule.Reversal is not RuleReversal.PatternCannotBeBuilt);
                }
        }

        /// <summary>
        /// Reversal carries the side condition and the soundness tier over unchanged. Both follow
        /// from what a rewrite rule claims: the condition is a predicate on the bindings and both
        /// directions produce the same bindings, and an equality is symmetric.
        /// </summary>
        [Fact]
        public void ReversalCarriesTheConditionAndTheTier()
        {
            var powerOfPower = MatchedRules.PowerOfPower.Rules.Single();
            var backwards = powerOfPower.Reversed;
            Assert.NotNull(backwards);
            Assert.Equal(powerOfPower.Soundness, backwards!.Soundness);

            // (a^b)^c = a^(b*c) needs a whole c or a positive real a. Read backwards the same
            // condition decides, so a^(b*c) is only rewritten to (a^b)^c where it is true.
            Assert.Equal("(x ^ y) ^ 2".ToEntity(), backwards.TryApply("x ^ (y * 2)".ToEntity()));
            Assert.Null(backwards.TryApply("x ^ (y * z)".ToEntity()));
        }
    }
}
