//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A rule set written as **data** has to do exactly what the <c>switch</c> that already
    /// expresses it does. https://github.com/asc-community/AngouriMath/issues/746 v1.0 asks for
    /// pattern matching as a data structure; this is what makes replacing a `switch` with one a
    /// mechanical step rather than a leap of faith.
    /// </summary>
    /// <remarks>
    /// The comparison is differential and generative: both forms are run over every expression
    /// a small grammar produces, and they must agree on all of them. A hand-written list of
    /// cases would only prove the cases someone thought of, and the interesting disagreements
    /// in a matcher are the shapes nobody pictured — a literal that is a `Rational` rather than
    /// an `Integer`, a node whose child count differs from the pattern's, a name bound twice.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class MatchedRulesAgreeWithTheSwitchTest
    {
        private static readonly string[] Leaves = { "x", "y", "2", "-1", "1/2", "1", "0" };

        private static readonly string[] Unary =
        {
            "-({0})", "1 / ({0})", "({0}) ^ 2", "sqrt({0})", "sin({0})", "abs({0})",
        };

        private static readonly string[] Binary =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
        };

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
                catch { /* the generator makes some strings the parser declines; not its subject */ }
            }
            return parsed;
        }

        private static void AssertAgrees(
            string what, System.Func<Entity, Entity> bySwitch, MatchedRuleSet byData, int leastFirings)
        {
            var corpus = Corpus();
            Assert.True(corpus.Count > 500, $"the corpus is only {corpus.Count} expressions");

            var disagreements = new List<string>();
            var fired = 0;
            foreach (var expr in corpus)
            {
                var expected = bySwitch(expr);
                var actual = byData.ApplyHere(expr);
                if (!expected.Equals(expr)) fired++;
                if (!expected.Equals(actual))
                    disagreements.Add($"{expr.Stringize()}: switch gave {expected.Stringize()}, "
                        + $"data gave {actual.Stringize()}");
            }

            // The set has to actually fire, or agreement is the agreement of two things that
            // both did nothing.
            Assert.True(fired >= leastFirings,
                $"{what}: the rules only fired on {fired} of {corpus.Count} expressions");
            Assert.True(disagreements.Count == 0,
                $"{what}: {disagreements.Count} of {corpus.Count} disagreed:\n"
                + string.Join("\n", disagreements.Take(10)));
        }

        [Fact]
        public void DivisionPreparingAsDataMatchesTheSwitch()
            => AssertAgrees("DivisionPreparing", Patterns.DivisionPreparingRules,
                MatchedRules.DivisionPreparing, leastFirings: 20);

        /// <summary>
        /// The second set, and the one that says whether the shape generalises. It is harder in
        /// three ways: eight rules rather than three, an order that is load-bearing — the
        /// quotient-times-quotient rule has to be tried before the general product rule or the
        /// general one swallows it — and a predicate on a hole,
        /// <c>Integer { IsPositive: true }</c>.
        /// </summary>
        [Fact]
        public void CollapseMultipleFractionsAsDataMatchesTheSwitch()
            => AssertAgrees("CollapseMultipleFractions", Patterns.CollapseMultipleFractions,
                MatchedRules.CollapseMultipleFractions, leastFirings: 50);

        /// <summary>
        /// A predicate on a hole refuses what fails it, which is the C# property pattern
        /// <c>Integer { IsPositive: true }</c> as data.
        /// </summary>
        [Theory]
        [InlineData("(x / y) ^ 2", true)]
        [InlineData("(x / y) ^ (-2)", false)]
        [InlineData("(x / y) ^ 0", false)]
        [InlineData("(x / y) ^ z", false)]
        public void APredicateOnAHoleIsChecked(string expression, bool shouldFire)
        {
            var expr = expression.ToEntity();
            var rewritten = MatchedRules.CollapseMultipleFractions.ApplyHere(expr);
            Assert.Equal(shouldFire, !rewritten.Equals(expr));
        }

        /// <summary>
        /// Order is part of the data. Reversing the two rules that overlap makes the general
        /// one swallow the special one, which is what an ordered list is for and what a
        /// <c>switch</c> gets by accident of being written top to bottom.
        /// </summary>
        [Fact]
        public void TheOrderOfTheRulesIsLoadBearing()
        {
            var expr = "(a / b) * (c / d)".ToEntity();
            var asWritten = MatchedRules.CollapseMultipleFractions.FirstMatching(expr);
            Assert.Equal("product-of-two-quotients", asWritten!.Name);

            var reversed = new MatchedRuleSet("reversed",
                MatchedRules.CollapseMultipleFractions.Rules.Reverse().ToArray());
            Assert.NotEqual("product-of-two-quotients", reversed.FirstMatching(expr)!.Name);
        }

        /// <summary>
        /// A rule-level guard over <b>two</b> bindings at once, which no predicate on a single
        /// hole can express: <c>(a^b)^c = a^(b*c)</c> holds for a positive base whatever the
        /// exponents, and for any base when the outer exponent is whole.
        /// </summary>
        /// <remarks>
        /// Compared against the <c>switch</c> only where the comparison is meaningful.
        /// <c>PowerRules</c> is a large set and an earlier arm may fire on the same expression,
        /// so a case counts only where the switch either did nothing or produced exactly the
        /// power-of-a-power answer; anything else means a different arm matched and says
        /// nothing about this rule. <b>That the rule cannot be isolated from its `switch` any
        /// other way is itself the argument for rules being data.</b>
        /// </remarks>
        [Fact]
        public void AGuardOverTwoBindingsMatchesTheSwitch()
        {
            var disagreements = new List<string>();
            var compared = 0;
            var fired = 0;
            foreach (var expr in Corpus())
            {
                if (expr is not Entity.Powf(Entity.Powf(var a, var b), var c)) continue;
                var expected = Patterns.PowerRules(expr);
                var mine = MatchedRules.PowerOfPower.ApplyHere(expr);
                var theAnswer = (Entity)new Entity.Powf(a, b * c);

                if (!expected.Equals(expr) && !expected.Equals(theAnswer)) continue;
                compared++;
                if (!expected.Equals(expr)) fired++;
                if (!expected.Equals(mine))
                    disagreements.Add($"{expr.Stringize()}: switch gave {expected.Stringize()}, "
                        + $"data gave {mine.Stringize()}");
            }
            Assert.True(compared > 20, $"only {compared} comparable cases");
            Assert.True(fired > 0, "the switch never applied this rule, so agreement proves nothing");
            Assert.True(disagreements.Count == 0,
                $"{disagreements.Count} of {compared} disagreed:\n" + string.Join("\n", disagreements.Take(10)));
        }

        /// <summary>
        /// The guard is the whole point: #752 is what happens when this rule is applied without
        /// one. <c>sqrt(x^2)</c> must not become <c>x</c>, since at -0.63 that is -0.63 where
        /// the expression is 0.63.
        /// </summary>
        [Theory]
        [InlineData("(x ^ 2) ^ 3", true)]      // whole outer exponent, any base
        [InlineData("(x ^ 2) ^ (-1)", true)]   // still whole
        [InlineData("(2 ^ x) ^ (1/2)", true)]  // base is a positive real
        [InlineData("(x ^ 2) ^ (1/2)", false)] // neither, and this one is #752
        [InlineData("(x ^ 2) ^ (3/2)", false)]
        [InlineData("(x ^ y) ^ z", false)]
        public void TheGuardDecidesWhetherItFires(string expression, bool shouldFire)
        {
            var expr = expression.ToEntity();
            Assert.Equal(shouldFire, !MatchedRules.PowerOfPower.ApplyHere(expr).Equals(expr));
        }

        /// <summary>
        /// And where it does fire the value survives, checked at a negative point — which is
        /// where the unguarded version went wrong.
        /// </summary>
        [Theory]
        [InlineData("(x ^ 2) ^ 3", -0.63)]
        [InlineData("(x ^ 2) ^ (-1)", -1.7)]
        [InlineData("(x ^ 3) ^ 2", -2.4)]
        public void WhereItFiresTheValueSurvives(string expression, double at)
        {
            var expr = expression.ToEntity();
            var rewritten = MatchedRules.PowerOfPower.ApplyHere(expr);
            Assert.NotEqual(expr, rewritten);
            var before = expr.Substitute("x", at).EvalNumerical().RealPart.EDecimal.ToDouble();
            var after = rewritten.Substitute("x", at).EvalNumerical().RealPart.EDecimal.ToDouble();
            Assert.Equal(before, after, 8);
        }

        /// <summary>
        /// A name used twice binds the same subexpression both times — which is the
        /// <c>when any1 == any1a</c> guard the existing rules write out by hand, made
        /// structural.
        /// </summary>
        [Fact]
        public void ARepeatedNameMustMatchTheSameSubexpression()
        {
            var pattern = MatchPattern.Node<Entity.Sumf>(MatchPattern.Any("a"), MatchPattern.Any("a"));
            Assert.NotNull(new MatchedRule("doubles", pattern,
                bound => 2 * bound["a"], Soundness.Sound).TryApply("x + x".ToEntity()));
            Assert.Null(new MatchedRule("doubles", pattern,
                bound => 2 * bound["a"], Soundness.Sound).TryApply("x + y".ToEntity()));
        }

        /// <summary>A typed hole refuses what is not of its type.</summary>
        [Fact]
        public void ATypedHoleIsTyped()
        {
            var rule = new MatchedRule("numeric-left",
                MatchPattern.Node<Entity.Mulf>(
                    MatchPattern.Any<Entity.Number>("c"), MatchPattern.Any("a")),
                bound => bound["c"] + bound["a"], Soundness.Sound);
            Assert.NotNull(rule.TryApply("2 * x".ToEntity()));
            Assert.Null(rule.TryApply("y * x".ToEntity()));
        }

        /// <summary>
        /// The set is <b>enumerable</b> and each rule is addressable by name — the property the
        /// `switch` cannot have and the reason three separate tier-2 items are blocked on this.
        /// </summary>
        [Fact]
        public void TheRulesAreEnumerableAndNamed()
        {
            var rules = MatchedRules.DivisionPreparing.Rules;
            Assert.Equal(3, rules.Count);
            Assert.All(rules, rule => Assert.False(string.IsNullOrWhiteSpace(rule.Name)));
            Assert.Equal(rules.Count, rules.Select(rule => rule.Name).Distinct().Count());
            Assert.NotNull(MatchedRules.DivisionPreparing.FirstMatching("2 / x * y".ToEntity()));
        }

        /// <summary>
        /// A set's tier is <b>derived</b> from its rules rather than declared beside them, so it
        /// cannot drift from what it is about. That is the fix for the registry's thirty sets
        /// all declaring the same value.
        /// </summary>
        [Fact]
        public void TheSetsTierIsTheWeakestOfItsRules()
        {
            Assert.Equal(Soundness.SoundUnderAssumptions, MatchedRules.DivisionPreparing.Soundness);

            var mixed = new MatchedRuleSet("mixed",
                new MatchedRule("sound", MatchPattern.Any("a"), b => b["a"], Soundness.Sound),
                new MatchedRule("heuristic", MatchPattern.Any("a"), b => b["a"], Soundness.Heuristic));
            Assert.Equal(Soundness.Heuristic, mixed.Soundness);

            var allSound = new MatchedRuleSet("sound",
                new MatchedRule("one", MatchPattern.Any("a"), b => b["a"], Soundness.Sound));
            Assert.Equal(Soundness.Sound, allSound.Soundness);
        }
    }
}
