//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using Xunit;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// How <see cref="Real"/>'s comparison operators order <see cref="Real.NaN"/>, which is not
    /// how <see langword="double"/> does and is now written into their documentation.
    /// </summary>
    /// <remarks>
    /// Pinned because it is surprising and because the documentation asserts it. A reader
    /// arrives with the IEEE habit, where every comparison against NaN is false; here the
    /// comparison is a total order and NaN sits at the top of it.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RealComparisonTest
    {
        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(100000)]
        public void NaNIsAboveEveryNumber(int number)
        {
            Real value = number;
            Assert.True(Real.NaN > value);
            Assert.True(Real.NaN >= value);
            Assert.False(Real.NaN < value);
            Assert.False(Real.NaN <= value);

            Assert.True(value < Real.NaN);
            Assert.False(value > Real.NaN);
        }

        /// <summary>And the ordinary order is the ordinary one.</summary>
        [Fact]
        public void AndTheRestOrdersAsExpected()
        {
            Real one = 1, two = 2;
            Assert.True(two > one);
            Assert.True(one < two);
            Assert.True(one <= one);
            Assert.True(one >= one);
            Assert.False(one > one);
        }

        /// <summary>
        /// Division by zero answers <see cref="Real.NaN"/> rather than throwing, so a quotient
        /// always has an answer of some kind.
        /// </summary>
        [Fact]
        public void DivisionByZeroIsNaNRatherThanAThrow()
        {
            Real one = 1, zero = 0;
            Assert.True((one / zero).IsNaN);
        }
    }
}
