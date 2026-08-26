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
using AngouriMath;
using AngouriMath.Core.Transformations;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using AngouriMath.Functions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// <c>MatchedRuleSet.ApplyHere</c> asks only the rules whose pattern requires a root type the
    /// node is. This is what says that skipping the rest skips nothing.
    /// </summary>
    /// <remarks>
    /// The index is derived from <c>MatchPattern.RequiredRootType</c>, so it is only as sound as
    /// that property is — a pattern that under-reports what it requires would have rules skipped
    /// that could have fired, and the answer would silently stop changing. Comparing against a
    /// linear scan over the whole set is the check that cannot be fooled by the same mistake.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RuleIndexTest
    {
        private static IEnumerable<MatchedRuleSet> AllSets
        {
            get
            {
                const BindingFlags Any = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
                foreach (var property in typeof(MatchedRules).GetProperties(Any))
                    if (property.PropertyType == typeof(MatchedRuleSet))
                        yield return (MatchedRuleSet)property.GetValue(null)!;
                foreach (var factory in typeof(MatchedRules).GetMethods(Any))
                    if (factory.ReturnType == typeof(MatchedRuleSet)
                        && factory.GetParameters() is { Length: 1 } parameters
                        && parameters[0].ParameterType == typeof(TreeAnalyzer.SortLevel))
                        foreach (var level in Enum.GetValues(typeof(TreeAnalyzer.SortLevel)))
                            yield return (MatchedRuleSet)factory.Invoke(null, new[] { level })!;
            }
        }

        private static readonly string[] Leaves = { "x", "y", "2", "-1", "-2", "1/2", "1", "0" };

        private static readonly string[] Shapes =
        {
            "-({0})", "1 / ({0})", "({0}) ^ 2", "sqrt({0})", "sin({0})", "abs({0})", "sgn({0})",
            "({0})!", "cos({0})", "tan({0})", "log(2, {0})", "not ({0} > 1)",
        };

        private static readonly string[] Pairs =
        {
            "({0}) + ({1})", "({0}) - ({1})", "({0}) * ({1})", "({0}) / ({1})", "({0}) ^ ({1})",
            "({0}) = ({1})", "({0}) < ({1})", "({0}) >= ({1})",
            "({0}) and ({1})", "({0}) or ({1})", "({0}) implies ({1})",
        };

        private static List<Entity> Corpus()
        {
            var sources = new List<string>(Leaves);
            foreach (var shape in Shapes)
                foreach (var inner in Leaves)
                    sources.Add(string.Format(shape, inner));
            foreach (var shape in Pairs)
                foreach (var left in Leaves)
                    foreach (var right in Leaves)
                        sources.Add(string.Format(shape, left, right));
            sources.AddRange(new[]
            {
                @"A /\ B", @"A \/ B", @"A \ B", "{ 1, 2 }", "[0; 1]", "(-oo; +oo)",
                "{ x : x > 0 }", "x in [0; 1]", "1 provided x > 0",
            });

            var parsed = new List<Entity>();
            foreach (var source in sources)
            {
                try { parsed.Add(source.ToEntity()); }
                catch { /* the generator makes some strings the parser declines */ }
            }
            return parsed;
        }

        /// <summary>
        /// The indexed answer against the answer a scan over every rule gives, for every set and
        /// every expression — including the nodes inside each one, so a type the corpus only
        /// produces as a child is covered too.
        /// </summary>
        [Fact]
        public void TheIndexSkipsOnlyRulesThatCouldNotHaveFired()
        {
            var corpus = Corpus();
            Assert.True(corpus.Count > 500, $"the corpus is only {corpus.Count} expressions");

            var compared = 0;
            var changed = 0;
            foreach (var set in AllSets)
                foreach (var expr in corpus.SelectMany(Everywhere))
                {
                    var indexed = set.ApplyHere(expr);

                    var linear = expr;
                    foreach (var rule in set.Rules)
                        if (rule.TryApply(expr) is { } rewritten) { linear = rewritten; break; }

                    Assert.True(linear.Equals(indexed),
                        $"{set.Name} on '{expr.Stringize()}': a scan gave "
                        + $"'{linear.Stringize()}', the index gave '{indexed.Stringize()}'");
                    if (!indexed.Equals(expr)) changed++;
                    compared++;
                }

            Assert.True(compared > 20000, $"only {compared} set/expression pairs compared");
            // Agreement between two things that both did nothing is not agreement.
            Assert.True(changed > 500, $"the rules only fired {changed} times");
        }

        private static IEnumerable<Entity> Everywhere(Entity expr)
        {
            yield return expr;
            foreach (var child in expr.DirectChildren)
                foreach (var inner in Everywhere(child))
                    yield return inner;
        }
    }
}
