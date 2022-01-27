//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Discrete
{
    public sealed class EqualityInequalityEval
    {
        [Theory]
        [InlineData("5 > 3")]
        [InlineData("3 >= 3")]
        [InlineData("3 = 3")]
        [InlineData("3 < 0 or 3 > 0")]
        [InlineData("4.1 > 4")]
        [InlineData("4 < 4.1")]
        [InlineData("4 <= 4.1")]
        [InlineData("4 <= 4")]
        [InlineData("not 3 = 4")]
        [InlineData("3 > 2 and 5 < 6 and 1 >= 1 and 2 <= 2")]
        [InlineData("(true and false) = (false and true)")]
        [InlineData("true = true")]
        [InlineData("false = false")]
        [InlineData("(not (true and false)) = (not true or not false)")]
        public void IsTrue(string expr)
            => Assert.True(expr.EvalBoolean());

        [Theory]
        [InlineData("5 <= 3")]
        [InlineData("3 < 3")]
        [InlineData("3 = 4")]
        [InlineData("3 < 0 and 3 > 0")]
        [InlineData("4.1 <= 4")]
        [InlineData("4 > 4.1")]
        [InlineData("4 >= 4.1")]
        [InlineData("4 < 4")]
        [InlineData("3 < 2 or 5 > 6 or 1 < 1 or 2 > 2")]
        [InlineData("true = false")]
        [InlineData("false = true")]
        public void IsFalse(string expr)
            => Assert.False(expr.EvalBoolean());

        [Theory]
        [InlineData("3 + i > 3")]
        [InlineData("sqrt(-1) < 3")]
        [InlineData("i < 3")]
        [InlineData("i < i")]
        [InlineData("i > i")]
        [InlineData("i <= i")]
        [InlineData("i >= i")]
        [InlineData("1 < i")]
        [InlineData("1 > i")]
        [InlineData("1 <= i")]
        [InlineData("1 >= i")]
        public void IsNaN(string expr)
        {
            var ent = expr.ToEntity();
            Assert.Equal(MathS.NaN, ent.Evaled);
        }
    }
}
