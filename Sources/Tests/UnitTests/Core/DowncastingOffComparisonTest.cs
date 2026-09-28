//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A comparison of two numbers with no imaginary part is decided whatever the downcasting
    /// setting. With it off, <c>cos(3)</c> evaluates to a complex number whose imaginary part is
    /// zero, and a comparison of it was <c>NaN</c>, which also made every condition an answer
    /// stated on a sign undecidable.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class DowncastingOffComparisonTest
    {
        [Theory]
        [InlineData("cos(3) >= 0", false)]
        [InlineData("cos(1) >= 0", true)]
        [InlineData("cos(1) > cos(3)", true)]
        [InlineData("sin(4) < 0", true)]
        [InlineData("sin(4) <= -1", false)]
        [InlineData("cos(3) < 1 and cos(1) > 0", true)]
        public void AComparisonOfRealValuesIsDecided(string comparison, bool expected)
        {
            foreach (var downcasting in new[] { true, false })
            {
                using var _ = MathS.Settings.DowncastingEnabled.Set(downcasting);
                Assert.Equal(expected ? Boolean.True : Boolean.False, MathS.FromString(comparison, useCache: false).Evaled);
            }
        }

        /// <summary>A comparison with a value off the real line is still not decided.</summary>
        [Theory]
        [InlineData("i >= 0")]
        [InlineData("(1 + i) > 1")]
        [InlineData("sqrt(-2) >= 0")]
        public void AComparisonOffTheRealLineIsNaN(string comparison)
        {
            foreach (var downcasting in new[] { true, false })
            {
                using var _ = MathS.Settings.DowncastingEnabled.Set(downcasting);
                Assert.Equal(MathS.NaN, MathS.FromString(comparison, useCache: false).Evaled);
            }
        }
    }
}
