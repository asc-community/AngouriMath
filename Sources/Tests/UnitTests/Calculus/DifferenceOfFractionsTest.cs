//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A difference of two fractions that both diverge at the point being approached. Nothing
    /// in the descent takes a difference apart any further than asking what each half tends to,
    /// and each half here is an infinity, so 1/x - 1/sin(x) at 0 came out as NaN -- the claim
    /// that the limit does not exist -- where it is 0. Over the common denominator the same
    /// expression is (sin(x) - x) / (x * sin(x)), which is an ordinary 0/0 that l'Hopital's
    /// rule settles.
    /// </summary>
    public sealed class DifferenceOfFractionsTest
    {
        // Compared as numbers, since "1/2" parses as a division of two integers rather than as
        // the rational the limit answers with.
        private static void AssertLimit(string expression, string destination, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Evaled);

        [Theory]
        [InlineData("1/x - 1/sin(x)", "0", "0")]
        [InlineData("1/sin(x) - 1/x", "0", "0")]
        [InlineData("csc(x) - cotan(x)", "0", "0")]
        [InlineData("1/ln(x) - 1/(x - 1)", "1", "1/2")]
        [InlineData("1/(x - 1) - 1/ln(x)", "1", "-1/2")]
        public void ADifferenceOfDivergentFractionsGoesOverACommonDenominator(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        /// <summary>
        /// A sum is only put over a common denominator where a denominator contains the variable
        /// -- those are the ones that vanish or diverge and make the difference indeterminate.
        /// Combining over a constant gains nothing and still costs the rule an expression to
        /// differentiate.
        /// </summary>
        [Theory]
        [InlineData("x / 2 + x / 3", "+oo", "+oo")]
        [InlineData("1/x + 1/x ^ 2", "+oo", "0")]
        [InlineData("x - 1/x", "+oo", "+oo")]
        [InlineData("(x + 1) / x", "+oo", "1")]
        [InlineData("1/x - 1/x", "0", "0")]
        [InlineData("x + 1/2", "0", "1/2")]
        public void EstablishedLimitsAreUnaffected(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);
    }
}
