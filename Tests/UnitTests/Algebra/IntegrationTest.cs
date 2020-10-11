using AngouriMath;
using Xunit;

namespace UnitTests.Algebra
{
    public sealed class IntegrationTest
    {
        static readonly Entity.Variable x = nameof(x);
        [Fact]
        public void Test1()
        {
            var expr = x;
            Assert.True((expr.DefiniteIntegral(x, 0, 1).RealPart - 1.0/2).Abs() < 0.1);
        }
        [Fact]
        public void Test2()
        {
            var expr = MathS.Sin(x);
            Assert.Equal(0, expr.DefiniteIntegral(x, -1, 1));
        }
        [Fact]
        public void Test3()
        {
            var expr = MathS.Sin(x);
            Assert.True(expr.DefiniteIntegral(x, 0, 3).RealPart > 1.5);
        }
    }
}
