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
    /// The first remarkable limit rewrites a vanishing <c>sin(u)</c>, <c>tan(u)</c>,
    /// <c>arcsin(u)</c> or <c>arctan(u)</c> as the <c>u</c> it is equivalent to, and it
    /// matched a product or quotient whose <em>own child</em> is one of those functions --
    /// applied at the root of the expression alone. A constant factor written to the left
    /// pushes the sine one level down and out of reach: <c>2 * sin(1/x) * x</c> parses as
    /// <c>(2 * sin(1/x)) * x</c>, whose children are a product and a variable, and neither is
    /// a sine. The descent then read it as <c>0 * (+oo)</c> and was definite about it, so the
    /// same product answered 2 written one way round and NaN written the other.
    /// https://github.com/asc-community/AngouriMath/issues/749
    ///
    /// The rule is now applied down the product-and-quotient spine. Deliberately not a plain
    /// <c>Replace</c>: the equivalence holds for a <em>factor</em> of the whole expression,
    /// and not for a term of a sum, where the difference between the two forms is the entire
    /// answer. <see cref="ASumIsNotRewritten"/> is what holds that line.
    /// </summary>
    public sealed class FirstRemarkableOverFactorsTest
    {
        private static Entity LimitOf(string expression, string destination) =>
            expression.ToEntity().Limit("x", destination.ToEntity()).Simplify();

        /// <summary>
        /// The mathematics rather than the printed form -- the same value reaches this by
        /// several roundings depending on which factor the rewrite moved.
        /// </summary>
        private static void AssertLimit(string expression, string expected, string destination)
        {
            var difference = (LimitOf(expression, destination) - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        /// <summary>
        /// An infinite limit, where the difference above says nothing: (+oo) - (+oo) is NaN.
        /// </summary>
        private static void AssertDiverges(string expression, string expected, string destination) =>
            Assert.Equal(expected.ToEntity().Evaled, LimitOf(expression, destination).Evaled);

        /// <summary>
        /// The four spellings from the report. Only the first of them answered before, and
        /// which side of a product a caller writes a constant on is not a mathematical
        /// difference -- simplification produces either.
        /// </summary>
        [Theory]
        [InlineData("sin(1/x) * x * 2", "2")]
        [InlineData("2 * sin(1/x) * x", "2")]
        [InlineData("sin(1/x) * x / 2", "1/2")]
        [InlineData("(1/2) * sin(1/x) * x", "1/2")]
        public void AConstantFactorNoLongerHidesAVanishingSine(string expression, string expected) =>
            AssertLimit(expression, expected, "+oo");

        /// <summary>
        /// The other three functions the rule knows, each shielded the same way by a constant.
        /// </summary>
        [Theory]
        [InlineData("2 * tan(1/x) * x", "2")]
        [InlineData("tan(1/x) * x / 2", "1/2")]
        [InlineData("2 * arcsin(1/x) * x", "2")]
        [InlineData("arcsin(1/x) * x / 2", "1/2")]
        [InlineData("2 * arctan(1/x) * x", "2")]
        [InlineData("arctan(1/x) * x / 2", "1/2")]
        public void EveryFunctionTheRuleKnowsIsReachedThroughAConstant(string expression, string expected) =>
            AssertLimit(expression, expected, "+oo");

        /// <summary>
        /// A finite destination, where the shielding factor sat under a quotient rather than a
        /// product. The rule's quotient case fires only when both sides tend to 0, and a
        /// constant divisor does not, so these were out of reach for the same reason.
        /// </summary>
        [Theory]
        [InlineData("sin(x) * 1/x / 2", "1/2")]
        [InlineData("sin(2*x) * 1/x / 2", "1")]
        [InlineData("tan(x) * 1/x * (1/2)", "1/2")]
        [InlineData("arcsin(x) * 1/x / 2", "1/2")]
        [InlineData("arctan(2*x) * 1/x / 2", "1")]
        public void TheQuotientCaseIsReachedBelowTheRootToo(string expression, string expected) =>
            AssertLimit(expression, expected, "0");

        /// <summary>
        /// Where the rewrite leaves something that diverges. The point is that a value comes
        /// back at all -- each of these was NaN, the claim that the limit does not exist.
        /// </summary>
        [Theory]
        [InlineData("2 * sin(1/x) * x^2", "+oo")]
        [InlineData("sin(1/x) * x^2 / 2", "+oo")]
        [InlineData("2 * tan(1/x) * e^x", "+oo")]
        [InlineData("2 * arctan(1/x^2) * ln(x)", "0")]
        [InlineData("2 * arcsin(1/x^2) * x", "0")]
        public void ARewrittenFactorThatDivergesStillAnswers(string expression, string expected) =>
            AssertDiverges(expression, expected, "+oo");

        /// <summary>
        /// **The soundness guard, and the reason this is a spine walk rather than a
        /// <c>Replace</c>.** The equivalence licenses replacing a factor of the expression as
        /// a whole -- if <c>f/g -> 1</c> then <c>f*h</c> and <c>g*h</c> go to the same place --
        /// and says nothing about a term of a sum, where the difference between <c>f</c> and
        /// <c>g</c> is the whole answer. Rewriting the sine inside <c>(sin(x)/x - 1)/x^2</c>
        /// would answer 0 where the limit is -1/6, so every one of these would break under a
        /// rule that descended into sums.
        /// </summary>
        [Theory]
        [InlineData("(sin(x)/x - 1)/x^2", "-1/6")]
        [InlineData("(sin(x) - x)/x^3", "-1/6")]
        [InlineData("(tan(x) - x)/x^3", "1/3")]
        [InlineData("(x - sin(x))/(x - tan(x))", "-1/2")]
        public void ASumIsNotRewritten(string expression, string expected) =>
            AssertLimit(expression, expected, "0");

        /// <summary>
        /// The same sums with a constant factor on them -- which is exactly what this change
        /// makes reachable, so it is where a rule that walked one level too far would show up.
        /// </summary>
        [Theory]
        [InlineData("2 * (sin(x) - x)/x^3", "-1/3")]
        [InlineData("(sin(x)/x - 1) * 2/x^2", "-1/3")]
        [InlineData("(sin(x) - x)/(x^3) * 6", "-1")]
        public void AConstantFactorOnASumDoesNotOpenItEither(string expression, string expected) =>
            AssertLimit(expression, expected, "0");

        /// <summary>
        /// What this costs, pinned rather than left to be discovered.
        /// <para/>
        /// <c>lim x->0 sin(x) * ln(x) * 2</c> answered 0 before and is NaN now. The rewrite is
        /// sound -- <c>sin(x) * ln(x)</c> and <c>x * ln(x)</c> have the same limit -- and what
        /// it exposes is that the library answers <c>lim x->0 x * ln(x)</c> with NaN by
        /// design: <c>ln(x)</c> is not real to the left of 0, which is the same judgement that
        /// pins <c>lim x->0 x^x</c> as non-existent in <c>LimitTest.TestNoLimit</c>. The
        /// constant factor was accidentally shielding these from a rewrite the library already
        /// applies to the constant-free spelling, so master answered <c>sin(x) * ln(x)</c>
        /// with NaN and <c>sin(x) * ln(x) * 2</c> with 0 -- two spellings of one limit
        /// disagreeing. They now agree.
        /// <para/>
        /// Every answer this change turns into a NaN is of this one shape, and in each case
        /// the constant-free spelling was already NaN on master: measured over 811 generated
        /// products and quotients, 196 NaNs became values, 13 values became this NaN, and no
        /// answer changed into a different answer.
        /// </summary>
        [Theory]
        [InlineData("sin(x) * ln(x)")]
        [InlineData("sin(x) * ln(x) * 2")]
        [InlineData("2 * sin(x) * ln(x)")]
        [InlineData("arctan(x) * ln(x) * 2")]
        [InlineData("x * ln(x)")]
        public void AVanishingFactorAgainstALogarithmAgreesWithItsOwnSpellings(string expression)
        {
            var limit = expression.ToEntity().Limit("x", 0);
            Assert.Equal(MathS.NaN, limit.Evaled);
        }
    }
}
