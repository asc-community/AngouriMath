//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/1212, question I.2: an integral over
    /// whole bounds of an integrand with floor(x) in it is a sum of integrals over unit
    /// intervals, on each of which the floor is a number.
    /// </summary>
    public sealed class UnitIntervalIntegrationTest
    {
        [Fact]
        public void TheOrientationWeekIntegralIsHalfOfEMinusOne()
        {
            var value = "integral((x - floor(x)) / floor(x)!, x, 1, +oo)".ToEntity().Simplify();
            Assert.IsNotType<Integralf>(value);
            Assert.Equal("(e - 1) / 2".ToEntity().Simplify(), value);
        }

        [Theory]
        [InlineData("integral(floor(x), x, 0, 5)", "10")]
        [InlineData("integral(x - floor(x), x, 0, 3)", "3/2")]
        [InlineData("integral((x - floor(x))^2, x, 0, 4)", "4/3")]
        [InlineData("integral(floor(x) * x, x, 0, 3)", "13/2")]
        [InlineData("integral(floor(x) / (floor(x) + 1)!, x, 0, +oo)", "1")]
        public void AnIntegralOverWholeBoundsIsSummedOverUnitIntervals(string integral, string expected)
            => Assert.Equal(expected.ToEntity(), integral.ToEntity().Simplify());

        // The split is exact but the sum it leaves has no closed form here yet -- a geometric
        // series -- so the integral stays as written rather than becoming an unevaluated sum.
        [Fact]
        public void ASumWithoutAClosedFormLeavesTheIntegralAsWritten()
            => Assert.IsType<Integralf>("integral(2^(-floor(x)), x, 0, +oo)".ToEntity().Simplify());

        // A bound that is not whole contributes the piece up to the next whole number, on
        // which the step is one known constant.
        [Theory]
        [InlineData("integral(floor(x), x, 1/2, 2)", "1")]
        [InlineData("integral(x - floor(x), x, 0, 5/2)", "9/8")]
        [InlineData("integral(floor(x), x, 1/4, 3/4)", "0")]
        [InlineData("integral(x - floor(x), x, 3/2, 7/4)", "5/32")]
        [InlineData("integral(ceil(x), x, 0, 3)", "6")]
        [InlineData("integral(ceil(x) - x, x, 1/2, 2)", "5/8")]
        public void ABoundThatIsNotWholeContributesItsOwnPiece(string integral, string expected)
            => Assert.Equal(expected.ToEntity(), integral.ToEntity().Simplify());

        // A symbolic bound is declined rather than integrated as if the step were constant,
        // which used to answer (n - floor(n))^3 / 3 for this one; with the bound a number the
        // same integral splits.
        [Fact]
        public void ASymbolicBoundIsLeftAsWritten()
        {
            var value = "integral((x - floor(x))^2, x, 0, n)".ToEntity().Simplify();
            Assert.IsType<Integralf>(value);
            Assert.Equal("2".ToEntity(), value.Substitute("n", 6).Simplify());
        }

        // Not split and not integrated either: a floor of something other than the variable
        // is not a step this reads, and an integrand the unit interval cannot integrate stays.
        [Theory]
        [InlineData("integral(floor(x^2), x, 0, 2)")]
        [InlineData("integral(floor(x) * e^(e^x) , x, 0, 2)")]
        public void WhatCannotBeSplitIsLeftAsWritten(string integral)
            => Assert.IsType<Integralf>(integral.ToEntity().Simplify());

        [Fact]
        public void AnIntegralWithoutAFloorIsUnaffected()
            => Assert.Equal("ln(4/3) + ln(16/9) / 2".ToEntity(), "integral((2x^2+x+1)/(x^3+x^2+x+1), x, 3/4, 4/3)".ToEntity().Simplify());
    }
}
