using AngouriMath;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Number;
using Xunit;
using System.Collections.Generic;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace UnitTests.Algebra
{
    public sealed class SolveInequality
    {
        [Theory]
        [InlineData("x > 0", "(0; +oo)")]
        [InlineData("x < 0", "(-oo; +oo)")]
        [InlineData("x > 0 and x < 0", "{}")]
        [InlineData("(x - 2)(x + 2) > 0", @"(-oo; -2) \/ (2; +oo)")]
        [InlineData("(x - 2)(x + 2) < 0", "(-2; 2)")]
        [InlineData("(x - 2)(x + 2) <= 0", "[-2; 2]")]
        [InlineData("(x - a)(x + a) <= 0", "[-a; a]")]
        public void Test(string initial, string expected)
        {
            Assert.Equal(expected.ToEntity(), initial.Solve("x").Simplify());
        }


        [Theory]
        [InlineData("x > a")]
        [InlineData("x < a")]
        [InlineData("x <= a")]
        [InlineData("x >= a")]
        [InlineData("(x + 1)(x + 2) >= a")]
        [InlineData("(x + 1)(x + 2) > a")]
        [InlineData("(x + 1)(x + 2) <= a")]
        [InlineData("(x + 1)(x + 2) < a")]
        [InlineData("(x + a)(x + b) >= 0")]
        [InlineData("(x + a)(x + b) > 0")]
        [InlineData("(x + a)(x + b) <= 0")]
        [InlineData("(x + a)(x + b) < 0")]
        public void AutoTest(string inequality, string setToCheck = "{ -5, -3, 0, 3, 5 }")
        {
            FiniteSet checkpoints = setToCheck;
            var roots = (Set)inequality.Solve("x").Substitute("a", -10).Substitute("b", 10).Simplify();
            foreach (var cp in checkpoints)
                Assert.True(roots.Contains(cp) == inequality.Substitute("x", cp).Substitute("a", -10).Substitute("b", 10).EvalBoolean());
        }
    }
}
