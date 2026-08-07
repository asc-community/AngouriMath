//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// The rewrite rules reached absorption and stopped there, so <c>a and b or a and not b</c>
    /// factored correctly to <c>a and (b or not b)</c> and had nothing to finish it -- there is
    /// no rule taking <c>b or not b</c> to <c>true</c>. Two-level minimisation by
    /// Quine-McCluskey covers that, excluded middle, non-contradiction and every larger cover
    /// in one procedure.
    /// https://github.com/asc-community/AngouriMath/issues/768
    ///
    /// It is offered to <c>Simplify</c> as one more candidate rather than as a replacement for
    /// its answer. Candidates are ranked by node count and the shortest is returned, so this
    /// can only change an expression's answer where it is shorter than everything else on
    /// offer -- which is also why it needs no separate entry point.
    /// </summary>
    [Trait("Area", "PatternsTest")]
    public sealed class BooleanMinimisationTest
    {
        private static void AssertSimplifiesTo(string input, string expected) =>
            Assert.Equal(expected.ToEntity().InnerSimplified,
                         input.ToEntity().Simplify().InnerSimplified);

        /// <summary>
        /// The keystones. Without excluded middle and non-contradiction the factoring that
        /// already worked could not finish, which is what made case 1 stop one rewrite short.
        /// </summary>
        [Theory]
        [InlineData("a or not a", "true")]
        [InlineData("a and not a", "false")]
        [InlineData("a and b or a and not b", "a")]
        [InlineData("(a or b) and (a or not b)", "a")]
        public void ExcludedMiddleAndNonContradictionAreReached(string input, string expected) =>
            AssertSimplifiesTo(input, expected);

        /// <summary>
        /// Covers over more than one variable, which no single rewrite rule reaches.
        /// </summary>
        [Theory]
        [InlineData("(a and b) or (a and not b) or (not a and b)", "a or b")]
        [InlineData("(a and b and c) or (a and b and not c)", "a and b")]
        [InlineData("(a and b and c and d) or (a and b and c and not d)", "a and b and c")]
        public void ALargerCoverIsFound(string input, string expected) =>
            AssertSimplifiesTo(input, expected);

        /// <summary>
        /// #769's report: <c>Simplify</c> returned an <c>implies</c> form at 12 nodes, which
        /// beat the 16-node input honestly -- the 4-node answer was simply never generated for
        /// it to lose to. Supplying it settles that without touching the rule that produced the
        /// <c>implies</c>, which is what a naive reading of that issue would have removed.
        /// https://github.com/asc-community/AngouriMath/issues/769
        /// </summary>
        [Theory]
        [InlineData("(not a and not b and not c) or (not a and not b and c)", "not (a or b)")]
        [InlineData("(not a and not b and c) or (not a and b and c)", "not a and c")]
        [InlineData("(not a and not b and not c) or (a and not b and not c)", "not (b or c)")]
        public void TheMinimalFormBeatsTheImpliesRewrite(string input, string expected) =>
            AssertSimplifiesTo(input, expected);

        /// <summary>
        /// Declaring the variable boolean must reach the same answer the bare expression does.
        /// The condition says nothing about which minterms hold, so it travels with the result
        /// rather than blocking it.
        /// </summary>
        [Theory]
        [InlineData("a or not a provided a in BB", "true provided a in BB")]
        [InlineData("a and b or a and not b provided a in BB", "a provided a in BB")]
        public void ADeclaredBooleanIsMinimisedToo(string input, string expected) =>
            AssertSimplifiesTo(input, expected);

        /// <summary>
        /// **Where the minimiser must not win.** Its output is a sum of products, and that is
        /// not always the shortest way to write a function: <c>not (a and b)</c> is 4 nodes
        /// against the SOP form <c>not a or not b</c> at 5. Offering it as a candidate rather
        /// than taking it is what keeps these as they are, without any rule deciding which
        /// shape is preferable.
        /// </summary>
        [Theory]
        [InlineData("not (a and b)")]
        [InlineData("not (a or b)")]
        [InlineData("a and b")]
        [InlineData("a or b")]
        [InlineData("a xor b")]
        [InlineData("a implies b")]
        public void AnExpressionAlreadyShorterThanItsSumOfProductsIsLeftAlone(string input) =>
            AssertSimplifiesTo(input, input);

        /// <summary>
        /// Nothing that is not a boolean expression may reach the truth table at all --
        /// substituting <see langword="true"/> for the variable of <c>x &gt; 1</c> is not a
        /// question this can ask. <c>e</c> is the case worth pinning: it is a
        /// <c>Variable</c> node that evaluates to a number, so it passes a check on node types
        /// and is then absent from <c>Vars</c>.
        /// </summary>
        [Theory]
        [InlineData("x > 1 or x <= 1")]
        [InlineData("a and (e or not e)")]
        [InlineData("x + 1 = 2 or x + 1 != 2")]
        public void ANonBooleanExpressionIsDeclined(string input)
        {
            var simplified = input.ToEntity().Simplify();
            Assert.NotEqual(Entity.Boolean.True, simplified);
            Assert.NotEqual(Entity.Boolean.False, simplified);
        }

        /// <summary>
        /// **The cost guard.** A sum of k products is at least 2k-1 nodes, so where that
        /// already exceeds the input no cover can be chosen and the search is wasted. Parity is
        /// the case: over ten variables it has 512 minterms, no two of which combine, so every
        /// one is a prime implicant and the minimal form is 512 terms long. Without this the
        /// search took 1.5 s to produce a candidate that loses to the input at 19 nodes; with
        /// it, 34 ms.
        /// </summary>
        [Fact]
        public void ParityIsNotSearchedAndTerminatesQuickly()
        {
            var parity = "a xor b xor c xor d xor f xor g xor h xor j xor k xor l".ToEntity();
            var task = System.Threading.Tasks.Task.Run(() => parity.Simplify());
            Assert.True(task.Wait(System.TimeSpan.FromSeconds(10)),
                "minimising a ten-variable parity expression should decline rather than search");
            Assert.Equal(parity.InnerSimplified, task.Result.InnerSimplified);
        }
    }
}
