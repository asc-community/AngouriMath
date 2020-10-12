using AngouriMath;
using Xunit;
using AngouriMath.Extensions;

namespace UnitTests.Algebra
{
    public sealed class IntegrationTest
    {
        // TODO: add more tests
        [Theory]
        [InlineData("2x", "x2")]
        [InlineData("x2", "x3 / 3")]
        [InlineData("x2 + x", "x3 / 3 + x2 / 2")]
        [InlineData("x2 - x", "x3 / 3 - x2 / 2")]
        public void TestIndefinite(string initial, string expected)
        {
            Assert.Equal(expected.ToEntity(), initial.Integrate("x").Simplify());
        }

        static readonly Entity.Variable x = nameof(x);
        [Fact]
        public void Test1()
        {
            var expr = x;
            Assert.True((MathS.Compute.DefiniteIntegral(expr, x, 0, 1).RealPart - 1.0/2).Abs() < 0.1);
        }
        [Fact]
        public void Test2()
        {
            var expr = MathS.Sin(x);
            Assert.Equal(0, MathS.Compute.DefiniteIntegral(expr, x, -1, 1));
        }
        [Fact]
        public void Test3()
        {
            var expr = MathS.Sin(x);
            Assert.True(MathS.Compute.DefiniteIntegral(expr, x, 0, 3).RealPart > 1.5);
        }
    }
}
