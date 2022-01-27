//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    public sealed class FractionSimplify
    {
        [Theory]
        [InlineData("(a - 1) / (1 - b) - (1 - a) / (b - 1)", "0")] // #254
        [InlineData("(4a - 2) / (2x) + (1 - 2a) / x", "0")]
        [InlineData("sin(a) * a * b / a", "sin(a) * b")] // #311
        [InlineData("sin(a) * cos(b) * tan(c) / (tan(c)3 * sin(a)2 * cos(b)^(-2))", "csc(a) * cos(b) ^ 3 * cotan(c) ^ 2")]
        public void TestSimplify(string testeeRaw, string expectedRaw)
        {
            Entity expected = expectedRaw;
            Entity testee = testeeRaw;
            var actual = testee.Simplify();
            Assert.Equal(expected, actual);
        }
    }
}
