//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <see cref="MathS.UnsafeAndInternal.AreEqualNumerically(Entity, Entity)"/> compares the values at
    /// its check points to half the evaluation's digits, where it compared them exactly: two routes to
    /// one value differ in their last digits, and the derivative of <c>x ln(x)</c>'s antiderivative read
    /// as unequal to <c>x ln(x)</c> at -100 and 100.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1674">#1674</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class AreEqualNumericallyToThePrecisionTest
    {
        [Theory]
        [InlineData("2 * x * (-1/4 + ln(x)/2) + 1/x * 2/4 * x^2", "x * ln(x)")]
        [InlineData("sin(x)^2 + cos(x)^2", "1")]
        [InlineData("e^(2 * ln(x))", "x^2")]
        public void TwoRoutesToOneValueAreEqual(string one, string another)
            => Assert.True(MathS.UnsafeAndInternal.AreEqualNumerically(one.ToEntity(), another.ToEntity()));

        [Fact]
        public void AnAntiderivativeDifferentiatedBackIsItsIntegrand()
        {
            var integrand = "x * ln(x)".ToEntity();
            var back = integrand.Integrate("x").Substitute("C", 0).Differentiate("x");
            Assert.True(MathS.UnsafeAndInternal.AreEqualNumerically(back, integrand));
        }

        [Theory]
        // Past the tolerance, half of a hundred digits: still unequal.
        [InlineData("x", "x + 1/10^20")]
        [InlineData("x^2", "x^2 + 1/10^40")]
        public void ADifferenceAboveThePrecisionStillCounts(string one, string another)
            => Assert.False(MathS.UnsafeAndInternal.AreEqualNumerically(one.ToEntity(), another.ToEntity()));
    }
}
