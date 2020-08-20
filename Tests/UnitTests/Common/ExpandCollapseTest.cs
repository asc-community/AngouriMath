using AngouriMath;
using Xunit;

namespace UnitTests.Common
{
    public class ExpandCollapseTest
    {
        public static readonly Entity.Variable x = MathS.Var(nameof(x));
        public static readonly Entity.Variable y = MathS.Var(nameof(y));
        [Fact]
        public void Test1()
        {
            var expr = (x + y) * (x - y);
            Assert.Equal(16, expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval());
        }
        [Fact]
        public void Test2()
        {
            var expr = (x + y + x + y) * (x - y + x - y);
            Assert.Equal(64, expr.Expand().Substitute(x, 5).Substitute(y, 3).Eval());
        }
        [Fact]
        public void Test3()
        {
            var expr = x * y + x;
            Assert.Equal(x * (1 + y), expr.Collapse());
        }
        [Fact]
        public void Factorial()
        {
            var expr = MathS.Factorial(x + 3) / MathS.Factorial(x + 1);
            Assert.Equal(MathS.Pow(x, 2) + x * 3 + (2 * x + 6), expr.Expand());
            expr = MathS.Factorial(x + -3) / MathS.Factorial(x + -1);
            Assert.Equal(1 / (x + -2) / (x + -1), expr.Expand());
        }
    }
}
