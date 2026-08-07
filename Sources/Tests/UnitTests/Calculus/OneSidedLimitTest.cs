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
    /// A one-sided limit had nothing behind it. Where the two-sided and the infinite paths
    /// fall through to l'Hopital's rule, this one returned whatever the descent produced --
    /// and the descent can make an indeterminate form definite on the way down, by putting a
    /// part's own limit in place of the part. x * ln(x) at 0+ becomes 0 * ln(x), so 0 * -oo,
    /// so NaN, and NaN is the claim that the limit does not exist.
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class OneSidedLimitTest
    {
        private static Entity Limit(string expression, string destination, ApproachFrom side) =>
            expression.ToEntity().Limit("x", destination.ToEntity(), side).Simplify();

        private static void AssertLimit(string expression, string destination, ApproachFrom side, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, Limit(expression, destination, side).Evaled);

        /// <summary>
        /// Every one of these came back NaN. sin(x) / x is the one worth naming: approached
        /// from either side it is 1, and the library said the limit did not exist.
        /// </summary>
        [Theory]
        [InlineData("sin(x) / x", "0", "1")]
        [InlineData("tan(x) / x", "0", "1")]
        [InlineData("x / sin(x)", "0", "1")]
        [InlineData("x * ln(x)", "0", "0")]
        public void FormsThatUsedToBeCalledNonExistentFromTheRight(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.Right, expected);

        [Theory]
        [InlineData("sin(x) / x", "0", "1")]
        [InlineData("x / sin(x)", "0", "1")]
        public void FormsThatUsedToBeCalledNonExistentFromTheLeft(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, ApproachFrom.Left, expected);

        /// <summary>
        /// The second remarkable limit was applied only on the two-sided path, so from one side
        /// the descent read (1 + x)^(1/x) as 1^(+oo) and was definite about it: it answered 1,
        /// where the same expression approached from both sides gives e. Not a NaN, so nothing
        /// behind the descent would have been consulted -- this one had to be fixed in front
        /// of it.
        /// </summary>
        [Theory]
        [InlineData("(1 + x) ^ (1/x)", "0", ApproachFrom.Right, "e")]
        [InlineData("(1 + x) ^ (1/x)", "0", ApproachFrom.Left, "e")]
        [InlineData("(1 + 2 * x) ^ (1/x)", "0", ApproachFrom.Right, "e ^ 2")]
        [InlineData("(1 + 2 * x) ^ (1/x)", "0", ApproachFrom.Left, "e ^ 2")]
        public void TheRemarkableLimitsApplyFromOneSideToo(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// l'Hopital's rule was reached from the two-sided path only, so a one-sided limit of a
        /// quotient the descent could not read had nothing to fall back on. csc(x) * x is the
        /// odd one out: the csc rewrite makes it the product (1 / sin(x)) * x, which the descent
        /// does not take apart, and the rule reads it back as the quotient x / sin(x).
        /// </summary>
        [Theory]
        [InlineData("(1 - cos(x)) / x ^ 2", "0", ApproachFrom.Right, "1/2")]
        [InlineData("(1 - cos(x)) / x ^ 2", "0", ApproachFrom.Left, "1/2")]
        [InlineData("csc(x) * x", "0", ApproachFrom.Right, "1")]
        [InlineData("csc(x) * x", "0", ApproachFrom.Left, "1")]
        public void AQuotientTheDescentCannotReadGoesToTheRule(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The rule is asked with the side it was given, which is what the rule is stated with
        /// in the first place. Both questions it asks along the way -- what the two parts tend
        /// to, and what the differentiated quotient tends to -- can have a one-sided answer and
        /// no two-sided one. Here the first step of (1 - cos(x)) / x^3 reaches sin(x) / (3x^2),
        /// which is +oo on the right and -oo on the left, so asking about both sides at once
        /// gives NaN and the step is wasted.
        /// </summary>
        [Theory]
        [InlineData("(1 - cos(x)) / x ^ 3", "0", ApproachFrom.Right, "+oo")]
        [InlineData("(1 - cos(x)) / x ^ 3", "0", ApproachFrom.Left, "-oo")]
        public void TheRuleIsAskedAboutTheSideItWasGiven(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// The one-sided answers that were already right, kept. The fallback is only consulted
        /// where the descent returned nothing or NaN, so none of these should reach it at all.
        /// </summary>
        [Theory]
        [InlineData("1 / x", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / x", "0", ApproachFrom.Left, "-oo")]
        [InlineData("1 / x ^ 2", "0", ApproachFrom.Right, "+oo")]
        [InlineData("1 / x ^ 2", "0", ApproachFrom.Left, "+oo")]
        [InlineData("ln(x)", "0", ApproachFrom.Right, "-oo")]
        [InlineData("ln(x) / x", "0", ApproachFrom.Right, "-oo")]
        [InlineData("e ^ (1/x)", "0", ApproachFrom.Right, "+oo")]
        [InlineData("e ^ (1/x)", "0", ApproachFrom.Left, "0")]
        [InlineData("1 / (x - 2)", "2", ApproachFrom.Right, "+oo")]
        [InlineData("1 / (x - 2)", "2", ApproachFrom.Left, "-oo")]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "1", ApproachFrom.Right, "2")]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "1", ApproachFrom.Left, "2")]
        [InlineData("x / (x + 1)", "0", ApproachFrom.Right, "0")]
        public void EstablishedOneSidedLimitsAreUnaffected(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);

        /// <summary>
        /// https://github.com/asc-community/AngouriMath/issues/596 -- the reporter's difference
        /// of reciprocal logarithms, which is -1/2. Both one-sided readings answer it; the
        /// two-sided one does not, and promoting the agreement of the two into a two-sided answer
        /// is not open, since it would also make lim x->0 x * ln(x) equal 0, where the library
        /// deliberately answers NaN because ln(x) is not real on the left.
        /// </summary>
        [Theory]
        [InlineData(ApproachFrom.Right)]
        [InlineData(ApproachFrom.Left)]
        public void ADifferenceOfReciprocalLogarithms(ApproachFrom side)
        {
            var task = System.Threading.Tasks.Task.Run(() =>
                Limit("1 / ln(x + sqrt(x * x + 1)) - 1 / ln(x + 1)", "0", side));
            Assert.True(task.Wait(System.TimeSpan.FromSeconds(60)), "the limit did not terminate");
            Assert.Equal("-1/2".ToEntity().Evaled, task.Result.Evaled);
        }

        /// <summary>
        /// The fallback moves x out to infinity, which is what a finite destination does in any
        /// case. It must not be reached for a destination that is already infinite, where the
        /// substitution would mean nothing.
        /// </summary>
        [Theory]
        [InlineData("1 / x", "+oo", ApproachFrom.Right, "0")]
        [InlineData("x ^ 2 / (x ^ 2 + 1)", "+oo", ApproachFrom.Left, "1")]
        [InlineData("1 / x", "-oo", ApproachFrom.Left, "0")]
        public void AnInfiniteDestinationIsLeftAlone(string expression, string destination, ApproachFrom side, string expected) =>
            AssertLimit(expression, destination, side, expected);
    }
}
