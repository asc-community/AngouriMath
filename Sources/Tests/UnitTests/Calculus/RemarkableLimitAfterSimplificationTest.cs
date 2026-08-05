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
    /// The second remarkable limit reads a 1^oo, and it ran once, at the top of
    /// <c>ComputeLimit</c>, before anything had been simplified. A quotient of two powers is
    /// not a 1^oo at that point -- it is a quotient -- so the rule had nothing to match on.
    /// The descent then reached <c>SimplifyAndComputeLimitToInfinity</c>, whose first act is
    /// to simplify, and <c>(x - 5)^x / x^x</c> became <c>((x - 5)/x)^x</c>: a 1^oo that
    /// nothing would now re-read, because the rule that reads one had already run. Straight
    /// on into substitution, where the arithmetic answers 1^(+oo) with 1.
    ///
    /// Written as the single power the same limits were always right, which is what locates
    /// the cause in the ordering rather than in the rule.
    /// https://github.com/asc-community/AngouriMath/issues/738
    ///
    /// What this reaches is what the simplification gathers into one power, and it does not
    /// gather all of them: <c>(x^2 + 1)^x / (x^2)^x</c> comes back written as it went in, so
    /// no 1^oo is ever created and there is nothing here to re-read. That one is left
    /// unevaluated rather than answered wrongly, and its cause sits in the simplification
    /// rather than in the limits -- the same quotient written <c>((x^2 + 1) / x^2)^x</c> is
    /// answered 1 without any of this.
    /// </summary>
    public sealed class RemarkableLimitAfterSimplificationTest
    {
        private static Entity LimitOf(string expression) =>
            expression.ToEntity().Limit("x", "+oo".ToEntity()).Simplify();

        /// <summary>
        /// The mathematics rather than the printed form, and rather than the decimal either:
        /// the machinery answers <c>((x - 5)/x)^x</c> with <c>1/e^5</c> where the expectation
        /// here is written <c>e^(-5)</c>, and evaluating those two reaches the same number by
        /// different roundings, so they disagree in the last digit while being one value.
        /// </summary>
        private static void AssertLimit(string expression, string expected)
        {
            var difference = (LimitOf(expression) - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        /// <summary>
        /// An infinite limit, where the difference above says nothing: (+oo) - (+oo) is NaN.
        /// </summary>
        private static void AssertDiverges(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled, LimitOf(expression).Evaled);

        /// <summary>
        /// The reported wrong answers. Each of these is a quotient whose base ratio tends to 1
        /// and whose exponent diverges, and every one of them answered 1 before.
        /// </summary>
        [Theory]
        [InlineData("(x - 5) ^ x / x ^ x", "e ^ (-5)")]
        [InlineData("(x + 1) ^ x / x ^ x", "e")]
        [InlineData("(x + 3) ^ x / x ^ x", "e ^ 3")]
        [InlineData("(x - 1) ^ x / x ^ x", "e ^ (-1)")]
        [InlineData("x ^ x / (x + 1) ^ x", "e ^ (-1)")]
        [InlineData("x ^ x / (x - 5) ^ x", "e ^ 5")]
        public void AQuotientOfPowersIsReadAfterItBecomesOne(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// The same shape where the answer is not a whole power of e, where both bases move,
        /// and where the exponent is not simply x -- so a rule that merely stopped saying 1
        /// would still be wrong. The ratios are (1 + 1/(2x))^x, ((1 + 1/x)^x)^2 and
        /// (1 - 4/(x + 2))^x.
        /// </summary>
        [Theory]
        [InlineData("(2 * x + 1) ^ x / (2 * x) ^ x", "e ^ (1/2)")]
        [InlineData("(3 * x + 2) ^ x / (3 * x) ^ x", "e ^ (2/3)")]
        [InlineData("(x + 1) ^ (2 * x) / x ^ (2 * x)", "e ^ 2")]
        [InlineData("(x - 2) ^ x / (x + 2) ^ x", "e ^ (-4)")]
        public void WhatTheBaseRatioLeavesBehindDecidesIt(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// The claim the expected values rest on, checked at a point rather than argued.
        /// </summary>
        /// <remarks>
        /// (1 - 5/x)^x reaches e^(-5) from below and slowly -- the relative error falls like
        /// 12.5/x -- so the tolerance here is loose on purpose. It is still nowhere near loose
        /// enough to admit the 1 that used to come back, which is the whole question.
        /// </remarks>
        [Fact]
        public void TheQuotientApproachesTheExpectedValue()
        {
            var atAPoint = "(x - 5) ^ x / x ^ x".ToEntity()
                .Substitute("x", 400).EvalNumerical().RealPart.EDecimal.ToDouble();
            var expected = System.Math.Exp(-5);
            Assert.True(System.Math.Abs(atAPoint - expected) / expected < 0.05,
                $"the quotient at x = 400 is {atAPoint}, against e^(-5) = {expected}");
            Assert.True(System.Math.Abs(atAPoint - 1) > 0.9,
                $"and {atAPoint} is nowhere near the 1 that used to come back");
        }

        /// <summary>
        /// Written as a single power these were always right, and are what showed the rule
        /// itself is sound. They must stay so.
        /// </summary>
        [Theory]
        [InlineData("((x - 5) / x) ^ x", "e ^ (-5)")]
        [InlineData("(1 - 5/x) ^ x", "e ^ (-5)")]
        [InlineData("(1 + 1/x) ^ x", "e")]
        [InlineData("(1 + 2/x) ^ x", "e ^ 2")]
        public void TheSinglePowerFormsAreUnchanged(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// The neighbouring quotients, where the base ratio does not tend to 1 and so the
        /// second remarkable limit has no business firing. These were right before and are the
        /// ones a rule applied too eagerly would break.
        /// </summary>
        [Theory]
        [InlineData("(2 * x) ^ x / x ^ x", "+oo")]
        [InlineData("(x ^ 2) ^ x / x ^ x", "+oo")]
        [InlineData("x ^ x / (2 * x) ^ x", "0")]
        [InlineData("x ^ x / e ^ x", "+oo")]
        [InlineData("x ^ 2 / e ^ x", "0")]
        public void AQuotientWhoseBasesDivergeIsUntouched(string expression, string expected) =>
            AssertDiverges(expression, expected);
    }
}
