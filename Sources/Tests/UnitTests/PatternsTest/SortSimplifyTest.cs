//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    public sealed class SortSimplifyTest
    {
        [Theory]
        [InlineData("a + x + e + d + sin(x) + c + 1 + 2 + 2a", "3 * a")]
        [InlineData("x + a + b + c + arcsin(x2) + d + e + 1/2 - 23 * sqrt(3) + arccos(x * x)", "pi / 2")]
        [InlineData("x + a + b + c + arctan(x2) + d + e + 1/2 - 23 * sqrt(3) + arccot(x * x)", "pi / 2")]
        [InlineData("a / b + c + d + e + f + sin(x) + arcsin(x) + 1 + 0 - a * (b ^ -1)", "a / b", false)]
        // Skipped
        // [InlineData("sin(arcsin(c x) + arccos(x c) + c)2 + a + b + sin(x) + 0 + cos(c - -arcsin(c x) - -arccos(-c x * (-1)))2", "1")]
        // [InlineData("sin(arcsin(c x) + arccos(x c) + c)2 + a + b + sin(x) + 0 + cos(c - -arcsin(c x) - -arccos(-c x * (-1)))2", ") ^ 2", false)]
        [InlineData("sec(x) + a + sin(x) + c + 1 + 0 + 3 + sec(x)", "2 * sec(x)")]
        [InlineData("tan(x) * a * b / c / sin(h + 0.1) * cotan(x)", "tan", false)]
        [InlineData("sin(x) * a * b / c / sin(h + 0.1) * cosec(x)", "cosec", false)]
        [InlineData("cos(x) * a * b / c / sin(h + 0.1) * sec(x)", "cos", false)]
        [InlineData("sec(x) * a * b / c / sin(h + 0.1) * cos(x)", "sec", false)]
        [InlineData("cosec(x) * a * b / c / sin(h + 0.1) * sin(x)", "cosec", false)]
        public void TestStringIn(string exprRaw, string toBeIn, bool ifToBeIn = true)
        {
            var entity = exprRaw.ToEntity();
            var actual = entity.Simplify(5);
            if (ifToBeIn)
                Assert.Contains(toBeIn, actual.Stringize());
            else
                Assert.DoesNotContain(toBeIn, actual.Stringize());
        }
    }
}
