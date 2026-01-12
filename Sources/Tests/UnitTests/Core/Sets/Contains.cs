//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Xunit;
using static AngouriMath.Entity;
using AngouriMath.Extensions;

namespace AngouriMath.Tests.Core.Sets
{
    public sealed class Contains
    {
        [Theory]
        [InlineData("{}", "2", false)]
        [InlineData("{}", "a", false)]
        [InlineData("{}", "{}", false)]
        [InlineData("{ a }", "a", true)]
        [InlineData("{ a }", "{}", false, false)]
        [InlineData("{ 1, 2, 3 }", "1", true)]
        [InlineData("{ 1, 2, 3 }", "{ 1 }", false)]
        [InlineData("{ 1, 2, 3, 4 }", "3.5", false)]
        [InlineData("{ 1, 2, 3, x^3 }", "x^3", true)]
        [InlineData("{ 1, 2, 3, { x^3 } }", "3", true)]
        [InlineData("{ 1, 2, x, { x^3 } }", "x", true)]
        [InlineData("{ 1, 2, 3, { x^3 } }", "{ x^3 }", true)]
        [InlineData("{ a, b, 1 }", "a", true)]
        [InlineData("{ a, b, { 1 } }", "{ 1 }", true)]
        [InlineData("{ a, b, { 1, b } }", "{ 1, b }", true)]
        [InlineData("{ a, b, { 1, b } }", "2", false, false)]
        [InlineData("{ a, b, { 1, b } }", "x", false, false)]
        [InlineData("{ a, b, { 1, b } }", "x^3", false, false)]
        [InlineData("{ a, b, { 1, b } }", "2b", false, false)]
        [InlineData("{ 1, 2 }", "x^3", false, false)]
        [InlineData("{ 1, 2, { x } }", "x^3", false, false)]
        [InlineData("[0; 3)", "2", true)]
        [InlineData("[3; -3]", "0", false)]
        [InlineData("[2; 2]", "2", true)]
        [InlineData("[2; 2)", "2", false)]
        [InlineData("(2; 2)", "2", false)]
        [InlineData("(2; 2]", "2", false)]
        [InlineData("[2; 5)", "5", false)]
        [InlineData("[2; 5]", "5", true)]
        [InlineData("(2; 5)", "2", false)]
        [InlineData("[2; 5)", "2", true)]
        [InlineData("[2; a)", "a", false)]
        [InlineData("[2; a]", "a", true)]
        [InlineData("[b; a)", "b", true)]
        [InlineData("(b; a)", "b", false)]
        [InlineData("(b; a)", "x", false, false)]
        [InlineData("({1,2}; {3,4})", "x", false, false)]
        [InlineData("{ x : x > 0 }", "3", true)]
        [InlineData("{ x : x < 0 }", "3", false)]
        [InlineData("{ x : x = a }", "a", true)]
        [InlineData("{ x : x = a }", "b", false, false)]
        [InlineData("RR", "3", true)]
        [InlineData("RR", "-3", true)]
        [InlineData("RR", "-0.3243", true)]
        [InlineData("RR", "3 + i", false)]
        [InlineData("RR", "x", false, false)]
        [InlineData("CC", "3 + i", true)]
        [InlineData("BB", "true", true)]
        [InlineData("BB", "false", true)]
        [InlineData("BB", "3", false)]
        [InlineData("QQ", "3 / 4", true)]
        [InlineData("QQ", "3 / 4 + i", false)]
        [InlineData("ZZ", "3 / 4 + i", false)]
        [InlineData("ZZ", "3 / 4", false)]
        [InlineData("ZZ", "8", true)]
        [InlineData("ZZ", "-8", true)]
        [InlineData("ZZ \\ {-8}", "-8", false)]
        [InlineData("{x} /\\ {y}", "1", false, false)]
        [InlineData("{x} /\\ {y}", "z", false, false)]
        [InlineData("{x} /\\ {1}", "1", false, false)]
        [InlineData("{x} /\\ {1}", "x", false, false)]
        [InlineData("{1} /\\ {1}", "1", true)]
        [InlineData("{x} /\\ {x}", "x", true)]
        [InlineData("{1} \\ {y}", "1", false, false)]
        [InlineData("{1} \\ {y}", "z", false, false)]
        [InlineData("{1} \\ {1}", "1", false)]
        [InlineData("{1} \\ {1}", "x", false, false)] // can't determine x in {1}
        [InlineData("{x} \\ {x}", "1", false, false)] // can't determine 1 in {x}
        [InlineData("{x} \\ {x}", "x", false)]
        [InlineData("{x} \\ {y}", "1", false, false)]
        [InlineData("{x} \\ {y}", "z", false, false)]
        [InlineData("{x} \\ {1}", "1", false, false)]
        [InlineData("{x} \\ {1}", "x", false, false)]
        public void Test(string given, string expected, bool containsExpected, bool tryContainsExpected = true)
        {
            var actualRaw = given.ToEntity();
            var actual = Assert.IsAssignableFrom<Set>(actualRaw);
            var simplified = expected.ToEntity().InnerSimplified;
            Assert.Equal(tryContainsExpected, actual.TryContains(simplified, out var containsActual));
            Assert.Equal(containsExpected, containsActual);
        }

        [Theory]
        [InlineData("{ x : x = a and x > a }", "a", false)]
        public void TestSkip(string given, string expected, bool containsExpected)
            => Test(given, expected, containsExpected);
    }
}
