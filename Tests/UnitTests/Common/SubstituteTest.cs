using AngouriMath.Extensions;
using Xunit;

namespace UnitTests.Common
{
    public sealed class SubstituteTest
    {
        [Theory]
        [InlineData("x + 2", "1 + 2")]
        [InlineData("x2", "1 ^ 2")]
        [InlineData("sqrt(x)", "sqrt(1)")]
        [InlineData("x", "1")]
        [InlineData("sin(x)", "sin(1)")]
        [InlineData("{ x : x > 3 }", "{ x : x > 3 }")]
        [InlineData("x + { x : x > 3 }", "1 + { x : x > 3 }")]
        [InlineData("x * derivative(x + 2, x, 1)", "1 * derivative(x + 2, x, 1)")]
        [InlineData("x * integral(x + 2, x, 1)", "1 * integral(x + 2, x, 1)")]
        [InlineData("x * limit(x + 2, x, 1)", "1 * limit(x + 2, x, 1)")]
        public void Test(string unsubstituted, string expectedRaw)
        {
            var actual = unsubstituted.Substitute("x", 1);
            var expected = expectedRaw.ToEntity();
            Assert.Equal(expected, actual);
        }
    }
}
