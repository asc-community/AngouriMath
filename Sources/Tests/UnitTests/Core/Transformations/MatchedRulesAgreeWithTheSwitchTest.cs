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

        [Fact]
        public void DivisionPreparingAsDataMatchesTheSwitch()
        {
            var corpus = Corpus();
            Assert.True(corpus.Count > 500, $"the corpus is only {corpus.Count} expressions");

            var disagreements = new List<string>();
            var fired = 0;
            foreach (var expr in corpus)
            {
                var bySwitch = Patterns.DivisionPreparingRules(expr);
                var byData = MatchedRules.DivisionPreparing.ApplyHere(expr);
                if (!bySwitch.Equals(expr)) fired++;
                if (!bySwitch.Equals(byData))
                    disagreements.Add($"{expr.Stringize()}: switch gave {bySwitch.Stringize()}, "
                        + $"data gave {byData.Stringize()}");
            }

            // The set has to actually fire, or agreement is the agreement of two things that
            // both did nothing.
            Assert.True(fired > 20, $"the rules only fired on {fired} of {corpus.Count} expressions");
            Assert.True(disagreements.Count == 0,
                $"{disagreements.Count} of {corpus.Count} disagreed:\n"
                + string.Join("\n", disagreements.Take(10)));
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
