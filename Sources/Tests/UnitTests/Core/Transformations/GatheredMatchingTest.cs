//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// The n-ary half of <a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a>:
    /// a rule about two terms finding them among many.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class GatheredMatchingTest
    {
        private static MatchPattern SquaredSine(string arg) =>
            MatchPattern.Node<Powf>(
                MatchPattern.Node<Sinf>(MatchPattern.Any(arg)),
                MatchPattern.Exact(Integer.Create(2)));

        private static MatchPattern SquaredCosine(string arg) =>
            MatchPattern.Node<Powf>(
                MatchPattern.Node<Cosf>(MatchPattern.Any(arg)),
                MatchPattern.Exact(Integer.Create(2)));

        private static MatchPattern Pythagoras =>
            MatchPattern.Gathered<Sumf>("rest", SquaredSine("x"), SquaredCosine("x"));

        /// <summary>
        /// The point of the whole exercise: the pair is found wherever it sits, including split
        /// apart by terms that have nothing to do with it.
        /// </summary>
        [Theory]
        [InlineData("sin(x)^2 + cos(x)^2")]
        [InlineData("a + sin(x)^2 + cos(x)^2")]
        [InlineData("sin(x)^2 + a + cos(x)^2")]
        [InlineData("sin(x)^2 + a + b + cos(x)^2")]
        [InlineData("a + sin(x)^2 + b + cos(x)^2 + c")]
        [InlineData("cos(x)^2 + sin(x)^2")]
        [InlineData("a + cos(x)^2 + b + sin(x)^2")]
        public void ThePairIsFoundWhereverItSits(string expression)
            => Assert.True(Pythagoras.Matches(expression.ToEntity()));

        /// <summary>And is not found where it is not there.</summary>
        [Theory]
        [InlineData("sin(x)^2 + sin(x)^2")]
        [InlineData("a + b + c")]
        [InlineData("sin(x)^2")]
        [InlineData("sin(x)^2 * cos(x)^2")]
        public void AndNotWhereItIsNot(string expression)
            => Assert.False(Pythagoras.Matches(expression.ToEntity()));

        /// <summary>
        /// A repeated hole unifies across operands, so the two terms must be about the same
        /// argument. Without this the rule would rewrite <c>sin(x)^2 + cos(y)^2</c> to 1.
        /// </summary>
        [Fact]
        public void TheTwoTermsMustShareAnArgument()
        {
            Assert.True(Pythagoras.Matches("sin(x)^2 + cos(x)^2 + q".ToEntity()));
            Assert.False(Pythagoras.Matches("sin(x)^2 + cos(y)^2 + q".ToEntity()));
        }

        /// <summary>
        /// The leftover is the operator's identity when every operand was claimed — the empty sum
        /// is zero — so a right-hand side never has to ask which case it is in.
        /// </summary>
        [Fact]
        public void AnExhaustedSumLeavesZeroBehind()
        {
            var solution = First("sin(x)^2 + cos(x)^2");
            Assert.NotNull(solution);
            Assert.Equal(Integer.Create(0), solution!["rest"]);
        }

        [Theory]
        [InlineData("sin(x)^2 + cos(x)^2 + a", "a")]
        [InlineData("a + sin(x)^2 + cos(x)^2", "a")]
        public void AndTheRestOfTheSumOtherwise(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), First(expression)!["rest"]);

        /// <summary>
        /// Operands come from <c>LinearChildren</c>, which has already turned subtraction into a
        /// negated term — so a rule needs no second arm for the subtractive spelling.
        /// </summary>
        [Fact]
        public void SubtractionIsTheSameChain()
        {
            // The chain is { sin(x)^2, cos(x)^2, (-1) * a }, so the pair is still there...
            Assert.True(Pythagoras.Matches("sin(x)^2 + cos(x)^2 - a".ToEntity()));
            Assert.Equal((-1 * MathS.Var("a")).InnerSimplified,
                First("sin(x)^2 + cos(x)^2 - a")!["rest"].InnerSimplified);

            // ...and a *subtracted* square is a different term, which the rule must not claim:
            // a - sin(x)^2 + cos(x)^2 offers (-1) * sin(x)^2, not sin(x)^2.
            Assert.False(Pythagoras.Matches("a - sin(x)^2 + cos(x)^2".ToEntity()));
        }

        /// <summary>Gathering works over a product too, where the identity left behind is one.</summary>
        [Fact]
        public void OverAProductTheIdentityIsOne()
        {
            var pattern = MatchPattern.Gathered<Mulf>(
                "rest", MatchPattern.Any<Variable>("p"), MatchPattern.Any<Variable>("q"));
            var solution = pattern.Match("a * b".ToEntity(), Bindings.Empty).First();
            Assert.Equal(Integer.Create(1), solution["rest"]);
        }

        /// <summary>
        /// A sum of distinct variables. The names are letters only on purpose: <c>v0</c> parses
        /// as <c>v ^ 0</c>, since a digit after an identifier is exponentiation in this grammar,
        /// so a generated name with a digit in it is not a variable at all.
        /// </summary>
        private static Entity Chain(int terms)
            => string.Join(" + ", Enumerable.Range(0, terms)
                .Select(i => $"{(char)('a' + i / 26)}{(char)('a' + i % 26)}")).ToEntity();

        private static MatchPattern FreeHoles(int count)
            => MatchPattern.Gathered<Sumf>("rest",
                Enumerable.Range(0, count)
                    .Select(i => MatchPattern.Any<Variable>($"h{i}")).ToArray());

        /// <summary>
        /// Every assignment of holes to operands is enumerated, which is what backtracking needs
        /// and what a matcher returning one answer cannot give.
        /// </summary>
        [Fact]
        public void EveryAssignmentIsEnumerated()
        {
            // Three distinct holes over eight terms: 8 * 7 * 6 ordered assignments.
            var solutions = FreeHoles(3).Match(Chain(8), Bindings.Empty).Count();
            Assert.Equal(8 * 7 * 6, solutions);
        }

        /// <summary>
        /// And the search is <b>bounded</b>, not exhaustive. Five holes over twenty terms is
        /// 20*19*18*17*16 = 1,860,480 assignments; the ceiling stops it long before that.
        /// </summary>
        /// <remarks>
        /// Observed as a count rather than as a duration, because a timing assertion is flaky in
        /// CI and this one is exact: past the ceiling the enumeration simply stops yielding.
        /// </remarks>
        [Fact]
        public void TheSearchIsBoundedRatherThanExhaustive()
        {
            var solutions = FreeHoles(5).Match(Chain(20), Bindings.Empty).Count();
            Assert.InRange(solutions, 1, MatchPattern.MaxAssignments);
            Assert.True(solutions < 20 * 19 * 18 * 17 * 16);
        }

        /// <summary>
        /// A chain that cannot match declines rather than running away — the bound is on the
        /// whole search, so no branch of it can spend forever. Declining is a legitimate answer:
        /// the rule does not apply, and nothing wrong is returned.
        /// </summary>
        [Fact]
        public void APathologicalChainDeclines()
        {
            // Four holes that match anything, and a fifth that matches nothing, over forty
            // terms. Every one of the 40*39*38*37 prefixes is built and then fails on the last
            // hole, so this is the case where the bound is what makes it return.
            var parts = Enumerable.Range(0, 4)
                .Select(i => MatchPattern.Any<Variable>($"h{i}"))
                .Append(SquaredSine("never"))
                .ToArray();
            Assert.False(MatchPattern.Gathered<Sumf>("rest", parts).Matches(Chain(40)));
        }

        /// <summary>The rule set built on it, end to end.</summary>
        [Theory]
        [InlineData("sin(x)^2 + cos(x)^2", "1")]
        [InlineData("a + sin(x)^2 + cos(x)^2", "1 + a")]
        [InlineData("sin(x)^2 + a + cos(x)^2", "1 + a")]
        public void TheRuleSetRewritesIt(string expression, string expected)
        {
            var rewritten = MatchedRules.PythagoreanIdentity.ApplyHere(expression.ToEntity());
            Assert.Equal(expected.ToEntity().InnerSimplified, rewritten.InnerSimplified);
        }

        /// <summary>
        /// The claim that motivates <c>Gathered</c>, checked rather than asserted: the binary
        /// pattern that the <c>switch</c> form corresponds to finds the pair only when the two
        /// terms are siblings.
        /// </summary>
        [Fact]
        public void TheBinaryPatternIsTheOneThatCannotDoThis()
        {
            var binary = MatchPattern.Commutative<Sumf>(SquaredSine("x"), SquaredCosine("x"));
            Assert.True(binary.Matches("sin(x)^2 + cos(x)^2".ToEntity()));
            Assert.False(binary.Matches("a + sin(x)^2 + cos(x)^2".ToEntity()));
            Assert.True(Pythagoras.Matches("a + sin(x)^2 + cos(x)^2".ToEntity()));
        }

        private static Bindings? First(string expression)
            => Pythagoras.Match(expression.ToEntity(), Bindings.Empty).FirstOrDefault();
    }
}
