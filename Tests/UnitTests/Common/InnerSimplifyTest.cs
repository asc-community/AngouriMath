using AngouriMath.Extensions;
using Xunit;

namespace UnitTests.Common
{
    public class InnerSimplifyTest
    {
        [Theory]
        [InlineData("3 ^ 100")]
        [InlineData("(-3) ^ 100")]
        [InlineData("0.01 ^ 100")]
        public void ShouldNotChangeTest(string expr)
        {
            var expected = expr.ToEntity();
            var actual = expr.ToEntity().InnerSimplified;
            Assert.Equal(expected, actual);
        }
    }
}
