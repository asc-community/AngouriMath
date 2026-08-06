//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Tests.Algebra
{
    public sealed class SolveInequality
    {
        [Theory]
        [InlineData("x > 0", "(0; +oo)")]
        [InlineData("x < 0", "(-oo; 0)")]
        [InlineData("x > 0 and x < 0", "{}")]
        [InlineData("(x - 2)(x + 2) > 0", @"(-oo; -2) \/ (2; +oo)")]
        [InlineData("(x - 2)(x + 2) < 0", "(-2; 2)")]
        [InlineData("(x - 2)(x + 2) <= 0", "[-2; 2]")]
        // The symbolic coefficient is no longer pinned by its printed form here. It was, for
        // two releases, as evidence of https://github.com/asc-community/AngouriMath/issues/757
        // -- first as [a; -a] and then as the empty (sqrt(a^2); -sqrt(a^2)) -- and each of
        // those expectations recorded a wrong answer rather than a right one. It is now
        // checked for what it has to be true of instead, in
        // <see cref="ASymbolicCoefficientGivesAnIntervalOrderedForEitherSign"/>.
        public void Test(string initial, string expected)
        {
            Variable x = "x";
            Entity initialEnt = initial;
            Entity solutions = initialEnt.Solve(x);
            var expectedEntity = expected.ToEntity().InnerSimplified;
            var actualEntity = solutions.Simplify().InnerSimplified;
            Assert.Equal(expectedEntity, actualEntity);
        }

        /// <summary>
        /// A symbolic coefficient has no known sign, and the roots come out of the quadratic
        /// formula as <c>(-b - sqrt(D))/(2a)</c> before <c>(-b + sqrt(D))/(2a)</c> -- ascending
        /// only while <c>a</c> is positive. Solving <c>expr &lt;= 0</c> negates the expression,
        /// so it arrives with a negative leading coefficient and the pair the wrong way round,
        /// and <c>(x - a)(x + a) &lt;= 0</c> was answered with an interval running from
        /// <c>|a|</c> down to <c>-|a|</c>: empty, with the entire solution set lost.
        /// <para/>
        /// The endpoints are now ordered by the closed form of min and max rather than by a
        /// comparison that cannot be made, so the one interval is right for either sign.
        /// Checked by what it specialises to, not by how it prints -- the printed form is
        /// carrying an <c>abs</c> the simplifier keeps, since <c>abs(2 * sqrt(a^2))</c> is not
        /// <c>2 * sqrt(a^2)</c> unless <c>a</c> is known real.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/757">#757</a>
        /// </summary>
        /// <summary>
        /// Checked by which points the answer holds rather than by how it prints:
        /// <c>1/2 * abs(X)</c> and <c>abs(X) / 2</c> are one number written two ways, and a
        /// structural comparison calls that a disagreement.
        /// </summary>
        private static void AssertSolvesLike(string inequality, string value)
        {
            Variable x = "x";
            var symbolic = (Set)((Entity)inequality).Solve(x).Substitute("a", value.ToEntity()).Simplify();
            var truth = ((Entity)inequality).Substitute("a", value.ToEntity());
            foreach (var point in new Entity[] { -7, -3, -2, -1, "-1/2".ToEntity(), 0,
                                                 "1/2".ToEntity(), 1, 2, 3, 7 })
                Assert.True(symbolic.Contains(point) == truth.Substitute(x, point).EvalBoolean(),
                    $"solving {inequality} symbolically and then setting a = {value} gives "
                    + $"{symbolic}, which disagrees with the inequality itself at x = {point}");
        }

        [Theory]
        [InlineData("(x - a)(x + a) <= 0", "3")]
        [InlineData("(x - a)(x + a) <= 0", "-3")]
        [InlineData("(x - a)(x + a) <= 0", "1/2")]
        [InlineData("(x - a)(x + a) <= 0", "-1/2")]
        [InlineData("(x - a)(x + a) < 0", "3")]
        [InlineData("(x - a)(x + a) < 0", "-3")]
        [InlineData("(x - a)(x + a) > 0", "3")]
        [InlineData("(x - a)(x + a) > 0", "-3")]
        [InlineData("(x - a)(x + a) >= 0", "3")]
        [InlineData("(x - a)(x + a) >= 0", "-3")]
        public void ASymbolicCoefficientGivesAnIntervalOrderedForEitherSign(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// The roots need not be symmetric about zero, and need not both be symbolic, for the
        /// ordering to be undecidable -- <c>(x - 1)(x - a)</c> turns on whether <c>a</c> is
        /// above or below 1.
        /// </summary>
        [Theory]
        [InlineData("(x - a)(x - 2*a) <= 0", "3")]
        [InlineData("(x - a)(x - 2*a) <= 0", "-3")]
        [InlineData("(x - a)(x - 2*a) < 0", "3")]
        [InlineData("(x - a)(x - 2*a) < 0", "-3")]
        [InlineData("(x - 1)(x - a) <= 0", "3")]
        [InlineData("(x - 1)(x - a) <= 0", "-3")]
        [InlineData("(x - 1)(x - a) <= 0", "1/2")]
        [InlineData("(x - 1)(x - a) < 0", "3")]
        [InlineData("(x - 1)(x - a) < 0", "-3")]
        [InlineData("x^2 - a^2 <= 0", "3")]
        [InlineData("x^2 - a^2 <= 0", "-3")]
        [InlineData("x^2 - a^2 < 0", "3")]
        [InlineData("x^2 - a^2 < 0", "-3")]
        public void AsymmetricAndOneSidedRootsAreOrderedToo(string inequality, string value)
            => AssertSolvesLike(inequality, value);

        /// <summary>
        /// A double root, where the two endpoints coincide and the span between them is empty.
        /// The ordering arithmetic collapses to a point, which is what these have to be:
        /// <c>(x - a)^2 &lt;= 0</c> holds only at <c>a</c>, and <c>&lt; 0</c> nowhere. Before,
        /// each came back as an interval built from an unevaluated discriminant.
        /// </summary>
        [Theory]
        [InlineData("(x - a)^2 <= 0", "3")]
        [InlineData("(x - a)^2 <= 0", "-3")]
        [InlineData("(x - a)^2 < 0", "3")]
        [InlineData("(x - a)^2 > 0", "3")]
        [InlineData("(x - a)^2 >= 0", "3")]
        [InlineData("x^2 - 2*a*x + a^2 < 0", "3")]
        [InlineData("x^2 - 2*a*x + a^2 > 0", "-3")]
        public void ADoubleRootCollapsesToThePoint(string inequality, string value)
            => AssertSolvesLike(inequality, value);


        [Theory]
        [InlineData("x > a")]
        [InlineData("x < a")]
        [InlineData("x <= a")]
        [InlineData("x >= a")]
        [InlineData("(x + a)(x + b) >= 0")]
        [InlineData("(x + a)(x + b) > 0")]
        public void AutoTest(string inequality, string setToCheck = "{ -5, -3, 0, 3, 5 }")
        {
            FiniteSet checkpoints = (FiniteSet)setToCheck.Simplify();
            var roots = (Set)inequality.Solve("x").Substitute("a", -10).Substitute("b", 10).Simplify();
            foreach (var cp in checkpoints)
                Assert.True(roots.Contains(cp) == inequality.Substitute("x", cp).Substitute("a", -10).Substitute("b", 10).EvalBoolean(), 
                    $"{roots} doesn't contain {cp}");
        }

        [Theory(Skip = "Piecewise required")]
        [InlineData("(x + 1)(x + 2) < a")]
        [InlineData("(x + 1)(x + 2) <= a")]
        [InlineData("(x + 1)(x + 2) > a")]
        [InlineData("(x + 1)(x + 2) >= a")]
        [InlineData("(x + a)(x + b) <= 0")]
        [InlineData("(x + a)(x + b) < 0")]
        public void AutoTestSkip(string inequality, string setToCheck = "{ -5, -3, 0, 3, 5 }")
            => AutoTest(inequality, setToCheck);
    }
}
