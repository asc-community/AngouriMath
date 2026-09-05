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

        private static IEnumerable<Entity> Corpus()
        {
            foreach (var source in Seeds)
            {
                Entity parsed;
                try { parsed = source.ToEntity(); }
                catch { continue; }
                yield return parsed;
            }
        }

        /// <summary>How many rewrites to allow before calling it divergence rather than a cycle.</summary>
        private const int Steps = 64;

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

            Assert.True(cycles.Count == 0,
                $"{cycles.Count} rule sets rewrite back to a term they have already produced:\n"
                + string.Join("\n", cycles.Distinct()));
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
