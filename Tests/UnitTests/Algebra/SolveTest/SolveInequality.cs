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
    }
}
