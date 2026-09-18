//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A conjunction does not repeat a conjunct it already has, and one that contradicts another
    /// makes the whole conjunction <c>False</c>. What this is for: a piecewise combined with a
    /// piecewise joins every case's predicate to every other's, and two piecewises that split on
    /// the same three signs of one quantity must combine to three cases and not nine — left as
    /// nine, a sum of such piecewises had 3^12 cases and took 8 GB.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1414">#1414</a>
    /// </summary>
    public sealed class ContradictoryConjunctionTest
    {
        [Theory]
        [InlineData("x > 0 and x > 0", "x > 0")]
        [InlineData("(x > 0 and y > 0) and x > 0", "x > 0 and y > 0")]
        [InlineData("x > 0 and (y > 0 and x > 0)", "x > 0 and y > 0")]
        [InlineData("(a = 0 and not b = 0) and (a = 0 and not b = 0)", "a = 0 and not b = 0")]
        public void ARepeatedConjunctIsWrittenOnce(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().InnerSimplified);

        [Theory]
        [InlineData("q = 0 and q > 0")]
        [InlineData("q > 0 and q = 0")]
        [InlineData("q = 0 and q < 0")]
        [InlineData("q = 0 and not q = 0")]
        [InlineData("not q = 0 and q = 0")]
        [InlineData("(4 * c / a^2 = 0 and not a = 0) and (4 * c / a^2 > 0 and not a = 0)")]
        [InlineData("x = 0 and (y > 0 and (z > 0 and x < 0))")]
        public void AnEqualityAgainstAStrictSignOfTheSameQuantityIsFalse(string input)
            => Assert.Equal(Boolean.False, input.ToEntity().InnerSimplified);

        // False everywhere, not only over the reals: at q = i the equality is False, and
        // `False and anything` is False.
        [Theory]
        [InlineData("q = 0 and q > 0")]
        [InlineData("q = 0 and q < 0")]
        public void ThatIsSoundOffTheRealLine(string input)
            => Assert.Equal(Boolean.False, input.ToEntity().Substitute("q", "i").Evaled);

        // Two order comparisons are NaN off the real line, so deciding them is Simplify's, under
        // `q in RR` (https://github.com/asc-community/AngouriMath/issues/876); evaluation leaves
        // them, and compatible signs or signs of different quantities are left as written too.
        [Theory]
        [InlineData("q > 0 and q < 0")]
        [InlineData("q > 0 and q <= 0")]
        [InlineData("q < 0 and q >= 0")]
        [InlineData("not (q > 0) and q > 0")]
        [InlineData("q > 0 and q >= 0")]
        [InlineData("q >= 0 and q <= 0")]
        [InlineData("q = 0 and q >= 0")]
        [InlineData("q > 0 and p < 0")]
        [InlineData("q > 0 and q > 1")]
        [InlineData("not q = 0 and q > 0")]
        public void NothingElseIsDecided(string input)
            => Assert.Equal(input.ToEntity(), input.ToEntity().InnerSimplified);

        private static int CaseCount(Entity piecewise)
            => Assert.IsType<Piecewise>(piecewise).Cases.Count();

        [Fact]
        public void TwoPiecewisesOnTheSameSplitCombineCaseByCase()
        {
            // Three matching pairs, and the two pairs of opposite strict signs, `q > 0 and q < 0`
            // and `q < 0 and q > 0`, which are NaN rather than False off the real line and so stay
            // (five cases, not nine). Everything else pairs an equality against a sign and is gone.
            var sum = "piecewise(1 provided q = 0, 2 provided q > 0, 3 provided q < 0) + piecewise(10 provided q = 0, 20 provided q > 0, 30 provided q < 0)".ToEntity().InnerSimplified;
            Assert.Equal(5, CaseCount(sum));
            Assert.Equal(Number.Integer.Create(11), sum.Substitute("q", 0).Evaled);
            Assert.Equal(Number.Integer.Create(22), sum.Substitute("q", 1).Evaled);
            Assert.Equal(Number.Integer.Create(33), sum.Substitute("q", -1).Evaled);

            // And a chain of them stays at five cases rather than growing by a factor of three each
            // time: the two contradictory cases absorb every further sign rather than multiplying.
            var chain = sum;
            for (var i = 0; i < 6; i++)
                chain = (chain + "piecewise(1 provided q = 0, 2 provided q > 0, 3 provided q < 0)".ToEntity()).InnerSimplified;
            Assert.Equal(5, CaseCount(chain));
            Assert.Equal(Number.Integer.Create(22 + 12), chain.Substitute("q", 1).Evaled);
            Assert.Equal(Number.Integer.Create(33 + 18), chain.Substitute("q", -1).Evaled);
            Assert.Equal(Number.Integer.Create(11 + 6), chain.Substitute("q", 0).Evaled);
        }

        [Fact]
        public void TwoPiecewisesOnDifferentSplitsStillCombineEveryPair()
        {
            var sum = "piecewise(1 provided p > 0, 2 provided p <= 0) * piecewise(10 provided q > 0, 20 provided q <= 0)".ToEntity().InnerSimplified;
            Assert.Equal(4, CaseCount(sum));
            Assert.Equal(Number.Integer.Create(40), sum.Substitute("p", -1).Substitute("q", -1).Evaled);
            Assert.Equal(Number.Integer.Create(10), sum.Substitute("p", 1).Substitute("q", 1).Evaled);
        }
    }
}
