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
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A rule set applied over and over should stop. This runs each of them to a fixed point and
    /// fails on a term it has already produced.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>This is the property <see cref="RewriteRuleGrowth"/> is standing in for and cannot
    /// express.</b> A rule that rearranges changes no node count, so two of them can rewrite back
    /// and forth for ever while each one individually looks harmless. That is not hypothetical:
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a> was exactly
    /// such a pair, and what came out of it is a comment asking the next author to remember that
    /// collecting a <c>-1</c> is undone by
    /// <c>a-negated-reciprocal-rational-factor-is-a-negated-division</c>. A comment is where a
    /// check belongs.
    /// </para>
    /// <para>
    /// <b>A revisited term is the failure, not a long run.</b> Some sets are meant to grow —
    /// expansion, the multiple-angle rules — so reaching the step limit without repeating itself
    /// is reported and allowed. Only a term the set has already produced proves it will never
    /// stop, since the rules are deterministic and first-match-wins: the same term must go the
    /// same way again.
    /// </para>
    /// <para>
    /// Rewriting is at the root through <see cref="MatchedRuleSet.ApplyHere"/> rather than over
    /// the whole tree, which is what makes the loop honest — a full pass could move a rewrite to
    /// a different node each time and never repeat a term while still not terminating. The cycle
    /// #1056 found was two rules matching one node shape, which is what this reaches.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleSetsDoNotCycleTest
    {
        private static readonly string[] Seeds =
        {
            "x", "-x", "2 * x", "-2 * x", "x / 2", "x / -2", "-1 * x", "x * -1",
            "x + y", "x - y", "-x + y", "x + -y", "x * y", "x / y", "-x / -y", "x / -y",
            "1 / x", "-1 / x", "x / (1 / y)", "(1/2) * x", "x * (1/2)", "(-1/2) * x",
            "x ^ 2", "x ^ -2", "(x ^ 2) ^ 3", "x ^ 2 * x ^ 3", "sqrt(x)", "abs(x)", "abs(-x)",
            "sin(x)", "cos(x)", "tan(x)", "sin(-x)", "cos(-x)", "sin(x) / cos(x)",
            "sin(x) * cos(x)", "sin(x) ^ 2 + cos(x) ^ 2", "1 / sin(x)", "arcsin(sin(x))",
            "ln(x)", "log(2, x)", "ln(x * y)", "e ^ ln(x)",
            "a and b", "a or b", "not a", "not (not a)", "not (a and b)", "a implies b",
            "x > y", "0 > x", "x >= x", "not (x > y)", "x / (-2) > 0",
            "x!", "(x + 1)! / x!", "[1; 2] \\/ [3; 4]", "A \\/ A", "x in [1; 2]",
            "(x + 1) * (x - 1)", "x ^ 2 - 1", "(x + y) ^ 2", "1 / (1 + sqrt(2))",
            "x * (y / z)", "(x / y) * z", "x / y / z", "x / (y / z)",
            "2 * x + 3 * x", "x + x", "x - x", "x * x", "a * c + a / b",
        };

        private static readonly string[] Leaves =
            { "x", "y", "2", "-1", "-2", "1/2", "-1/2", "1", "0" };

        private static readonly string[] Unary =
            { "-({0})", "1 / ({0})", "({0}) ^ 2", "({0}) ^ (-2)", "sqrt({0})", "sin({0})", "abs({0})" };

        private static readonly string[] Binary =
            { "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})" };

        /// <summary>
        /// The seeds above, and the generated arithmetic the growth check uses on top of them.
        /// The seeds alone are shapes somebody thought of; the grammar reaches the arrangements
        /// nobody would write down, which is where <c>-x / (-y)</c> came from in the first place.
        /// </summary>
        private static IEnumerable<Entity> Corpus()
        {
            var generated = new List<string>(Leaves);
            var level2 = new List<string>();
            foreach (var shape in Unary)
                foreach (var inner in generated)
                    level2.Add(string.Format(shape, inner));
            foreach (var shape in Binary)
                foreach (var left in generated)
                    foreach (var right in generated)
                        level2.Add(string.Format(shape, left, right));
            var level3 = new List<string>();
            foreach (var shape in Binary)
                foreach (var left in level2.Where((_, i) => i % 17 == 0))
                    foreach (var right in level2.Where((_, i) => i % 23 == 0))
                        level3.Add(string.Format(shape, left, right));

            foreach (var source in Seeds.Concat(generated).Concat(level2).Concat(level3))
            {
                Entity parsed;
                try { parsed = source.ToEntity(); }
                catch { continue; }
                yield return parsed;
            }
        }

        /// <summary>How many rewrites to allow before calling it divergence rather than a cycle.</summary>
        private const int Steps = 64;

        /// <summary>
        /// The cycles that are known, filed and deliberately still here. Written as the whole loop
        /// rather than as a set name, so that the entry says what it is excusing.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Both are <c>Power</c> holding an inverse pair:
        /// <c>two-powers-of-one-exponent-share-a-base</c> collects <c>a ^ b * c ^ b</c> into
        /// <c>(a * c) ^ b</c>, and <c>a-numeric-factor-comes-out-of-a-power-of-a-product</c> takes
        /// it straight back. Neither rule is wrong and each is wanted in its own direction; a
        /// *set* containing both has no normal form to reach, which is
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1171">#1171</a>.
        /// <see cref="Entity.Simplify()"/> is unaffected — it folds between passes, so <c>1 * x</c>
        /// collapses and the shape the second rule needs stops existing.
        /// </para>
        /// <para>
        /// <b>The second rule was named wrongly here first, and the name is the whole content of
        /// an entry like this.</b> It said <c>positive-power-of-a-product-distributes</c>, which is
        /// a real rule with the right shape and is in <c>CollapseMultipleFractions</c>, not in
        /// <c>Power</c> — so <see cref="MatchedRuleSet.ApplyHere"/> on <c>Power</c> can never reach
        /// it. Asking the set which of its own rules fires is one loop and settles it; reading two
        /// rule names off their patterns does not.
        /// </para>
        /// </remarks>
        private static readonly string[] KnownCycles =
        {
            "Power: 1 ^ (-2) * x ^ (-2)  ->  (1 * x) ^ (-2)  ->  1 ^ (-2) * x ^ (-2)",
            "Power: (-2) ^ 2 * (1/2) ^ 2  ->  ((-2) * 1/2) ^ 2  ->  (-2) ^ 2 * (1/2) ^ 2",
        };

        [Fact]
        public void NoRuleSetRewritesBackToATermItHasAlreadyProduced()
        {
            var cycles = new List<string>();

            foreach (var set in MatchedRules.All)
                foreach (var start in Corpus())
                {
                    var seen = new List<Entity> { start };
                    var current = start;
                    for (var step = 0; step < Steps; step++)
                    {
                        Entity next;
                        try { next = set.ApplyHere(current); }
                        catch { break; /* a rule declining loudly is not this test's subject */ }
                        if (next == current) break;               // a fixed point, which is the point
                        var revisited = seen.IndexOf(next);
                        if (revisited >= 0)
                        {
                            var loop = seen.Skip(revisited).Append(next)
                                .Select(e => e.Stringize());
                            cycles.Add($"{set.Name}: {string.Join("  ->  ", loop)}");
                            break;
                        }
                        seen.Add(next);
                        current = next;
                    }
                }

            var found = cycles.Distinct().ToList();
            var unexpectedCycles = found.Except(KnownCycles).ToList();
            Assert.True(unexpectedCycles.Count == 0,
                $"{unexpectedCycles.Count} rule sets rewrite back to a term they have already "
                + "produced:\n" + string.Join("\n", unexpectedCycles));

            // And each known one is still there, so that fixing #1171 deletes the entry rather
            // than leaving a loop written out here that no longer happens.
            var goneCycles = KnownCycles.Except(found).ToList();
            Assert.True(goneCycles.Count == 0,
                $"{goneCycles.Count} entries above no longer cycle and should be deleted:\n"
                + string.Join("\n", goneCycles));
        }

        /// <summary>
        /// The same question over a whole tree rather than at the root, which is the half the
        /// loop above cannot answer: a full pass can move its rewrite to a different node each
        /// time and never repeat a term while still never settling.
        /// </summary>
        /// <remarks>
        /// <c>UntilStable</c> reports hitting its bound as <b>no answer</b> rather than as the
        /// last value reached, and its remarks say why — "an unbounded rewrite loop is the failure
        /// mode this layer is supposed to make visible, and handing back a value from the middle
        /// of one would hide exactly the case worth seeing". This is a caller taking it up on
        /// that.
        /// </remarks>
        [Fact]
        public void EveryRegisteredSetSettlesOnTheSeeds()
        {
            // The two shapes of #1171, which are the whole-tree face of the pair in
            // <see cref="KnownCycles"/>. Nothing else is excused: NumericNeat grew `-x / (-y)` by
            // four nodes a pass for ever until #1167, and its entry here is gone rather than left
            // behind meaning nothing.
            var known = new[]
            {
                "Power: 1 ^ (-2) * x ^ (-2)",
                "Power: (-2) ^ 2 * (1/2) ^ 2",
            };

            var unsettled = new List<string>();

            foreach (var set in RewriteRules.All)
            {
                var rewriting = Transformation.Rewriting(set).UntilStable(Steps);
                foreach (var start in Corpus())
                {
                    TransformationResult result;
                    try { result = rewriting.Apply(start); }
                    catch { continue; /* a rule declining loudly is not this test's subject */ }
                    if (!result.Succeeded)
                        unsettled.Add($"{set.Name}: {start.Stringize()}");
                }
            }

            var unexpected = unsettled.Distinct().Except(known).ToList();
            Assert.True(unexpected.Count == 0,
                $"{unexpected.Count} set-and-expression pairs newly fail to reach a fixed point "
                + $"in {Steps} passes:\n" + string.Join("\n", unexpected.Take(20)));

            // And each known one is still known: fixing it should delete it from the list rather
            // than leave a name here that no longer means anything.
            var gone = known.Except(unsettled).ToList();
            Assert.True(gone.Count == 0,
                $"{gone.Count} entries above now settle and should be deleted:\n"
                + string.Join("\n", gone));
        }

        /// <summary>
        /// The loop above has to actually rewrite something, or it passes by never firing. Named
        /// as a count that moves rather than as "it works".
        /// </summary>
        [Fact]
        public void TheSeedsMakeTheSetsRewrite()
        {
            var rewriting = 0;
            foreach (var set in MatchedRules.All)
                foreach (var start in Corpus())
                {
                    Entity next;
                    try { next = set.ApplyHere(start); }
                    catch { continue; }
                    if (next != start) { rewriting++; break; }
                }

            Assert.True(rewriting >= 20,
                $"only {rewriting} of the {MatchedRules.All.Count} sets rewrote any seed at all");
        }
    }
}
