using Xunit;
using AngouriMath.Extensions;

namespace UnitTests.Core.Sets
{
    public sealed class Arithmetics
    {
        [Theory]
        [InlineData("[2; 3] + 3", "[5; 6]")]
        [InlineData("3 + [2; 3]", "[5; 6]")]
        [InlineData("3 * [2; 3]", "3 * [2; 3]")]
        [InlineData("[2; 3] * 3", "[2; 3] * 3")]
        [InlineData("[2; 3] / 2", "[2; 3] / 2")]
        [InlineData("{ 1, 2, 3 } + 10", "{ 11, 12, 13 }")]
        [InlineData("10 + { 1, 2, 3 }", "{ 11, 12, 13 }")]
        [InlineData("{ 1, 2, 3 } * 10", "{ 10, 20, 30 }")]
        [InlineData("10 * { 1, 2, 3 }", "{ 10, 20, 30 }")]
        [InlineData("10 / { 1, 2, 5 }", "{ 10, 5, 2 }")]
        [InlineData("{ 1, 2, 5 } / 10", "{ 0.1, 0.2, 0.5 }")]
        [InlineData("{ 1, 2, 5 } ^ 2", "{ 1, 4, 25 }")]
        [InlineData("{ 1, 2, 5 }!", "{ 1, 2, 120 }")]
        public void TestSimplify(string unsimplified, string expected)
        {
            var exp = expected.ToEntity();
            var actUnsim = unsimplified.ToEntity();
            var act = actUnsim.InnerSimplified;
            Assert.Equal(exp, act);
        }
    }
}
