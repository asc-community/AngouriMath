using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace UnitTests.Common
{
    public class InnerSimplifyTest
    {
        [Theory(Skip = "Moved to the 1.2.2 milestone, see issue here https://github.com/asc-community/AngouriMath/issues/263")]
        [InlineData("3 ^ 100")]
        [InlineData("(-3) ^ 100")]
        [InlineData("0.01 ^ 100")]
        public void ShouldNotChangeTest(string expr)
        {
            var expected = expr.ToEntity();
            var actual = expr.ToEntity().InnerSimplified;
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("a provided b", "a provided b")]
        [InlineData("a provided (b provided c)", "a provided c and b")]
        [InlineData("(a provided (b provided c)) + 1 + x + (y provided h)", "(a + 1 + x + y) provided (c and b and h)")]
        [InlineData("a provided 0 = 0", "a")]
        [InlineData("a provided 0 = 1", "NaN")]
        [InlineData("a provided b provided c provided d", "a provided d and (c and b)")]
        public void ShouldChangeTo(string from, string to)
        {
            var expected = to.ToEntity().Replace(c => c == "NaN" ? MathS.NaN : c);
            var actualInnerSimplified = from.ToEntity().InnerSimplified;
            var actualInnerEvaled = from.ToEntity().Evaled;
            Assert.Equal(expected, actualInnerSimplified);
            Assert.Equal(expected, actualInnerEvaled);
        }
    }
}
