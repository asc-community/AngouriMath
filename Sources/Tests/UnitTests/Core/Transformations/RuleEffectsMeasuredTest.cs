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
using AngouriMath.Core;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// Three things about a rewrite that can be measured rather than declared: whether the rule
    /// ever fires, whether it changes where its result has a value, and which way it moves the
    /// cost the simplifier actually selects by.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Measured and not declared, deliberately.</b> Each of these could be a field on
    /// <see cref="MatchedRule"/> alongside <see cref="RewriteRuleGrowth"/>, and each would then be
    /// one more thing to write down wrongly. Measuring costs nothing at run time — none of this
    /// runs in production — and it cannot go stale against the rules it describes.
    /// </para>
    /// <para>
    /// The definedness check is contract obligation <b>O4</b>, which is otherwise only as true as
    /// whoever wrote the rule remembering to attach a <c>Provided</c>. The cost check is the one
    /// that says something growth cannot: growth counts nodes, and the candidate selection uses
    /// <c>MathS.Settings.ComplexityCriteria</c>, so a rule can shrink a tree and raise the number
    /// that decides whether its answer is taken.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleEffectsMeasuredTest
    {
        private static readonly string[] Leaves =
            { "x", "y", "2", "-1", "-2", "1/2", "-1/2", "1", "0" };

        private static readonly string[] Unary =
            { "-({0})", "1 / ({0})", "({0}) ^ 2", "({0}) ^ (-2)", "sqrt({0})", "sin({0})", "abs({0})" };

        private static readonly string[] Binary =
            { "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})" };

        /// <summary>
        /// The same generated arithmetic as <c>RuleGrowthAgreesWithTheCorpusTest</c>, and the same
        /// supplied shapes, so that the rules those reach are the rules these measure.
        /// </summary>
        private static readonly string[] Reached =
        {
            "sin(x)", "cos(x)", "tan(x)", "cotan(x)", "sec(x)", "cosec(x)",
            "sin(2 * x)", "cos(2 * x)", "sin(x + y)", "sin(x) * cos(x)", "sin(x) / cos(x)",
            "sin(x) ^ 2 + cos(x) ^ 2", "1 / sin(x)", "tan(x) * cotan(x)", "cosec(x) * sin(x)",
            "arcsin(x) + arccos(x)", "sin(arcsin(x))", "arcsin(sin(x))", "sin(-x)", "cos(-x)",
            "ln(x)", "log(2, x)", "ln(x * y)", "e ^ ln(x)", "log(2, 1/x)", "log(1/2, x)",
            "x!", "(x + 1)!", "(x + 1)! / x!", "x! * (x + 1)",
            "a and b", "a or b", "not a", "not (not a)", "not (a and b)", "a implies b",
            "a and not a", "a or not a", "a and (b or c)", "not a and not b",
            "x > y", "x = y", "0 > x", "x >= x", "x / (-2) > 0", "(-2) * x = 0",
            "x in [1; 2]", "[1; 2] \\/ [3; 4]", "A \\/ A", "A \\ A",
            "sqrt(x) * sqrt(x)", "x ^ 2 * x ^ 3", "x ^ 3 / x ^ 2", "(x ^ 2) ^ 3", "x ^ 2 * y ^ 2",
            "x ^ 2 - 1", "(x + y) ^ 2", "(x + y) * (x - y)", "1 / (1 + sqrt(2))",
            "x * (y / z)", "x / y / z", "x / (y / z)", "2 * x + 3 * x", "x + x", "x - x", "x * x",
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
            foreach (var source in level1.Concat(level2).Concat(level3).Concat(Reached))
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the generator makes some strings the parser declines */ }
            }
            return parsed;
        }

        /// <summary>Every firing of every rule, tried at every node as a rewrite pass would.</summary>
        private static Dictionary<MatchedRule, List<(Entity Before, Entity After)>> Firings()
        {
            var corpus = Corpus().SelectMany(expr => expr.Nodes).Distinct().ToList();
            var firings = new Dictionary<MatchedRule, List<(Entity, Entity)>>();
            foreach (var set in MatchedRules.All)
                foreach (var rule in set.Rules)
                {
                    var seen = new List<(Entity, Entity)>();
                    foreach (var expr in corpus)
                    {
                        Entity? after;
                        try { after = rule.TryApply(expr); }
                        catch { continue; }
                        if (after is not null && after != expr)
                            seen.Add((expr, after));
                    }
                    if (seen.Count > 0) firings[rule] = seen;
                }
            return firings;
        }

        /// <summary>
        /// <b>Zero is the point of the exercise.</b> Arithmetic definedness changes where a
        /// denominator vanishes, a logarithm reaches its pole or a root turns negative, and every
        /// one of those is at or near zero. A set of four comfortable reals would have run the
        /// check without ever asking the question it exists to ask.
        /// </summary>
        private static readonly Entity[] CheckPoints = { 0, 0.37, 1.91, -2.63, 5.2 };

        /// <summary>
        /// Whether substituting a real into this is a question with an answer. A boolean or a set
        /// takes no real, so comparing what it evaluates to before and after says nothing about
        /// definedness.
        /// </summary>
        private static bool IsArithmetic(Entity expr)
            => !expr.Nodes.Any(node => node is Entity.Statement or Entity.Set);

        /// <summary>
        /// Substitutes a real for every free variable and answers whether the result has a value.
        /// A boolean or set-valued expression does not evaluate to a number and does not throw —
        /// it comes back as itself — which is the trap the saturation check fell into (#1162).
        /// </summary>
        private static bool? HasValueAt(Entity expr, Entity point)
        {
            var vars = expr.Vars.ToList();
            var substituted = vars.Aggregate(expr, (e, v) => e.Substitute(v, point));
            // Finite, not merely a Number. `0 / 0` evaluates to NaN and NaN *is* a Number, so a
            // check for `is Number` calls the undefined case defined and passes over exactly what
            // it exists to look at -- removing the `Provided` from `a / a = 1` did not trip it.
            // An infinity at a pole is the same: a value the expression does not have.
            try { return substituted.Evaled is Entity.Number { IsFinite: true }; }
            catch { return null; }
        }

        /// <summary>
        /// <b>Contract obligation O4, asked of the rules that claim it.</b> A rewrite must not
        /// quietly change where its result has a value: turning an undefined expression into a
        /// defined one invents an answer, and the other way round loses one.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Asked only of <see cref="Soundness.Sound"/> rules, because that is what the tier means
        /// — "holds for every value the pattern admits, with nothing assumed". A rule declared
        /// <c>SoundUnderAssumptions</c> is already saying it holds given something it does not
        /// check, and changing definedness at a degenerate input is one such thing:
        /// <c>a / (b / c) = a * c / b</c> turns <c>NaN</c> into <c>0</c> when <c>c</c> is zero,
        /// and its tier says so. Flagging those would be re-reporting a declaration.
        /// </para>
        /// <para>
        /// Or the rule attaches a <c>Provided</c>, which is the other way of saying it.
        /// </para>
        /// </remarks>
        [Fact]
        public void NoSoundRuleChangesWhereItsResultHasAValue()
        {
            var changed = new List<string>();

            foreach (var (rule, firings) in Firings())
            {
                if (rule.Soundness is not Soundness.Sound) continue;
                foreach (var (before, after) in firings)
                {
                    // A rule whose replacement attaches a condition is declaring the change.
                    if (after.Nodes.Any(n => n is Entity.Providedf)) continue;

                    // Arithmetic only. Substituting a real into a boolean or a set is a category
                    // error rather than a definedness change: `not not a` stays unevaluated at
                    // 0.37 while the `a` it rewrites to becomes a number, and reading that as the
                    // rule inventing an answer is the same mistake #1162 was.
                    if (IsArithmetic(before) is false || IsArithmetic(after) is false) continue;

                    foreach (var point in CheckPoints)
                    {
                        var had = HasValueAt(before, point);
                        var has = HasValueAt(after, point);
                        if (had is null || has is null) continue;
                        if (had == has) continue;
                        changed.Add($"{rule.Name}: {before.Stringize()} -> {after.Stringize()} "
                                    + $"at {point.Stringize()}: had a value {had}, has one {has}");
                        break;
                    }
                }
            }

            // Named rather than counted. `k - k = 0` is declared Sound and assumes k has a value:
            // at x = 0, `x^(-2) - x^(-2)` is undefined and `0` is not. Its sibling for the same
            // shape, `a / a = 1`, attaches `provided a is not zero` and this one attaches nothing.
            // Fixing it should delete the entry rather than leave a name that means nothing.
            // https://github.com/asc-community/AngouriMath/issues/1169
            var known = new[] { "a-term-subtracted-from-itself-vanishes" };

            var offending = changed.Select(c => c.Split(':')[0]).Distinct().ToList();
            var unexpected = offending.Except(known).ToList();

            Assert.True(unexpected.Count == 0,
                $"{unexpected.Count} rules declared Sound newly change where the result has a "
                + "value without attaching a condition:\n"
                + string.Join("\n", changed.Where(c => unexpected.Contains(c.Split(':')[0])).Take(20)));

            Assert.Equal(known, offending.Intersect(known).ToArray());
        }

        /// <summary>
        /// <b>Growth is not cost.</b> `growth` counts nodes; the simplifier chooses between
        /// candidates with <c>MathS.Settings.ComplexityCriteria</c>. A rule declared
        /// <c>Collects</c> that raises that number shrinks the tree and makes its own answer less
        /// likely to be taken, which is worth knowing about and is invisible from the node count.
        /// </summary>
        /// <remarks>
        /// Reported as a named list rather than asserted to be empty: the two measures are allowed
        /// to disagree, and where they do it is a fact about the rule rather than a defect. What
        /// the assertion protects is that they mostly agree, so that `growth` remains a usable
        /// proxy for what the selector does.
        /// </remarks>
        [Fact]
        public void ADeclaredCollectsMostlyLowersTheCostTheSelectorUses()
        {
            var cost = MathS.Settings.ComplexityCriteria.Value;
            int agreeing = 0, disagreeing = 0;
            var examples = new List<string>();

            foreach (var (rule, firings) in Firings())
            {
                if (rule.Growth is not RewriteRuleGrowth.Collects) continue;
                foreach (var (before, after) in firings)
                {
                    double costBefore, costAfter;
                    try { costBefore = cost(before); costAfter = cost(after); }
                    catch { continue; }
                    if (costAfter < costBefore) agreeing++;
                    else
                    {
                        disagreeing++;
                        if (examples.Count < 10)
                            examples.Add($"{rule.Name}: {before.Stringize()} -> {after.Stringize()} "
                                         + $"costs {costBefore:g4} -> {costAfter:g4}");
                    }
                }
            }

            Assert.True(agreeing > 0, "no rule declared Collects fired at all");
            Assert.True(agreeing > disagreeing,
                $"{disagreeing} of {agreeing + disagreeing} firings of rules declared Collects "
                + $"raise the cost the selector uses, which is more than lower it -- `growth` has "
                + $"stopped being a proxy for what selection does:\n" + string.Join("\n", examples));
        }

        /// <summary>
        /// How much of the registry this corpus reaches, as a count that moves. A rule the corpus
        /// never fires is a rule none of the checks above says anything about, so this is the
        /// reach of everything measured here rather than a claim about the rules themselves.
        /// </summary>
        /// <remarks>
        /// 123 of the 324 today. Fewer than the 194 that
        /// <c>RuleGrowthAgreesWithTheCorpusTest</c> reaches, because that one carries the shapes
        /// for the comparison and chaining families as well; the arithmetic and the transcendental
        /// shapes here are what the definedness and cost questions are about.
        /// </remarks>
        [Fact]
        public void TheCorpusReachesTheRulesTheseMeasurementsDescribe()
        {
            var fired = Firings().Count;
            var all = MatchedRules.All.Sum(s => s.Rules.Count);

            Assert.True(fired >= 120,
                $"only {fired} of {all} rules fire on this corpus, down from 123 -- the "
                + "measurements here describe a shrinking part of the registry");
        }
    }
}
