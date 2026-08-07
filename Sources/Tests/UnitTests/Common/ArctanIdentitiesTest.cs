//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// arctan(1/2) + arctan(1/3) is pi/4, and neither half of that was reachable:
    /// the two could not be added, and arctan(1) stayed as it was. No issue is filed for
    /// this; it comes from the gaps a solver corpus showed.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class ArctanIdentitiesTest
    {
        private static Entity Simplified(string expression) => expression.ToEntity().Simplify();

        [Theory]
        [InlineData("arctan(1/2) + arctan(1/3)", "pi / 4")]
        [InlineData("arctan(1/3) + arctan(1/2)", "pi / 4")]
        [InlineData("arctan(1/5) + arctan(1/8)", "arctan(1/3)")]
        [InlineData("arctan(1/2) + arctan(1/5)", "arctan(7/9)")]
        public void TwoArctangentsAddUp(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), Simplified(expression));

        [Theory]
        [InlineData("arctan(1)", "pi / 4")]
        [InlineData("arctan(-1)", "-pi / 4")]
        [InlineData("arctan(0)", "0")]
        [InlineData("arctan(sqrt(3))", "pi / 3")]
        [InlineData("arctan(1/sqrt(3))", "pi / 6")]
        public void TheAnglesArctangentNamesOutright(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), Simplified(expression));

        /// <summary>
        /// The addition formula holds as written only while the product of the two
        /// arguments is below one. Past that the sum leaves the range arctan answers in and
        /// the identity is off by a whole pi, so those are left alone rather than answered
        /// wrongly: arctan(2) + arctan(3) is 3pi/4, not arctan(-1).
        /// </summary>
        [Theory]
        [InlineData("arctan(2) + arctan(3)")]
        [InlineData("arctan(1) + arctan(2)")]
        [InlineData("arctan(x) + arctan(y)")]
        public void WhereTheFormulaDoesNotHoldNothingIsClaimed(string expression) =>
            Assert.Contains("arctan", Simplified(expression).Stringize());

        /// <summary>
        /// Whatever a sum of two arctangents simplifies to has to be the same number. The
        /// pairs run either side of the boundary the formula is guarded by.
        /// </summary>
        [Fact]
        public void ASimplifiedSumIsTheSameNumber()
        {
            double[] values = { -5, -3, -2, -1.5, -1, -0.75, -0.5, -0.25, 0, 0.25, 0.5, 0.75, 1, 1.5, 2, 3, 5 };
            foreach (var a in values)
                foreach (var b in values)
                {
                    var simplified = Simplified($"arctan({a}) + arctan({b})");
                    var actual = simplified.EvalNumerical().RealPart.EDecimal.ToDouble();
                    Assert.Equal(Math.Atan(a) + Math.Atan(b), actual, 9);
                }
        }

        // The identities next to this one have to keep working.
        [Theory]
        [InlineData("arcsin(x) + arccos(x)", "pi / 2")]
        [InlineData("arctan(x) + arccotan(x)", "pi / 2")]
        [InlineData("tan(arctan(x))", "x")]
        [InlineData("arctan(tan(1/2))", "1/2")]
        public void NeighbouringIdentitiesAreUnaffected(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Simplify(), Simplified(expression));
    }
}
