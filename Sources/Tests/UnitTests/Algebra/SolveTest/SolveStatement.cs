using AngouriMath;
using Xunit;

namespace UnitTests.Algebra.SolveTest
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
