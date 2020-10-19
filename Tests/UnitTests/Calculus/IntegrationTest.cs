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
        [InlineData("x2", "(1/3) * x3")]
        [InlineData("x2 + x", "(1/3) * x3 + (1/2) * x2")]
        [InlineData("x2 - x", "1/3 * x ^ 3 - 1/2 * x ^ 2")]
        [InlineData("a / x", "ln(x) * a")]
        
        [InlineData("x cos(x)", "cos(x) + sin(x) * x")]
        [InlineData("ln(x)", "x * (ln(x) - 1)")]
        [InlineData("log(a, x)", "x * (ln(x) - 1) / ln(a)")]
        [InlineData("e ^ x", "e ^ x")]
        [InlineData("a ^ x", "a ^ x / ln(a)")]
        public void TestIndefinite(string initial, string expected)
        {
            Assert.Equal(expected.ToEntity().InnerSimplified, initial.Integrate("x").Simplify());
        }

        [Theory(Skip = "Fix bug in integrals")]
        [InlineData("sin(x)cos(x)", "-1/2 cos(x)2")]
        public void TestIndefiniteSkip(string initial, string expected)
            => TestIndefinite(initial, expected);

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
