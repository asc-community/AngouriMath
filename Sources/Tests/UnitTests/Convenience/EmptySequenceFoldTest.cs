//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Convenience
{
    /// <summary>
    /// What a fold over an empty sequence answers.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1028">#1028</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A fold over a monoid has an identity, so the empty sum is <c>0</c> and the empty product
    /// is <c>1</c>. These threw <c>AngouriBugException</c> instead — an exception whose message
    /// asks the caller to report a bug against this repository, for a list their own
    /// <c>Where</c> happened to filter to nothing.
    /// </para>
    /// <para>
    /// The law is what pins it rather than the two constants: concatenation has to distribute
    /// over the fold, and it is the empty case that a wrong identity breaks.
    /// </para>
    /// </remarks>
    [Trait("Area", "Convenience")]
    public sealed class EmptySequenceFoldTest
    {
        [Fact]
        public void TheEmptySumIsZero()
            => Assert.Equal((Entity)0, Array.Empty<Entity>().SumAll());

        [Fact]
        public void TheEmptyProductIsOne()
            => Assert.Equal((Entity)1, Array.Empty<Entity>().MultiplyAll());

        /// <summary>The same two, reached through the operator classes rather than the extensions.</summary>
        [Fact]
        public void TheOperatorEntryPointsAgree()
        {
            Assert.Equal((Entity)0, Sumf.Sum(Array.Empty<Entity>()));
            Assert.Equal((Entity)1, Mulf.Multiply(Array.Empty<Entity>()));
        }

        /// <summary>
        /// The law the identity is for: splitting a sequence anywhere, including at either end,
        /// must not change what the fold gives.
        /// </summary>
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public void ConcatenationDistributesOverTheFold(int count)
        {
            var all = Enumerable.Range(1, count).Select(i => (Entity)i).ToArray();
            for (var split = 0; split <= count; split++)
            {
                var left = all.Take(split).ToArray();
                var right = all.Skip(split).ToArray();
                Assert.Equal(all.SumAll().Evaled, (left.SumAll() + right.SumAll()).Evaled);
                Assert.Equal(all.MultiplyAll().Evaled, (left.MultiplyAll() * right.MultiplyAll()).Evaled);
            }
        }

        /// <summary>
        /// A vector of no entries is refused, and refused from inside the documented hierarchy —
        /// it used to leak <see cref="IndexOutOfRangeException"/>, which a caller catching
        /// <c>AngouriMathBaseException</c> does not catch.
        /// </summary>
        [Fact]
        public void AnEmptyVectorIsRefusedFromInsideTheHierarchy()
        {
            Assert.Throws<InvalidMatrixOperationException>(() => MathS.Vector());
            Assert.Throws<InvalidMatrixOperationException>(() => Array.Empty<Entity>().ToVector());
            Assert.IsAssignableFrom<AngouriMathBaseException>(
                Record.Exception(() => MathS.Vector()));
        }
    }
}
