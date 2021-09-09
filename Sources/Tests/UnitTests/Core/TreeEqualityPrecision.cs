//
// Copyright (c) 2019-2021 Angouri.
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


namespace UnitTests.Core
{
    public class TreeEqualityPrecision
    {
        [Theory]
        [InlineData("0.00127398573298572395", "0.002238949234823", 0.1, true)]
        [InlineData("0.0", "1.0", 0.5, false)]
        [InlineData("0.0", "1.0", 1.5, true)]
        [InlineData("-0.5", "0.5", 0.5, false)]
        [InlineData("-0.5", "0.5", 1.5, true)]
        [InlineData("1", "1.73618368124124124", 1d, true)]
        [InlineData("100", "110", 11d, true)]
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
