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
        /// Shapes the arithmetic grammar does not build. Without them the corpus reaches the sets
        /// about quotients and powers and none of the six about trigonometry, booleans, sets,
        /// comparisons or factorials — so those rules' declarations would be checked by a test
        /// that never ran them.
        /// </summary>
        private static readonly string[] Reached =
        {
            // trigonometric, and the arguments the multiple-angle and collapse rules want
            "sin(x)", "cos(x)", "tan(x)", "cotan(x)", "sec(x)", "cosec(x)",
            "sin(2 * x)", "cos(2 * x)", "sin(x + y)", "cos(x + y)", "sin(3 * x)", "cos(3 * x)",
            "sin(x) * cos(x)", "cos(x) * sin(x)", "sin(x) ^ 2 + cos(x) ^ 2", "cos(x) ^ 2 + sin(x) ^ 2",
            "sin(x) / cos(x)", "cos(x) / sin(x)", "1 / sin(x)", "1 / cos(x)", "1 / tan(x)",
            "tan(x) * cotan(x)", "sin(x) ^ 2", "cos(x) ^ 2", "sin(x) ^ 2 - cos(x) ^ 2",
            "arcsin(x)", "arccos(x)", "arctan(x)", "arccotan(x)",
            "arcsin(x) + arccos(x)", "arctan(x) + arccotan(x)", "sin(arcsin(x))", "cos(arccos(x))",

            // logarithms and exponentials
            "ln(x)", "log(2, x)", "ln(x * y)", "ln(x ^ 2)", "e ^ ln(x)", "ln(e ^ x)",
            "log(x, x)", "2 ^ x * 2 ^ y", "e ^ x * e ^ y",

            // factorials, both sets
            "x!", "(x + 1)!", "(x + 1)! / x!", "x! / (x + 1)!", "x! * (x + 1)", "(x + 1) * x!",
            "(x + 2)! / x!",

            // boolean
            "a and b", "a or b", "not a", "not (not a)", "a implies b", "a xor b",
            "a and not a", "a or not a", "a and a", "a or a", "a and (a or b)", "a or (a and b)",
            "not (a and b)", "not (a or b)", "a and (b or c)", "(a and b) or (a and c)",
            "true and a", "false or a", "true or a", "false and a",

            // comparisons and the equality set
            "x > y", "x < y", "x >= y", "x <= y", "x = y", "(x < y) or (x = y)", "(x > y) or (x = y)",
            "not (x > y)", "not (x = y)",

            // sets
            "x in [1; 2]", "[1; 2] \\/ [3; 4]", "[1; 2] /\\ [3; 4]", "[1; 2] \\ [3; 4]",
            "A \\/ A", "A /\\ A", "A \\/ {}", "A /\\ {}", "{ t : t > 0 }",

            // powers and roots the arithmetic grammar reaches only shallowly
            "sqrt(x) * sqrt(x)", "sqrt(x ^ 2)", "(x ^ 2) ^ 3", "x ^ 2 * x ^ 3", "x ^ 3 / x ^ 2",
            "x ^ 2 * y ^ 2", "(x / y) ^ 2", "x ^ (1/2) * x ^ (1/2)", "(-x) ^ 2",
            "1 / x ^ (-2)", "x ^ 0", "0 ^ x",

            // polynomial shapes for the division, gcd and factorisation sets
            "(x ^ 2 - 1) / (x - 1)", "(x ^ 2 + 2 * x + 1) / (x + 1)", "x ^ 2 - 1",
            "x ^ 2 + 2 * x + 1", "x ^ 3 - 1", "x ^ 2 - y ^ 2", "x ^ 4 - y ^ 4",
            "x ^ 2 + 2 * x * y + y ^ 2", "(x + y) ^ 2", "(x + y) * (x - y)",
            "1 / (1 + sqrt(2))", "1 / (sqrt(3) - 1)", "2 / (1 + sqrt(x))",

            // Comparisons. The largest block of rules the arithmetic grammar cannot reach at all:
            // they key on zero or a number being on the left, on both sides being the same thing,
            // and on a negative factor or divisor standing beside a zero.
            "0 > x", "0 < x", "0 >= x", "0 <= x", "0 = x",
            "2 > x", "2 < x", "2 >= x", "2 <= x", "2 = x",
            "x > x", "x < x", "x >= x", "x <= x",
            "x / (-2) > 0", "x / (-2) < 0", "x / (-2) >= 0", "x / (-2) <= 0", "x / (-2) = 0",
            "(-2) * x > 0", "(-2) * x < 0", "(-2) * x >= 0", "(-2) * x <= 0", "(-2) * x = 0",
            "x * (-2) > 0", "x * (-2) < 0", "x * (-2) >= 0", "x * (-2) <= 0", "x * (-2) = 0",
            "(x > y) and (y > z)", "(x < y) and (y < z)",
            "not (x >= y)", "not (x <= y)", "not (x < y)", "y > x", "y >= x",

            // Boolean, both directions of De Morgan and the absorptions with a negation.
            "not a and not b", "not a or not b", "a and not b", "a or not b",
            "not a or b", "not a and b", "a and (b and c)", "a or (b or c)",
            "(a and b) and c", "(a or b) or c", "(a or b) and (a or c)", "(a and b) or c",

            // The trigonometric reciprocal pairs and the inverses the shapes above miss.
            "cosec(x) * sin(x)", "sin(x) * cosec(x)", "sec(x) * cos(x)", "cos(x) * sec(x)",
            "cotan(arccotan(x))", "tan(arctan(x))", "arcsin(sin(x))", "arctan(tan(x))",
            "sin(2 * x) * cosec(x)", "cos(2 * x) * sec(x)", "cotan(x) * tan(x)",

            // Logarithms in and of reciprocals.
            "log(1/2, x)", "log(2, 1/x)", "log(1/2, 1/x)", "log(1/x, y)", "log(x, 1/y)",

            // A factorial beside a zero, and the differences and products that contain their own
            // operand a second time.
            "x! = 0", "(x - y) / (y - x)", "x - (x - y)", "(x - y) - x",
            "x * y - x", "x - x * y", "x * x", "x * x * y", "x * (x * y)",
            "sin(x) * 2", "2 * sin(x)",
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

        private static int Size(Entity entity) => entity.Nodes.Count();

        /// <summary>
        /// Every firing of every rule over the corpus, as (rule, how the size moved). A rule that
        /// hands back what it was given has not fired and is not counted — the factorial sets
        /// have arms that match a shape and then decline.
        /// </summary>
        private static Dictionary<MatchedRule, List<(Entity Before, Entity After)>> Firings()
        {
            // Every node, not only the root: a rule matches where its shape sits, and asking
            // only about whole corpus expressions leaves most rules never firing at all.
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

            // Named as counts that move rather than as "most of them". 194 of the 324 rules reach
            // the corpus and 59 of those carry a declaration, so the check above is exercised
            // rather than passing by never running; the rest are the measured evidence for
            // declaring more of them. It was 84 and 18 before the shapes in `Reached` were added
            // and before rules were tried at every node rather than only at the root.
            Assert.True(fired.Count >= 190, $"only {fired.Count} rules fired at all");
            Assert.True(declared >= 56, $"only {declared} rules with a declared growth fired");
        }
    }
}
