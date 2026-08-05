//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// https://github.com/asc-community/AngouriMath/issues/333 -- arcsine had no limit at an
    /// infinite argument. It is not real past 1, and the library reads it on the side of the cut
    /// below the real axis, so along the reals it goes off to infinity in the imaginary direction
    /// with its real part settling at a right angle.
    /// </summary>
    public sealed class InverseTrigonometryLimitTest
    {
        private static Entity Limit(string expression, string destination) =>
            expression.ToEntity().Limit("x", destination.ToEntity(), ApproachFrom.BothSides).Simplify();

        /// <summary>
        /// Compared as numbers rather than as decimals: a right angle reached one way carries a
        /// trailing zero the same right angle reached another way does not, and the two are the
        /// same number. An infinite part has to match in sign and in being infinite.
        /// </summary>
        private static void AssertSameNumber(string expected, Entity actual)
        {
            var (want, got) = ((Entity.Number.Complex)expected.ToEntity().Evaled,
                               (Entity.Number.Complex)actual.Evaled);
            AssertSamePart(want.RealPart, got.RealPart, "real");
            AssertSamePart(want.ImaginaryPart, got.ImaginaryPart, "imaginary");

            static void AssertSamePart(Entity.Number.Real want, Entity.Number.Real got, string which)
            {
                Assert.True(want.IsFinite == got.IsFinite, $"the {which} part: wanted {want}, got {got}");
                if (want.IsFinite)
                    Assert.True(System.Math.Abs(want.EDecimal.ToDouble() - got.EDecimal.ToDouble()) < 1e-12,
                        $"the {which} part: wanted {want}, got {got}");
                else
                    Assert.True(want.IsNegative == got.IsNegative, $"the {which} part: wanted {want}, got {got}");
            }
        }

        [Theory]
        [InlineData("arcsin(x)", "+oo", "pi / 2 - i * (+oo)")]
        [InlineData("arcsin(x)", "-oo", "-pi / 2 - i * (+oo)")]
        [InlineData("arccos(x)", "+oo", "i * (+oo)")]
        [InlineData("arccos(x)", "-oo", "pi + i * (+oo)")]
        [InlineData("arcsec(x)", "+oo", "pi / 2")]
        [InlineData("arcsec(x)", "-oo", "pi / 2")]
        [InlineData("arccsc(x)", "+oo", "0")]
        public void ADivergingArgumentIsAnswered(string expression, string destination, string expected) =>
            AssertSameNumber(expected, Limit(expression, destination));

        /// <summary>
        /// The limit has to be the limit of the function this library computes, whichever side of
        /// the cut that is. arcsin(10^6) is a right angle with a large negative imaginary part
        /// here, so the limit is a right angle with an infinite negative imaginary part, and the
        /// two must at least agree in sign.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x)", "1000000", "+oo")]
        [InlineData("arcsin(x)", "-1000000", "-oo")]
        [InlineData("arccos(x)", "1000000", "+oo")]
        [InlineData("arccos(x)", "-1000000", "-oo")]
        public void TheLimitAgreesWithTheFunctionItIsALimitOf(string expression, string farOut, string destination)
        {
            var atAPoint = (Entity.Number.Complex)expression.ToEntity().Substitute("x", farOut.ToEntity()).EvalNumerical();
            var atInfinity = (Entity.Number.Complex)Limit(expression, destination).Evaled;
            Assert.Equal(atAPoint.RealPart, atInfinity.RealPart);
            Assert.False(atAPoint.ImaginaryPart.IsZero);
            Assert.False(atInfinity.ImaginaryPart.IsFinite);
            Assert.Equal(atAPoint.ImaginaryPart.IsNegative, atInfinity.ImaginaryPart.IsNegative);
        }

        /// <summary>
        /// The arguments that stay inside the domain are unaffected, and so is the first
        /// remarkable limit that arcsine takes part in.
        /// </summary>
        [Theory]
        [InlineData("arcsin(x) / x", "0", "1")]
        [InlineData("arcsin(1 / x)", "+oo", "0")]
        [InlineData("arccos(1 / x)", "+oo", "pi / 2")]
        [InlineData("arctan(x)", "+oo", "pi / 2")]
        [InlineData("arctan(x)", "-oo", "-pi / 2")]
        public void TheOnesInsideTheDomainAreUnaffected(string expression, string destination, string expected) =>
            AssertSameNumber(expected, Limit(expression, destination));
    }
}
