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
    /// https://github.com/asc-community/AngouriMath/issues/335 -- the descent substituted the
    /// parts' own limits only where both of them were finite, so a combination that is perfectly
    /// determinate with an infinity in it was left to the solvers, which substitute the
    /// destination and read what comes out. That answered the same limit differently depending on
    /// how it was written: cos(x) / sin(x) at 0+ was +oo and cos(x) * (1 / sin(x)) was
    /// unevaluated.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class DeterminateCombinationLimitTest
    {
        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity(), side).Simplify().Evaled);

        /// <summary>
        /// A finite non-zero factor against a diverging one: the product diverges, with the sign
        /// the finite factor gives it.
        /// </summary>
        [Theory]
        [InlineData("cos(x) * (1 / sin(x))", "0", ApproachFrom.Right, "+oo")]
        [InlineData("cos(x) * (1 / sin(x))", "0", ApproachFrom.Left, "-oo")]
        [InlineData("(3 + x) * ln(x)", "0", ApproachFrom.Right, "-oo")]
        [InlineData("(2 + 1/x) * x", "+oo", ApproachFrom.BothSides, "+oo")]
        [InlineData("(x - 5) * (1 / x)", "0", ApproachFrom.Right, "-oo")]
        public void AFiniteFactorAgainstADivergingOne(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The rest of the algebra of limits, which reads the same way: a finite part added to a
        /// diverging one diverges, and a finite dividend over a diverging divisor vanishes.
        /// </summary>
        [Theory]
        [InlineData("cos(x) + 1 / x", "0", ApproachFrom.Right, "+oo")]
        [InlineData("cos(x) - 1 / x", "0", ApproachFrom.Right, "-oo")]
        [InlineData("cos(x) / (1 / x)", "0", ApproachFrom.Right, "0")]
        [InlineData("ln(x) + ln(x)", "0", ApproachFrom.Right, "-oo")]
        public void TheRestOfTheAlgebraOfLimits(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The indeterminate combinations are still indeterminate and must keep falling through
        /// to the readings that can take them apart. Each of these is a form the arithmetic
        /// answers with NaN, and each has a limit that is not NaN.
        /// </summary>
        [Theory]
        [InlineData("x * ln(x)", "0", ApproachFrom.Right, "0")]            // 0 * -oo
        [InlineData("(1 / x) / (1 / x ^ 2)", "0", ApproachFrom.Right, "0")] // +oo / +oo
        [InlineData("1 / x - 1 / sin(x)", "0", ApproachFrom.Right, "0")]    // +oo - +oo
        [InlineData("sin(x) / x", "0", ApproachFrom.Right, "1")]            // 0 / 0
        public void TheIndeterminateOnesStillFallThrough(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// Powers are deliberately not settled this way. The arithmetic gives 1 for both
        /// 1 ^ (+oo) and (+oo) ^ 0, and as limits neither is settled at all.
        /// </summary>
        [Theory]
        [InlineData("(1 + 1/x) ^ x", "+oo", "e")]
        [InlineData("(1 + x) ^ (1/x)", "0", "e")]
        public void APowerIsNotSettledByItsPartsAlone(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.BothSides, expected);
    }
}
