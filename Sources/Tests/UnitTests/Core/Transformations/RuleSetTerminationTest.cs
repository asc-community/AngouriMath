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
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// Every rule set in the registry reaches a fixed point when it is iterated.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 asks for
    /// "termination checked by tooling rather than asserted by authors". The tooling existed and
    /// lived in a workspace harness, so it answered for whoever ran it and for no one else. A
    /// non-terminating rule set is a hang rather than a wrong answer, and a hang is what the
    /// suite is worst at reporting, so this is the check worth having where the build can see it.
    /// </para>
    /// <para>
    /// Two questions, because they have different answers. <b>Alone</b> is what tier 2 asks: rules
    /// as first-class data means a caller may apply one set by itself. <b>Composed</b> is how the
    /// simplifier actually runs them, with the normalisation between passes. A set that settles
    /// only in composition is half a rewrite system, and until this test there was nothing saying
    /// which ones those were outside a generated report.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleSetTerminationTest
    {
        /// <summary>Applications before a set is called non-terminating on an input.</summary>
        private const int MaxPasses = 64;

        /// <summary>
        /// The sets that cycle when iterated on their own and settle once the normalisation runs
        /// between passes. Named rather than counted: a count going from two to two says nothing
        /// when one set has been fixed and another has started, and a name is what a failure
        /// needs to be actionable.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Empty, and both entries it held have been used.</b> Every registered set reaches a
        /// fixed point on its own now; none of them needs the normalisation between passes to
        /// stop. That is worth stating, because it is the property
        /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a> tier 2 asks
        /// for and it was not true of this registry until recently.
        /// </para>
        /// <para>
        /// <c>NumericNeat</c> rewrote <c>--x</c> through a product of ones that collapsed only
        /// when the normalisation multiplied them out, because four rules took a factor of
        /// <c>-1</c> out of a product and left its magnitude behind as a literal <c>1 *</c>. They
        /// decline <c>-1</c> now, it being the sign rather than a factor to take out:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1167">#1167</a>.
        /// </para>
        /// <para>
        /// <c>Power</c> split a power of a product — <c>(2 * x) ^ 2</c> to <c>2 ^ 2 * x ^ 2</c> —
        /// and something had to fold <c>2 ^ 2</c> before the result stopped being a power of a
        /// product. Its collector and its numeric-factor rule were an inverse pair that undid each
        /// other, and each declines the other's shape now:
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1171">#1171</a>.
        /// </para>
        /// <para>
        /// Named rather than counted, and asserted in both directions, so a set that starts
        /// needing the normalisation fails here rather than being absorbed into a number.
        /// </para>
        /// </remarks>
        private static readonly HashSet<string> SettleOnlyComposed = new();

        // `-2` is here because `-1` is not enough of a negative: the four rules that take a
        // negative factor out of a product decline a factor of -1 since #1167, so a corpus whose
        // only negative is -1 cannot make them fire at all.
        private static readonly string[] Leaves = { "x", "y", "2", "-1", "-2", "1/2", "1", "0" };

        private static readonly string[] Unary =
        {
            "-({0})", "1 / ({0})", "({0}) ^ 2", "sqrt({0})", "sin({0})", "abs({0})",
        };

        private static readonly string[] Binary =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
        };

        /// <summary>
        /// Parsed once. Rebuilding it per rule set parsed the same few hundred strings thirty
        /// times over, which is the whole cost of this test and none of its coverage.
        /// </summary>
        private static readonly IReadOnlyList<Entity> Inputs = BuildCorpus();

        private static List<Entity> BuildCorpus()
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

            // A third level, and it is load-bearing: the shapes that cycle are a unary applied
            // to something already compound -- `(2 * x) ^ 2`, `--x`, `sqrt(1/2 * 2)`. Without it
            // this corpus reproduces none of them and the list below reads as empty, which is
            // exactly what the second assertion caught the first time it ran.
            var level3 = new List<string>();
            foreach (var shape in Unary)
                foreach (var inner in level2)
                    level3.Add(string.Format(shape, inner));

            // And binary at the third level too, sampled, because a unary-only third level never
            // builds a quotient of quotients or a product of quotients -- the shapes where two
            // rules of one set both fire on one node. `RulePriorityTest` measured what that
            // costs on its own corpus: growing the third level with binary shapes took the
            // conflicts it can see from 3 to 45. This corpus had the same blind spot.
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

        /// <summary>
        /// The corpus is the whole strength of the two checks below, and neither of them fails
        /// when it shrinks — a smaller corpus makes them pass sooner, which is the failure mode a
        /// coverage number exists to stop. <b>4344</b> inputs, of which <b>1760</b> are the binary
        /// third level (5 shapes over 22 sampled left operands and 16 sampled right ones).
        /// </summary>
        [Fact]
        public void TheCorpusIsTheSizeTheseChecksWereMeasuredOn() =>
            Assert.Equal(4344, Inputs.Count);

        private static bool Terminates(RewriteRuleSet set, Entity from, bool normalise, out Entity stuck)
        {
            var current = from;
            for (var pass = 0; pass < MaxPasses; pass++)
            {
                Entity applied;
                try
                {
                    applied = normalise
                        ? set.ApplyOnce(current).InnerSimplified
                        : set.ApplyOnce(current);
                }
                catch
                {
                    // A set that throws has not claimed anything about termination.
                    stuck = current;
                    return true;
                }
                if (applied.Equals(current))
                {
                    stuck = current;
                    return true;
                }
                current = applied;
            }
            stuck = current;
            return false;
        }

        /// <summary>
        /// The sets that reach no fixed point <b>even with the normalisation between passes</b>,
        /// which is how the simplifier runs them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Empty, and kept.</b> <c>Common</c> was the one entry: a three-cycle on
        /// <c>-x * 1/2</c>, <c>Mulf(-1/2, x)</c> to <c>Mulf(-1, Divf(x, 2))</c> to
        /// <c>Divf(Mulf(-1, x), 2)</c> and back — three trees printing as two strings, which is
        /// why the remark wrote them as shapes.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a>
        /// </para>
        /// <para>
        /// The pair that turned it were exact inverses on one shape:
        /// <c>a-numeric-quotient-of-a-numeric-multiple-collects-its-numbers</c> reads
        /// <c>(-1 * x) / 2</c> as a numeric factor to collect, giving <c>-1/2 * x</c>, and
        /// <c>a-negated-reciprocal-rational-factor-is-a-negated-division</c> reads that back as
        /// <c>-(x / 2)</c>. The first now declines a factor of <c>-1</c>, which is the sign rather
        /// than a number to collect. The positive case never cycled, because <c>x / 2</c> is a
        /// <c>Divf</c> over a leaf and so does not re-enter the first rule's pattern at all — the
        /// loop existed only because a negation is spelled as a product.
        /// </para>
        /// <para>
        /// The list stays because the shape of the assertion is what matters: it is held in both
        /// directions, so a set that starts cycling fails and a set that stops cycling has to
        /// leave it, which is how this one was found to have stopped.
        /// </para>
        /// </remarks>
        private static readonly HashSet<string> NeverSettle = new();

        /// <summary>
        /// With the normalisation between passes — how the simplifier runs them — every set
        /// settles except those named, and the naming is asserted in both directions.
        /// </summary>
        [Fact]
        public void OnlyTheNamedSetsReachNoFixedPoint()
        {
            var cycling = new SortedSet<string>();
            var examples = new List<string>();
            foreach (var set in RewriteRules.All)
                foreach (var expr in Inputs)
                {
                    Entity once;
                    try { once = set.ApplyOnce(expr); } catch { continue; }
                    if (once.Equals(expr))
                        continue;
                    if (!Terminates(set, once, normalise: true, out var stuck))
                    {
                        cycling.Add(set.Name);
                        if (examples.Count < 10)
                            examples.Add($"{set.Name} on `{expr.Stringize()}`, stuck at `{stuck.Stringize()}`");
                    }
                }

            var started = cycling.Except(NeverSettle).ToList();
            Assert.True(started.Count == 0,
                "these sets reach no fixed point in " + MaxPasses + " passes even with the "
                + "normalisation between them: " + string.Join(", ", started)
                + "\n" + string.Join("\n", examples));

            var stopped = NeverSettle.Except(cycling).ToList();
            Assert.True(stopped.Count == 0,
                "these sets terminate now and should leave the list: " + string.Join(", ", stopped));
        }

        /// <summary>
        /// Iterated on its own, every set settles except the two named — and those two must still
        /// be the ones named, so that a set which starts cycling is a failure rather than a
        /// number that moved.
        /// </summary>
        [Fact]
        public void OnlyTheNamedSetsNeedTheNormalisationToSettle()
        {
            var cycling = new SortedSet<string>();
            foreach (var set in RewriteRules.All)
                foreach (var expr in Inputs)
                {
                    Entity once;
                    try { once = set.ApplyOnce(expr); } catch { continue; }
                    if (once.Equals(expr))
                        continue;
                    if (!Terminates(set, once, normalise: false, out _))
                        cycling.Add(set.Name);
                }

            // A set that reaches no fixed point even composed reaches none alone either, so the
            // two lists together are what may cycle here.
            var known = new HashSet<string>(SettleOnlyComposed);
            known.UnionWith(NeverSettle);

            var started = cycling.Except(known).ToList();
            Assert.True(started.Count == 0,
                "these sets have started cycling when iterated alone: " + string.Join(", ", started));

            // The other direction, so the list cannot outlive what it describes.
            var stopped = known.Except(cycling).ToList();
            Assert.True(stopped.Count == 0,
                "these sets no longer need the normalisation and should leave the list: "
                + string.Join(", ", stopped));
        }
    }
}
