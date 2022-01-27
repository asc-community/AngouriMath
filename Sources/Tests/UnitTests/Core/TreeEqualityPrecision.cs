//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using static AngouriMath.Entity.Number;
using Xunit;
using static AngouriMath.Entity.Set;
using AngouriMath.Extensions;
using System;


namespace AngouriMath.Tests.Core
{
    public class TreeEqualityPrecision
    {
        [Theory]
        [InlineData("a", "b", 2, false)]
        [InlineData("derivative(a, x, 2)", "derivative(a, x, 3)", 0.1, false)]
        [InlineData("derivative(a, x, 2)", "derivative(a, x, 3)", 3.1, true)]
        [InlineData("0.00127398573298572395", "0.002238949234823", 0.1, true)]
        [InlineData("0.0", "1.0", 0.5, false)]
        [InlineData("0.0", "1.0", 1.5, true)]
        [InlineData("-0.5", "0.5", 0.5, false)]
        [InlineData("-0.5", "0.5", 1.5, true)]
        [InlineData("1", "1.73618368124124124", 1d, true)]
        [InlineData("1.73618368124124124", "1", 1d, true)]
        [InlineData("100", "110", 11d, true)]
        [InlineData("a + 0.1", "a + 0.2", 0.3, true)]
        [InlineData("3 + a", "3.1 + a", 0.2, true)]
        [InlineData("3.1 + a", "3 + a", 0.2, true)]
        [InlineData("3.1 + a", "3 + a", 0.01, false)]
        [InlineData("3", "3.1", 0.2, true)]
        [InlineData("3.1", "3", 0.2, true)]
        [InlineData("3.132239408234", "3", 0.2, true)]
        [InlineData("3", "3.132239408234", 0.2, true)]
        [InlineData("3.132239408234", "3", 0.1, false)]
        [InlineData("3", "3.132239408234", 0.1, false)]
        [InlineData("1 + 2 + 3i + 4", "1.01 + 1.99 + 3.01i + 4", 0.1, true)]
        [InlineData("{ 1, 2, 3 }", "{ 1, 2, 3 }", 0.1, true)]
        [InlineData("{ 1, 2, 3 }", "{ 1, 2, 3, 4 }", 0.1, false)]
        [InlineData("{ 1, 2, 3 }", "{ 1, 2, 3.1 }", 0.1, true)]
        [InlineData("{ 1, 2, 3 }", "{ 1.99, 2.9, 0.955 }", 0.2, true)]
        [InlineData("{ 1, 2, 3 }", "{ 1.99, 2.9, 0.5 }", 0.2, false)]
        [InlineData("{ 1, 2, a, 3 }", "{ 1.99, a, 2.9, 0.95 }", 0.2, true)]
        [InlineData("{ 1, 2, a + 1, 3 }", "{ 1.99, a + 1.03, 2.9, 0.95 }", 0.2, true)]
        [InlineData("{ 1, 2, 0.32 + a + 1, 3 }", "{ 1.99, 0.3 + a + 1.03, 2.9, 0.95 }", 0.2, true)]
        [InlineData("a + { 1, 2, 0.32 + a + 1, 3 }", "a + { 1.99, 0.3 + a + 1.03, 2.9, 0.95 }", 0.2, true)]
        [InlineData("0.1 + a + { 1, 2, 0.32 + a + 1, 3 }", "0.2 + a + { 1.99, 0.3 + a + 1.03, 2.9, 0.95 }", 0.2, true)]
        [InlineData("{ 1, 2, b, 3 }", "{ 1.99, a, 2.9, 0.95 }", 0.2, false)]
        [InlineData("{ 1, 2, a + 1, 3 }", "{ 1.99, a + 1.33, 2.9, 0.95 }", 0.2, false)]
        [InlineData("{ 1, 2, 0.32 + a + 1, 3 }", "{ 1.99, 0.3 + a + 1.03, 2.9, 0.95 } + 1", 0.2, false)]
        [InlineData("a + { 1, 2, 0.32 + b + 1, 3 }", "a + { 1.99, 0.3 + a + 1.03, 2.9, 0.95 }", 0.2, false)]
        [InlineData("0.1 + a + { 1, 2, 0.32 + a + 1, 3 }", "0.2 + a + { 1.99, 0.3 + a + 1.03, 2.9, 0.55 }", 0.2, false)]
        public void ShouldBeEqual(string a, string b, double error, bool shouldBeEqual)
        {
            Entity e1 = a;
            Entity e2 = b;
            if (shouldBeEqual)
                Assert.True(e1.EqualsImprecisely(e2, error));
            else
                Assert.False(e1.EqualsImprecisely(e2, error));
        }
    }
}
