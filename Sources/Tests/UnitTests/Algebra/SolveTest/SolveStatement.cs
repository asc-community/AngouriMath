//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.Algebra.SolveTest
{
    public sealed class SolveStatement
    {
        [Theory]
        [InlineData("x2 = 3 and x > 0", "{ sqrt(3) }")]
        [InlineData("x4 = 3 and x in RR", "{ -3^(1/4), 3^(1/4) }")]
        public void TestStatementSolver(string statement, string expectedRaw)
        {
            var expr = MathS.FromString(statement);
            var expected = MathS.FromString(expectedRaw);
            var actual = expr.Solve("x");
            Assert.Equal(expected, actual);
        }
    }
}
