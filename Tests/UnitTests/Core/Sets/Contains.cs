using AngouriMath;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;
using static AngouriMath.MathS;
using static AngouriMath.MathS.Sets;
using AngouriMath.Extensions;

namespace UnitTests.Core.Sets
{
    public class Contains
    {
        [Theory]
        [InlineData("{}", "a", false)]
        [InlineData("{ a }", "a", true)]
        [InlineData("{ 1, 2, 3 }", "1", true)]
        [InlineData("{ 1, 2, 3 }", "{ 1 }", false)]
        [InlineData("{ 1, 2, 3, 4 }", "3.5", false)]
        [InlineData("{ a, b, 1 }", "a", true)]
        [InlineData("{ a, b, { 1 } }", "{ 1 }", true)]
        [InlineData("{ a, b, { 1, b } }", "{ 1, b }", true)]
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
        [InlineData("{ x : x > 0 }", "3", true)]
        [InlineData("{ x : x < 0 }", "3", false)]
        [InlineData("{ x : x = a }", "a", false)]
        [InlineData("{ x : x = a and x > a }", "a", false)]
        [InlineData("RR", "3", true)]
        [InlineData("RR", "-3", true)]
        [InlineData("RR", "-0.3243", true)]
        [InlineData("RR", "3 + i", false)]
        [InlineData("CC", "3 + i", false)]
        [InlineData("BB", "true", true)]
        [InlineData("BB", "false", true)]
        [InlineData("BB", "3", false)]
        [InlineData("QQ", "3 / 4", true)]
        [InlineData("QQ", "3 / 4 + i", false)]
        [InlineData("ZZ", "3 / 4 + i", false)]
        [InlineData("ZZ", "3 / 4", false)]
        [InlineData("ZZ", "8", false)]
        [InlineData("ZZ", "-8", false)]
        public void Test(string given, string expected, bool containsExpected)
        {
            var actualRaw = given.ToEntity();
            var actual = Assert.IsAssignableFrom<Set>(actualRaw);
            Assert.True(actual.TryContains(expected.ToEntity().InnerSimplified, out var containsActual));
            Assert.Equal(containsExpected, containsActual);
        }
    }
}
