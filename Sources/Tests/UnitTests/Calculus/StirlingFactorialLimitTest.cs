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
    /// A power whose base holds a factorial had no limit at all, because every route out of
    /// <c>ln(f)</c> runs through differentiating <c>f</c> and a factorial's derivative wants the
    /// digamma function, which this library does not have.
    ///
    /// <c>f^g</c> is <c>e^(g * ln f)</c>, and Stirling's expansion is stated for exactly that
    /// logarithm: <c>ln(f!) = f*ln(f) - f + ln(2*pi*f)/2 + 1/(12f) + O(1/f^3)</c>. Applying it
    /// to the exponent rather than substituting for the factorial in the base is what makes it
    /// sound -- what is dropped here **vanishes**, where the asymptotic for <c>f!</c> itself has
    /// an error that is merely relative and survives being raised to a power.
    ///
    /// Vanishing is still not enough on its own: the dropped term is multiplied by the exponent
    /// the rewrite sits under, so <c>power / f -> 0</c> is required. For <c>(x!/x^x)^(1/x)</c>
    /// that ratio is <c>1/x^2</c>.
    ///
    /// https://github.com/asc-community/AngouriMath/issues/754
    /// </summary>
    public sealed class StirlingFactorialLimitTest
    {
        private static Entity LimitOf(string expression) =>
            expression.ToEntity().Limit("x", Entity.Number.Real.PositiveInfinity).Simplify();

        private static void AssertLimit(string expression, string expected)
        {
            var difference = (LimitOf(expression) - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        private static void AssertDiverges(string expression) =>
            Assert.Equal(Entity.Number.Real.PositiveInfinity, LimitOf(expression).Evaled);

        /// <summary>
        /// The reported case, and the last <c>lim:factorial</c> miss in the corpus.
        /// <c>x!/x^x</c> is <c>sqrt(2*pi*x) * e^(-x)</c> to leading order, so its x-th root is
        /// <c>1/e</c> -- checked numerically at <c>x = 1e9</c>, where the quotient's root is
        /// 0.3678794453 against <c>1/e = 0.3678794412</c>.
        /// </summary>
        [Fact]
        public void TheCorpusMissIsAnswered() =>
            AssertLimit("(x! / x^x) ^ (1/x)", "1/e");

        /// <summary>
        /// The wrong answer the same issue reported, which was <c>1</c> and then unevaluated,
        /// and is a value now. <c>(x!)^(1/x)</c> is asymptotic to <c>x/e</c>: at <c>x = 1e9</c>
        /// it is 367879445, against <c>x/e = 367879441</c>.
        /// </summary>
        [Theory]
        [InlineData("(x!) ^ (1/x)")]
        [InlineData("(x!) ^ (2/x)")]
        [InlineData("(x!) ^ (1/ln(x))")]
        [InlineData("(x! * x) ^ (1/x)")]
        [InlineData("(x! / e^x) ^ (1/x)")]
        public void AFactorialUnderAVanishingExponentDiverges(string expression) =>
            AssertDiverges(expression);

        /// <summary>
        /// Where the exponent vanishes fast enough to hold the factorial's growth down. Each of
        /// these was checked numerically before being written here.
        /// </summary>
        [Theory]
        [InlineData("(x!) ^ (1/x^2)", "1")]
        [InlineData("(x! / x^x) ^ (1/x^2)", "1")]
        [InlineData("((x+1)! / x!) ^ (1/x)", "1")]
        public void AnExponentThatVanishesFasterHoldsItDown(string expression, string expected) =>
            AssertLimit(expression, expected);

        /// <summary>
        /// The guard, and the reason the expansion is not simply applied wherever a factorial
        /// appears. What Stirling drops is <c>1/(12f)</c>, and the answer is
        /// <c>e^(power * ln(base))</c> -- so that error contributes <c>power/(12f)</c> to the
        /// exponent and only disappears where <c>power/f</c> does. <c>(x!)^x</c> fails that
        /// (the ratio is <c>x/x</c>, which is 1), and it is left unanswered rather than
        /// answered from an expansion that does not hold there.
        /// </summary>
        [Theory]
        [InlineData("(x!) ^ x")]
        [InlineData("(x!) ^ (x^2)")]
        public void AnExponentThatDoesNotVanishAgainstTheFactorialIsNotRewritten(string expression)
        {
            var limit = expression.ToEntity().Limit("x", Entity.Number.Real.PositiveInfinity);
            Assert.True(limit.Evaled is Entity.Limitf or Entity.Number.Real { IsFinite: false },
                $"{expression} should be left unsettled or diverge, and came back {limit.Evaled}");
        }

        /// <summary>
        /// The powers with no factorial in them, which must reach the same answers by the same
        /// route they always did -- the expansion is only reached where a diverging factorial
        /// is actually present.
        /// </summary>
        [Theory]
        [InlineData("x ^ (1/x)", "1")]
        [InlineData("(1 + 1/x) ^ x", "e")]
        [InlineData("(x - 5) ^ x / x ^ x", "e^(-5)")]
        [InlineData("(x^2) ^ (1/ln(x))", "e^2")]
        [InlineData("(1 - 1/x^2) ^ (x^2)", "1/e")]
        public void APowerWithoutAFactorialIsUntouched(string expression, string expected) =>
            AssertLimit(expression, expected);
    }
}
