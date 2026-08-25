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
        /// <c>Power</c> splits a power of a product — <c>(2 * x) ^ 2</c> to <c>2 ^ 2 * x ^ 2</c> —
        /// and something has to fold <c>2 ^ 2</c> before the result stops being a power of a
        /// product. <c>NumericNeat</c> rewrites <c>--x</c> through a product of ones that only
        /// collapses when the normalisation multiplies them out. Both are the same shape: a rule
        /// whose right-hand side is a fixed point only after arithmetic that the set itself does
        /// not do.
        /// </remarks>
        private static readonly HashSet<string> SettleOnlyComposed = new() { "Power", "NumericNeat" };

        private static readonly string[] Leaves = { "x", "y", "2", "-1", "1/2", "1", "0" };

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

            var parsed = new List<Entity>();
            foreach (var source in level1.Concat(level2).Concat(level3))
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the generator makes some strings the parser declines */ }
            }
            return parsed;
        }

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
        /// <c>Common</c> has a three-cycle on <c>-x * 1/2</c>:
        /// <c>Mulf(-1/2, x)</c> to <c>Mulf(-1, Divf(x, 2))</c> to <c>Divf(Mulf(-1, x), 2)</c> and
        /// back — three trees printing as two strings, which is why it wants writing down as
        /// shapes. Two rules disagree about whether <c>c * x</c> or <c>(c * x) / d</c> is the
        /// destination. <c>Simplify</c> bounds its own iteration and does not hang, so this is a
        /// property of the set rather than a defect a caller sees today
        /// (<a href="https://github.com/asc-community/AngouriMath/issues/1056">#1056</a>).
        /// </remarks>
        private static readonly HashSet<string> NeverSettle = new() { "Common" };

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
