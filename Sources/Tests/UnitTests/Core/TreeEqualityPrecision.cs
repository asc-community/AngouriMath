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
        [InlineData("0.00127398573298572395", "0.002238949234823")]
        public void ShouldBeEqual(string a, string b)
        {
            Entity e1 = a;
            Entity e2 = b;
            using var _ = MathS.Settings.PrecisionErrorCommon.Set(0.1m);
            Assert.True(e1.EqualsImprecisely(e2));
            Assert.Equal(e1, e2);
        }
    }
}
