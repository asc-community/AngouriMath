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
    /// How <see cref="Real"/>'s comparisons treat <see cref="Real.NaN"/>: the operators refuse
    /// it, <see cref="Real.CompareTo(Real)"/> orders it, and the split is the point.
    /// </summary>
    /// <remarks>
    /// https://github.com/asc-community/AngouriMath/issues/947. Both halves are pinned because
    /// each is surprising on its own and only makes sense beside the other.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class RealComparisonTest
    {
        /// <summary>Every operator is false against NaN, in both directions, as with double.</summary>
        [Theory]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(100000)]
        public void NoOperatorHoldsAgainstNaN(int number)
        {
            Real value = number;
            Assert.False(Real.NaN > value);
            Assert.False(Real.NaN >= value);
            Assert.False(Real.NaN < value);
            Assert.False(Real.NaN <= value);

            Assert.False(value > Real.NaN);
            Assert.False(value >= Real.NaN);
            Assert.False(value < Real.NaN);
            Assert.False(value <= Real.NaN);
        }

        /// <summary>
        /// Including against itself: NaN is not greater than, less than, or equal-or-either to
        /// NaN. This is the row a reader is most likely to get wrong from habit.
        /// </summary>
        [Fact]
        public void NorAgainstItself()
        {
            Assert.False(Real.NaN > Real.NaN);
            Assert.False(Real.NaN >= Real.NaN);
            Assert.False(Real.NaN < Real.NaN);
            Assert.False(Real.NaN <= Real.NaN);
        }

        /// <summary>
        /// The guard that motivated the change: an undefined value must not read as exceeding a
        /// threshold, which is the least safe way for it to fail.
        /// </summary>
        [Fact]
        public void AnUndefinedValueDoesNotExceedAThreshold()
        {
            Real one = 1, zero = 0;
            var undefined = one / zero;
            Assert.True(undefined.IsNaN);
            Assert.False(undefined > (Real)100);
        }

        /// <summary>
        /// CompareTo still orders NaN, because sorting needs a total order and would otherwise
        /// loop or throw. This is the half that did *not* change.
        /// </summary>
        [Fact]
        public void CompareToStillTotallyOrdersSoSortingWorks()
        {
            Real one = 1, two = 2;
            Assert.True(Real.NaN.CompareTo(one) > 0);
            Assert.True(one.CompareTo(Real.NaN) < 0);
            Assert.Equal(0, Real.NaN.CompareTo(Real.NaN));

            var values = new[] { two, Real.NaN, one };
            System.Array.Sort(values);
            Assert.Equal(new[] { one, two, Real.NaN }, values);
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
