using AngouriMath;
using Xunit;

namespace UnitTests.Common
{
    public sealed class SubstituteTest
    {
        static readonly Entity.Variable x = nameof(x);
        [Fact]
        public void Test1()
        {
            var expr = x * x + MathS.Sin(x) * 0;
            Assert.True(expr.Substitute(x, 0).Simplify() == 0);
        }

        [Fact]
        public void Test2()
        {
            var y = MathS.Var("y");
            var expr = x.Pow(x) - MathS.Sqrt(x - 3) / x + MathS.Sin(x);
            var expected = (3 * y).Pow(3 * y) - MathS.Sqrt(3 * y - 3) / (3 * y) + MathS.Sin(3 * y);
            var actual = expr.Substitute(x, 3 * y);
            Assert.True(expected == actual);
        }

        [Fact]
        public void Test3()
        {
            var expr = x;
            Assert.True(expr.Substitute(x, 0) == 0);
        }
    }
}
