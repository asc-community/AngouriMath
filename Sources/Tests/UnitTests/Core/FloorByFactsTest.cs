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

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The floor and the ceiling as the quantifiers around them establish their argument: whole,
    /// real and not whole, or shifted by a whole number. Sullivan and Mackey's Prob 1.5.6 asks
    /// for each of its expressions "depending on x", which is a statement for every whole
    /// <c>x</c> and one for every other real <c>x</c>.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class FloorByFactsTest
    {
        [Theory]
        // Prob 1.5.6, items 1, 2, 3, 5 and 6, for a whole x and for any other real one.
        [InlineData("forall x in ZZ : floor(x) + floor(1 - x) = 1")]
        [InlineData("forall x in RR : not x in ZZ implies floor(x) + floor(1 - x) = 0")]
        [InlineData("forall x in RR \\ ZZ : floor(x) + floor(1 - x) = 0")]
        [InlineData("forall x in ZZ : ceil(x) + ceil(1 - x) = 1")]
        [InlineData("forall x in RR : not x in ZZ implies ceil(x) + ceil(1 - x) = 2")]
        [InlineData("forall x in ZZ : floor(x) + ceil(x) = 2 x")]
        [InlineData("forall x in RR : not x in ZZ implies floor(x) + ceil(x) = 2 floor(x) + 1")]
        // The cases as a disjunction: P or not Q, with Q about x alone, is Q implies P.
        [InlineData("forall x in RR : floor(x) + ceil(x) = 2 x or not x in ZZ")]
        [InlineData("forall x in RR : not x in ZZ or floor(x) + floor(1 - x) = 1")]
        [InlineData("forall x in ZZ : floor(x^2) - floor(x)^2 = 0")]
        [InlineData("forall x in ZZ : ceil(x^2) - ceil(x)^2 = 0")]
        // A whole term comes out of either, and a negation turns one into the other.
        [InlineData("forall n in ZZ : forall x in RR : floor(x + n) = floor(x) + n")]
        [InlineData("forall x in RR : floor(-x) = -ceil(x)")]
        [InlineData("forall x in CC : ceil(3 - x) = 3 - floor(x)")]
        public void DecidedByWhatTheQuantifiersEstablish(string statement)
            => Assert.Equal(Boolean.True, statement.ToEntity().Simplify());

        [Theory]
        // False at every whole x: ceil(1) = floor(1).
        [InlineData("forall x in RR : ceil(x) = floor(x) + 1")]
        // False at x = i/2, whose ceiling is i and whose floor is 0: taken componentwise, the
        // ceiling is one above the floor only on the real line.
        [InlineData("forall x in CC : not x in ZZ implies ceil(x) = floor(x) + 1")]
        // False at x = 1/2: 2 x is whole there.
        [InlineData("forall x in RR : not x in ZZ implies ceil(2 x) = floor(2 x) + 1")]
        public void NotClaimedWhereTheFactsDoNotHold(string statement)
            => Assert.NotEqual(Boolean.True, statement.ToEntity().Simplify());

        [Fact]
        public void OutsideAQuantifierNothingIsAssumed()
        {
            // Normal forms are what they were: the argument could be anything.
            Assert.IsType<Floorf>("floor(x + 1)".ToEntity().Simplify());
            Assert.IsType<Ceilf>("ceil(-x)".ToEntity().Simplify());
            // One node, shared with the caller, rewritten inside the statement only because the
            // quantifier establishes that x is whole.
            var sum = MathS.FromString("floor(x) + floor(1 - x)", useCache: false);
            var statement = MathS.ForAll("x", "ZZ", sum.Equalizes(1));
            Assert.Equal(Boolean.True, statement.Simplify());
            Assert.Contains(sum.InnerSimplified.Nodes, node => node is Floorf);
            Assert.Contains(sum.Simplify().Nodes, node => node is Floorf);
        }
    }
}
