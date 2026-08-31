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
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// A pattern reaching inside a binder:
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1074">#1074</a>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bound name of a <see cref="Set.ConditionalSet"/> is not among its
    /// <see cref="Entity.DirectChildren"/>, and the body published there has it replaced by a
    /// placeholder invented per traversal — so a pattern could reach neither, and
    /// <c>{ x : x in S } = S</c> had to be written as code. <c>MatchPattern.Binder</c> reads the
    /// declared pair instead.
    /// </para>
    /// <para>
    /// Everything here compares entities rather than printed forms, and the placeholder is
    /// exactly the kind of thing a printed comparison would hide.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class BinderMatchingTest
    {
        private static MatchPattern MembershipInItsOwnSet =>
            MatchPattern.Binder<Set.ConditionalSet>(
                "v", MatchPattern.Node<Set.Inf>(MatchPattern.Any("v"), MatchPattern.Any("s")));

        /// <summary>
        /// The defect itself: a two-child pattern over a set builder used to match nothing,
        /// because the node offered one child.
        /// </summary>
        [Theory]
        [InlineData("{ x : x in [0; 1] }", "[0; 1]")]
        [InlineData("{ y : y in RR }", "RR")]
        [InlineData("{ k : k in {1, 2, 3} }", "{1, 2, 3}")]
        [InlineData("{ x : x in A \\/ B }", "A \\/ B")]
        public void ABinderPatternReachesTheBoundNameAndTheBody(string source, string supset)
        {
            var bindings = Bindings.Empty;
            var solutions = MembershipInItsOwnSet.Match(source.ToEntity(), bindings).ToList();

            var solution = Assert.Single(solutions);
            Assert.Equal(supset.ToEntity(), solution["s"]);
        }

        /// <summary>
        /// The repeated hole is the <c>when v1 == v1a</c> the <c>switch</c> wrote: a predicate
        /// about some <em>other</em> name is a different statement and must not match.
        /// </summary>
        [Theory]
        [InlineData("{ x : y in [0; 1] }")]
        [InlineData("{ x : x > 0 }")]
        [InlineData("{ x : x in [0; 1] and x > 0 }")]
        public void APredicateThatIsNotMembershipOfTheBoundNameDoesNotMatch(string source)
            => Assert.Empty(MembershipInItsOwnSet.Match(source.ToEntity(), Bindings.Empty));

        /// <summary>
        /// Alpha-invariance: the name position is a hole, so the pattern says <em>the same
        /// name</em> and never <em>which</em> name, and two set builders differing only in their
        /// bound name are matched alike.
        /// </summary>
        [Fact]
        public void TheBoundNameIsNotWhatIsMatchedOn()
        {
            var underX = MembershipInItsOwnSet.Match("{ x : x in [0; 1] }".ToEntity(), Bindings.Empty).ToList();
            var underQ = MembershipInItsOwnSet.Match("{ q : q in [0; 1] }".ToEntity(), Bindings.Empty).ToList();

            Assert.Single(underX);
            Assert.Single(underQ);
            Assert.Equal(underX[0]["s"], underQ[0]["s"]);
            // And the two sets are one expression to begin with, which is what the rename in
            // DirectChildren is for and what reading the declared pair must not undo.
            Assert.Equal("{ x : x in [0; 1] }".ToEntity(), "{ q : q in [0; 1] }".ToEntity());
        }

        /// <summary>
        /// A set builder offers two matchable children where it offers one direct child, and
        /// nothing but a pattern over its own type may have them. Otherwise the widening would
        /// hand every two-child pattern in the library a new node to match.
        /// </summary>
        /// <remarks>
        /// The node type is checked before matching on all three entry points —
        /// <c>Match</c>, <c>TryMatchOnce</c> and <c>TryMatchChoice</c> — so all three are asked
        /// here rather than just the one a rewrite pass happens to use.
        /// </remarks>
        [Fact]
        public void ATwoChildPatternOfAnotherTypeStillDoesNotMatchASetBuilder()
        {
            var set = "{ x : x in [0; 1] }".ToEntity();
            Assert.Single(set.DirectChildren);
            Assert.Equal(2, ((Entity)set).MatchableChildren.Count);

            foreach (var pattern in new[]
            {
                MatchPattern.Node<Sumf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                MatchPattern.Node<Set.Inf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
                MatchPattern.Commutative<Mulf>(MatchPattern.Any("a"), MatchPattern.Any("b")),
            })
            {
                Assert.Empty(pattern.Match(set, Bindings.Empty));
                Assert.False(pattern.TryMatchOnce(set, Bindings.Empty, out _));
                Assert.False(pattern.TryMatchChoice(set, Bindings.Empty, 0, out _));
            }
        }

        /// <summary>
        /// The rule that needed this, end to end, through the set it belongs to.
        /// </summary>
        [Theory]
        [InlineData("{ x : x in [0; 1] }", "[0; 1]")]
        [InlineData("{ x : x in RR }", "RR")]
        public void TheRuleFiresThroughItsSet(string source, string expected)
        {
            var rule = RewriteRules.SetOperator.Rules.Single(
                r => r.Name == "a-conditional-set-whose-condition-is-its-own-membership-is-that-set");
            Assert.Equal(expected.ToEntity(), rule.TryApply(source.ToEntity()));
        }

        /// <summary>
        /// <b>One node type needs the binder pattern, not six.</b> The issue expected every binder
        /// to hide its bound name; this is the measurement that says otherwise, and the reason
        /// there is one <see cref="Entity.MatchableChildren"/> override rather than a family of
        /// them.
        /// </summary>
        /// <remarks>
        /// A summation offers four children and the second is the index, a limit three and the
        /// second is the variable, and so on — un-renamed in every case, so an ordinary
        /// <c>Node&lt;T&gt;</c> already reaches them. Asserted as the shapes rather than as
        /// "they are fine", so that a binder that starts hiding its name fails here.
        /// </remarks>
        [Fact]
        public void EveryOtherBinderPublishesTheNameItBinds()
        {
            var hiding = new List<string>();
            foreach (var source in new[]
            {
                "lambda(x, x + 1)", "sum(x, x, 1, 3)", "product(x, x, 1, 3)",
                "integral(x, x)", "integral(x, x, 0, 1)", "derivative(x, x)", "limit(x, x, 0)",
            })
            {
                var binder = source.ToEntity();
                if (!binder.DirectChildren.Contains((Entity)MathS.Var("x")))
                    hiding.Add($"{source} offers [{string.Join(" | ", binder.DirectChildren)}]");
            }

            Assert.True(hiding.Count == 0,
                "these binders no longer publish the name they bind, so a pattern can no longer "
                + "name it and each wants a MatchableChildren override of its own:\n  "
                + string.Join("\n  ", hiding));

            // And the one that does hide it, stated the same way round.
            var set = "{ x : x in [0; 1] }".ToEntity();
            Assert.DoesNotContain((Entity)MathS.Var("x"), set.DirectChildren);
            Assert.Single(MembershipInItsOwnSet.Match(set, Bindings.Empty));
        }
    }
}
