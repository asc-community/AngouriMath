using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using static AngouriMath.MathS.Sets;
using AngouriMath.Extensions;

namespace UnitTests.Core.Sets
{
    public class Arithmetics
    {
        [Theory]
        [InlineData("[2; 3] + 3", "[5; 6]")]
        [InlineData("3 + [2; 3]", "[5; 6]")]
        [InlineData("3 * [2; 3]", "3 * [2; 3]")]
        [InlineData("[2; 3] * 3", "[2; 3] * 3")]
        [InlineData("[2; 3] / 2", "[2; 3] * 0.5")]
        [InlineData("{ 1, 2, 3 } + 10", "{ 11, 12, 13 }")]
        [InlineData("10 + { 1, 2, 3 }", "{ 11, 12, 13 }")]
        [InlineData("{ 1, 2, 3 } * 10", "{ 10, 20, 30 }")]
        [InlineData("10 * { 1, 2, 3 }", "{ 10, 20, 30 }")]
        [InlineData("10 / { 1, 2, 5 }", "{ 10, 5, 2 }")]
        [InlineData("{ 1, 2, 5 } / 10", "{ 0.1, 0.2, 0.5 }")]
        [InlineData("{ 1, 2, 5 } ^ 2", "{ 1, 4, 25 }")]
        [InlineData("{ 1, 2, 5 }!", "{ 1, 2, 120 }")]
        public void TestSimplify(string unsimplified, string expected)
        {
            Assert.Equal(expected.ToEntity(), unsimplified.Simplify());
        }
    }
}
