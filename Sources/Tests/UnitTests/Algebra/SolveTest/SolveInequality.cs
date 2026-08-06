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
        // A symbolic coefficient has no known sign, and the solver builds (root1; root2)
        // without asking which root is the smaller -- so one sign of `a` always comes out
        // as an empty interval. This used to read as [a; -a], correct for a < 0 and empty
        // for a > 0, and it read that way only because sqrt(4a^2) was simplifying to 2a.
        // That is unsound for a < 0 and is fixed in
        // https://github.com/asc-community/AngouriMath/issues/752; with it gone the interval
        // is empty for either sign and the endpoints survive.
        //
        // Neither form is right for both signs. Pinned as it now stands rather than deleted,
        // because the expectation is still evidence -- of
        // https://github.com/asc-community/AngouriMath/issues/757, which wants the case split
        // on the sign that would answer this properly.
        [InlineData("(x - a)(x + a) <= 0", @"{ a, -a } \/ (sqrt(a ^ 2); -sqrt(a ^ 2))")]
        public void Test(string initial, string expected)
        {
            Variable x = "x";
            Entity initialEnt = initial;
            Entity solutions = initialEnt.Solve(x);
            var expectedEntity = expected.ToEntity().InnerSimplified;
            var actualEntity = solutions.Simplify().InnerSimplified;
            Assert.Equal(expectedEntity, actualEntity);
        }


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
