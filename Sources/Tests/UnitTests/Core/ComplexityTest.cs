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
    /// <see cref="Entity.Complexity"/> counts a shared subtree once for every place it is used,
    /// so an expression built by sharing is larger than <see cref="int.MaxValue"/> as a tree long
    /// before it is large in memory, and the count threw <see cref="System.OverflowException"/>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1600">#1600</a>
    /// </summary>
    /// <remarks>
    /// Nothing here enumerates the nodes: as a tree they are exponential in the number of
    /// squarings, which is the point.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class ComplexityTest
    {
        private static Entity Squared(int times)
        {
            Entity squared = "x";
            for (var i = 0; i < times; i++)
                squared = squared * squared;
            return squared;
        }

        /// <summary>
        /// Thirty-one squarings make <c>2^32 - 1</c> nodes, and the count of each operand,
        /// <c>2^31 - 1</c>, is already all an <c>int</c> holds.
        /// </summary>
        [Theory]
        [InlineData(31)]
        [InlineData(32)]
        [InlineData(60)]
        public void AnExpressionLargerThanAnIntAsATreeSaturates(int times)
            => Assert.Equal(int.MaxValue, Squared(times).Complexity);

        /// <summary>Below the bound the count is the count: <c>2^(n+1) - 1</c> nodes for <c>n</c> squarings.</summary>
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 3)]
        [InlineData(10, 2047)]
        [InlineData(29, 1073741823)]
        [InlineData(30, 2147483647)]
        public void BelowTheBoundItIsTheNumberOfNodes(int times, int nodes)
            => Assert.Equal(nodes, Squared(times).Complexity);
    }
}
