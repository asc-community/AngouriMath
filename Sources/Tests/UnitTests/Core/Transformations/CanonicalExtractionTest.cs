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
using AngouriMath.Core.Budgets;
using AngouriMath.Core.Transformations;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A canonical form asks a different question from a cost model. A cost model asks which of
    /// two expressions is <i>nicer</i>, and answers with a number, so two it cannot separate tie —
    /// and a tie is settled by whoever was reached first, which is not a form anybody chose.
    /// <see cref="EntityOrder.Canonical"/> asks which one <i>is the representative</i>, and answers
    /// with an order, so there is exactly one least member and it is the same one every time.
    /// </summary>
    [Trait("Area", "Transformations")]
    public sealed class CanonicalExtractionTest
    {
        private static readonly string[] Corpus =
        {
            "x", "1", "0", "-1", "1/2", "x + y", "y + x", "x * y", "x - 1", "-1 + x",
            "sin(x)", "cos(x)", "sin(x) + cos(x)", "(x + 1) ^ 2", "a * b + a * c",
            "1 / x", "x / y", "x ^ 2", "sqrt(x)", "ln(x)", "x > 0", "a and b",
        };

        private static IEnumerable<Entity> Parsed => Corpus.Select(source => source.ToEntity());

        #region The order itself

        /// <summary>
        /// Total up to <see cref="object.Equals(object)"/>: two expressions compare equal exactly
        /// when they are the same expression. Without that a canonical form is not well defined —
        /// two distinct members of a class could both be "least".
        /// </summary>
        [Fact]
        public void ComparingEqualIsBeingEqual()
        {
            foreach (var left in Parsed)
                foreach (var right in Parsed)
                    Assert.Equal(left.Equals(right), EntityOrder.Canonical.Compare(left, right) == 0);
        }

        [Fact]
        public void TheOrderIsAntisymmetric()
        {
            foreach (var left in Parsed)
                foreach (var right in Parsed)
                    Assert.Equal(
                        Math.Sign(EntityOrder.Canonical.Compare(left, right)),
                        -Math.Sign(EntityOrder.Canonical.Compare(right, left)));
        }

        [Fact]
        public void TheOrderIsTransitive()
        {
            var all = Parsed.ToList();
            foreach (var a in all)
                foreach (var b in all)
                    foreach (var c in all)
                        if (EntityOrder.Canonical.Compare(a, b) < 0 && EntityOrder.Canonical.Compare(b, c) < 0)
                            Assert.True(EntityOrder.Canonical.Compare(a, c) < 0,
                                $"{a.Stringize()} < {b.Stringize()} < {c.Stringize()} but not a < c");
        }

        /// <summary>
        /// Size first, so the representative of a class is its smallest member rather than
        /// whichever one happens to sort first. A canonical form that preferred the larger writing
        /// would be well defined and useless.
        /// </summary>
        [Fact]
        public void SmallerComesFirst()
        {
            Assert.True(EntityOrder.Canonical.Compare("x".ToEntity(), "x + y".ToEntity()) < 0);
            Assert.True(EntityOrder.Canonical.Compare("x + y".ToEntity(), "(x + 1) ^ 2 + y".ToEntity()) < 0);
        }

        /// <summary>
        /// It does not depend on the culture, on a hash, or on anything else that changes between
        /// two runs of the same program — the property <c>ENode</c>'s own order exists for.
        /// </summary>
        [Fact]
        public void TheOrderIsTheSameOnASecondPass()
        {
            var once = Parsed.OrderBy(e => e, EntityOrder.Canonical).Select(e => e.Stringize()).ToList();
            var twice = Parsed.Reverse().OrderBy(e => e, EntityOrder.Canonical).Select(e => e.Stringize()).ToList();
            Assert.Equal(once, twice);
        }

        #endregion

        #region Extraction by that order

        [Fact]
        public void TheLeastMemberOfAClassIsWhatComesBack()
        {
            var graph = new EGraph();
            var big = graph.AddEntity("(x + 1) ^ 2".ToEntity());
            graph.Union(big, graph.AddEntity("y".ToEntity()));

            Assert.Equal("y".ToEntity(), graph.ExtractLeast(big, EntityOrder.Canonical));
        }

        /// <summary>
        /// The whole point: two writings of one thing, put in one class, extract to the same tree.
        /// Under a cost model they can tie and then the answer is whichever was reached first.
        /// </summary>
        [Fact]
        public void TwoWritingsOfOneClassExtractToOneTree()
        {
            var graph = new EGraph();
            var first = graph.AddEntity("x + y".ToEntity());
            var second = graph.AddEntity("y + x".ToEntity());
            graph.Union(first, second);
            graph.Rebuild();

            Assert.Equal(
                graph.ExtractLeast(first, EntityOrder.Canonical),
                graph.ExtractLeast(second, EntityOrder.Canonical));
        }

        /// <summary>
        /// And it does not depend on which one was inserted first, which a set-order-dependent
        /// extraction would.
        /// </summary>
        [Fact]
        public void ExtractionDoesNotDependOnInsertionOrder()
        {
            static Entity? Extract(string a, string b)
            {
                var graph = new EGraph();
                var first = graph.AddEntity(a.ToEntity());
                graph.Union(first, graph.AddEntity(b.ToEntity()));
                graph.Rebuild();
                return graph.ExtractLeast(first, EntityOrder.Canonical);
            }

            Assert.Equal(Extract("x + y", "y + x"), Extract("y + x", "x + y"));
            Assert.Equal(Extract("sin(x)", "cos(x)"), Extract("cos(x)", "sin(x)"));
        }

        [Fact]
        public void ExtractLeastDeclinesAClassItCannotBuild()
        {
            var graph = new EGraph();
            var root = graph.AddEntity("{ 1, 2 }".ToEntity());

            Assert.Null(graph.ExtractLeast(root, EntityOrder.Canonical));
        }

        /// <summary>
        /// A narrowed codomain survives extraction here for the same reason it does under a cost
        /// model — it is per-node data the rebuild has to put back.
        /// </summary>
        [Fact]
        public void ExtractLeastPreservesANarrowedCodomain()
        {
            Entity narrowed = MathS.Sqrt(-1).WithCodomain(Domain.Real);
            var graph = new EGraph();
            var root = graph.AddEntity(narrowed);

            var extracted = graph.ExtractLeast(root, EntityOrder.Canonical);

            Assert.NotNull(extracted);
            Assert.Equal(Domain.Real, extracted!.Codomain);
        }

        #endregion

        #region The transformation

        private static WorkBudget Budget { get; }
            = new() { Steps = 300_000, Time = TimeSpan.FromSeconds(20) };

        private static Transformation Narrow => Transformation.CanonicalizationOverGraph(Budget);

        private static Transformation Wide
            => Transformation.CanonicalizationOverGraph(Budget, RewriteRuleGrowth.Unknown);

        /// <summary>
        /// The property that makes it a canonical form at all: running it twice says nothing more
        /// than running it once. Without this the answer is a step in a process rather than a form.
        /// </summary>
        [Theory]
        [InlineData("x + y")]
        [InlineData("y + x")]
        [InlineData("x - 1")]
        [InlineData("a * b + a * c")]
        [InlineData("1 / x + 1 / y")]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2")]
        [InlineData("(x + 1) ^ 2")]
        [InlineData("x > 0")]
        public void CanonicalisingTwiceIsCanonicalisingOnce(string source)
        {
            var once = Narrow.ApplyOrKeep(source.ToEntity());
            Assert.Equal(once, Narrow.ApplyOrKeep(once));
        }

        /// <summary>
        /// And the property that makes it worth having: two writings of one expression reach the
        /// same tree, not merely trees that print alike.
        /// </summary>
        [Theory]
        [InlineData("x + y", "y + x")]
        [InlineData("x - 1", "-1 + x")]
        [InlineData("x * y", "y * x")]
        [InlineData("(x + y) + a", "x + (y + a)")]
        public void TwoWritingsReachOneTree(string left, string right)
            => Assert.Equal(Narrow.ApplyOrKeep(left.ToEntity()), Narrow.ApplyOrKeep(right.ToEntity()));

        /// <summary>
        /// The extraction never picks a member larger than one it could have picked — the half of
        /// the order that makes it useful rather than merely well defined. Asserted of the graph
        /// step itself: the composed transformation runs a rule pass on each side of it, and
        /// <c>InnerSimplified</c> is free to enlarge on its way to a tidier form.
        /// </summary>
        [Theory]
        [InlineData("x + y", "(x + 1) ^ 2")]
        [InlineData("a", "a * b + a * c")]
        [InlineData("1 / x", "sqrt(2) / (sqrt(3) + sqrt(5))")]
        public void TheLeastMemberIsNeverTheLargerOne(string small, string large)
        {
            var graph = new EGraph();
            var id = graph.AddEntity(small.ToEntity());
            graph.Union(id, graph.AddEntity(large.ToEntity()));
            graph.Rebuild();

            var extracted = graph.ExtractLeast(id, EntityOrder.Canonical);

            Assert.NotNull(extracted);
            Assert.True(extracted!.Complexity <= large.ToEntity().Complexity);
        }

        /// <summary>
        /// What the wide ceiling buys: an equality that only shows through a <i>larger</i>
        /// intermediate form, which a rule pass cannot reach because having expanded it is
        /// standing on the expansion.
        /// </summary>
        [Fact]
        public void TheWideCeilingReachesWhatARulePassCannot()
        {
            var l = "sin(x) ^ 2 + cos(x) ^ 2".ToEntity();
            var r = "1".ToEntity();

            // The claim is about the ceiling, so it is only worth making where the pass fails.
            Assert.NotEqual(
                Transformation.Canonicalization.ApplyOrKeep(l),
                Transformation.Canonicalization.ApplyOrKeep(r));

            Assert.Equal(Wide.ApplyOrKeep(l), Wide.ApplyOrKeep(r));
        }

        /// <summary>
        /// Widening the ceiling brings the difference of squares together — the narrow ceiling
        /// leaves the product and the expansion as two forms, and the widest one canonicalises
        /// both to the same tree.
        /// </summary>
        [Fact]
        public void TheWidestCeilingBringsTheDifferenceOfSquaresTogether()
        {
            var product = "(x + y) * (x - y)".ToEntity();
            var squares = "x ^ 2 - y ^ 2".ToEntity();

            Assert.NotEqual(Narrow.ApplyOrKeep(product), Narrow.ApplyOrKeep(squares));
            Assert.Equal(Wide.ApplyOrKeep(product), Wide.ApplyOrKeep(squares));
        }

        /// <summary>
        /// The relationship that holds whether or not the rules are confluent, and the reason
        /// <see cref="Saturation.ProvesEqual"/> is the half to rely on: asking in one graph is
        /// never weaker than canonicalising separately and comparing. Two separate graphs each
        /// reach only what their own side reaches, so where the rules are not confluent one side
        /// can arrive somewhere the other cannot, and both are canonicalised correctly to
        /// different forms. One graph merges everything reachable from either.
        /// </summary>
        [Theory]
        [InlineData("x + y", "y + x")]
        [InlineData("x - 1", "-1 + x")]
        [InlineData("(x + y) * (x - y)", "x ^ 2 - y ^ 2")]
        [InlineData("sin(x) ^ 2 + cos(x) ^ 2", "1")]
        [InlineData("a * b + a * c", "a * (b + c)")]
        [InlineData("(x + 1) ^ 2", "x ^ 2 + 2 * x + 1")]
        [InlineData("1 / x + 1 / y", "(x + y) / (x * y)")]
        [InlineData("x", "y")]
        public void ProvingInOneGraphIsNeverWeakerThanComparingTwoCanonicalForms(string left, string right)
        {
            var l = left.ToEntity();
            var r = right.ToEntity();
            var rules = Saturation.RulesUpTo(RewriteRuleGrowth.Unknown);

            if (Wide.ApplyOrKeep(l).Equals(Wide.ApplyOrKeep(r)))
                Assert.True(Saturation.ProvesEqual(l, r, rules, Budget),
                    $"{left} and {right} canonicalise alike but one graph did not prove them equal");
        }

        /// <summary>
        /// A false is "not proved", never "unequal" — the distinction the library's own
        /// unevaluated-versus-NaN convention rests on.
        /// </summary>
        [Fact]
        public void NotProvedIsNotTheSameAsUnequal()
        {
            var rules = Saturation.RulesUpTo(RewriteRuleGrowth.Unknown);

            Assert.True(Saturation.ProvesEqual("x + 0".ToEntity(), "x".ToEntity(), rules, Budget));
            Assert.True(Saturation.ProvesEqual("x".ToEntity(), "x".ToEntity(), rules, Budget));

            // Genuinely unequal, and also not proved -- the two answers are the same word.
            Assert.False(Saturation.ProvesEqual("x".ToEntity(), "y".ToEntity(), rules, Budget));
            // Equal, and beyond what these rules reach: still not proved.
            Assert.False(Saturation.ProvesEqual(
                "1 / x + 1 / y".ToEntity(), "(x + y) / (x * y)".ToEntity(), rules, Budget));
        }

        /// <summary>
        /// Under no budget it proves only what insertion alone already showed. That is more than
        /// nothing — the graph folds a neutral element as it inserts, so <c>x + 0</c> and
        /// <c>x + 0 + 0</c> are one class before a rule has fired — and it is much less than what
        /// a rule would find.
        /// </summary>
        [Fact]
        public void UnderNoBudgetItProvesOnlyWhatInsertionAlreadyShowed()
        {
            var starved = new WorkBudget { Steps = 0, Time = TimeSpan.Zero };
            var rules = Saturation.RulesUpTo(RewriteRuleGrowth.Unknown);

            Assert.True(Saturation.ProvesEqual("x".ToEntity(), "x".ToEntity(), rules, starved));
            Assert.True(Saturation.ProvesEqual("x + 0".ToEntity(), "x + 0 + 0".ToEntity(), rules, starved));

            // Needs a rule to fire, so no budget means not proved.
            Assert.False(Saturation.ProvesEqual(
                "sin(x) ^ 2 + cos(x) ^ 2".ToEntity(), "1".ToEntity(), rules, starved));
        }

        /// <summary>
        /// A budget of nothing still answers, with the input — the convention every bounded
        /// computation here follows, and the reason a caller can hand this a small budget without
        /// having to catch anything.
        /// </summary>
        [Fact]
        public void AStarvedBudgetAnswersWithTheInput()
        {
            var starved = Transformation.CanonicalizationOverGraph(
                new WorkBudget { Steps = 0, Time = TimeSpan.Zero });
            var result = starved.Apply("x + y".ToEntity());

            Assert.True(result.Succeeded);
            Assert.Equal("x + y".ToEntity(), result.OutputOrInput);
        }

        /// <summary>
        /// The name says the ceiling, because that is the parameter that changes what it can do,
        /// and it says the rule pass on either side, because a reader of a derivation needs to see
        /// that those ran too.
        /// </summary>
        [Fact]
        public void ItSaysWhatItIsAndWhatItClaims()
        {
            Assert.Contains("canonical-over-graph[Rearranges]", Narrow.Name);
            Assert.Contains("canonical-over-graph[Unknown]", Wide.Name);
            Assert.Contains("rewrite[CanonicalOrderExact]", Narrow.Name);
            Assert.Equal(TransformationRelation.Equivalence, Narrow.Relation);
            Assert.Equal(Soundness.SoundUnderAssumptions, Narrow.Soundness);
        }

        [Fact]
        public void ItRefusesANullBudget()
            => Assert.Throws<ArgumentNullException>(
                () => Transformation.CanonicalizationOverGraph(null!));

        /// <summary>
        /// Only the widest ceiling admits a rule whose growth nobody judged, so firing one is a
        /// request a caller has to make on purpose rather than something a ceiling does quietly.
        /// </summary>
        [Fact]
        public void OnlyTheWidestCeilingAdmitsAnUnjudgedRule()
        {
            foreach (var widest in new[]
                     {
                         RewriteRuleGrowth.Collects, RewriteRuleGrowth.Rearranges,
                         RewriteRuleGrowth.Expands,
                     })
                Assert.DoesNotContain(Saturation.RulesUpTo(widest),
                    rule => rule.Growth is RewriteRuleGrowth.Unknown);

            Assert.Contains(Saturation.RulesUpTo(RewriteRuleGrowth.Unknown),
                rule => rule.Growth is RewriteRuleGrowth.Unknown);
        }

        /// <summary>Each ceiling admits everything the one below it does.</summary>
        [Fact]
        public void AWiderCeilingIsASuperset()
        {
            var ceilings = new[]
            {
                RewriteRuleGrowth.Collects, RewriteRuleGrowth.Rearranges,
                RewriteRuleGrowth.Expands, RewriteRuleGrowth.Unknown,
            }.Select(Saturation.RulesUpTo).ToList();

            for (var i = 1; i < ceilings.Count; i++)
            {
                Assert.All(ceilings[i - 1], rule => Assert.Contains(rule, ceilings[i]));
                Assert.True(ceilings[i].Count > ceilings[i - 1].Count,
                    $"ceiling {i} admitted nothing over ceiling {i - 1}, so it is not a knob");
            }
        }

        /// <summary>
        /// Where the rules actually are, asserted rather than described — the fact that decides
        /// what the growth ceiling is worth.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This used to say that the great majority are unjudged and to guard the share at under
        /// a quarter: 69 of 324 when it was written. The declarations of
        /// <a href="https://github.com/asc-community/AngouriMath/issues/825">#825</a> took it past
        /// half — 164 of 324 — so the old premise is spent rather than merely out of date, and
        /// the test is renamed instead of having its number loosened again.
        /// </para>
        /// <para>
        /// What is worth guarding now is that the ceiling is still doing something. It is the
        /// <c>Rearranges</c> one that the transformation catalogue actually uses, and if it ever
        /// admits nearly everything then <see cref="Saturation.RulesUpTo"/> has stopped being a
        /// dial and the caller who asks for the narrow set is paying for a distinction that is no
        /// longer there.
        /// </para>
        /// </remarks>
        [Fact]
        public void TheGrowthCeilingStillLeavesASubstantialPartOut()
        {
            var used = Saturation.RulesUpTo(RewriteRuleGrowth.Rearranges).Count;
            var judged = Saturation.RulesUpTo(RewriteRuleGrowth.Expands).Count;
            var all = Saturation.RulesUpTo(RewriteRuleGrowth.Unknown).Count;

            Assert.True(all > judged,
                $"every one of the {all} sound rules now has a judged growth, so the ceiling "
                + "excludes nothing at all and is no longer a dial");
            Assert.True(all - used > all / 4,
                $"the ceiling the catalogue uses admits {used} of {all} sound rules, leaving "
                + $"{all - used} out -- less than a quarter, so it has stopped being a dial");
        }

        #endregion
    }
}
