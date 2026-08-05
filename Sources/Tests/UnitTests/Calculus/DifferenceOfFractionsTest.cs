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
        /// https://github.com/asc-community/AngouriMath/issues/727.
        /// <para/>
        /// The same difference with both denominators squared. Putting it over a common
        /// denominator is not enough on its own here: the denominator that comes out is a
        /// product of two vanishing factors, so l'Hopital's rule differentiates it by the
        /// product rule and the quotient grows at every step before it collapses. The rule
        /// used to refuse the first of those steps and leave the descent to answer
        /// +oo - +oo, which is NaN -- the claim that the limit does not exist -- where it
        /// is -1/3.
        /// </summary>
        [Theory]
        [InlineData("1/x ^ 2 - 1/sin(x) ^ 2", "0", "-1/3")]
        [InlineData("1/sin(x) ^ 2 - 1/x ^ 2", "0", "1/3")]
        [InlineData("csc(x) ^ 2 - 1/x ^ 2", "0", "1/3")]
        public void ADifferenceOfSquaredFractionsIsSettledToo(string expression, string destination, string expected) =>
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
