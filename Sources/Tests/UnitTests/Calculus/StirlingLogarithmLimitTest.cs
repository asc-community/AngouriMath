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
    /// Stirling's expansion arrived with <c>SolveAsIndeterminatePower</c>, which is where
    /// <c>f^g</c> becomes <c>e^(g * ln f)</c> -- so it found a factorial only under a vanishing
    /// exponent. <c>((x!)/x^x)^(1/x)</c> was answered and <c>ln(x!)/x</c> was not, which is the
    /// harder question answered and the easier one left.
    ///
    /// This applies it wherever a diverging factorial's logarithm appears, from the position
    /// that is reached only once nothing else has a reading.
    /// https://github.com/asc-community/AngouriMath/issues/765
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class StirlingLogarithmLimitTest
    {
        private static Entity LimitOf(string expression) =>
            expression.ToEntity().Limit("x", Entity.Number.Real.PositiveInfinity).Simplify();

        private static void AssertLimit(string expression, string expected)
        {
            var difference = (LimitOf(expression) - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        private static void AssertEquals(string expression, Entity expected) =>
            Assert.Equal(expected.Evaled, LimitOf(expression).Evaled);

        /// <summary>
        /// <c>ln(n!) ~ n*ln(n)</c> is the standard statement of the result, and these are the
        /// ordinary ways of writing it. Each value was checked numerically at up to
        /// <c>x = 1e12</c> before being written here -- <c>ln(x!)/(x*ln(x))</c> approaches 1
        /// from below and slowly, reaching only 0.9638 there.
        /// </summary>
        [Theory]
        [InlineData("ln(x!) / (x * ln(x))", "1")]
        [InlineData("ln(x!) / x^2", "0")]
        [InlineData("ln((2*x)!) / (x * ln(x))", "2")]
        [InlineData("ln((x+1)!) / x^2", "0")]
        public void TheLogarithmOfAFactorialIsRead(string expression, string expected) =>
            AssertLimit(expression, expected);

        [Theory]
        [InlineData("ln(x!) / x")]
        [InlineData("ln((2*x)!) / x")]
        [InlineData("ln((x+1)!) / x")]
        public void TheLogarithmOverAPolynomialDiverges(string expression) =>
            AssertEquals(expression, Entity.Number.Real.PositiveInfinity);

        /// <summary>
        /// <c>ln(x!)/ln(x)</c> was <c>NaN</c> -- the claim that the limit does not exist, where
        /// it is <c>+oo</c>: the quotient is asymptotic to <c>x</c>, and is 9.6e11 at
        /// <c>x = 1e12</c>. A wrong answer rather than a missing one, and the only one in this
        /// family.
        /// </summary>
        [Theory]
        [InlineData("ln(x!) / ln(x)")]
        [InlineData("ln((2*x)!) / ln(x)")]
        [InlineData("ln((x+1)!) / ln(x)")]
        public void AQuotientOfLogarithmsIsNoLongerCalledNonExistent(string expression)
        {
            var limit = LimitOf(expression);
            Assert.NotEqual(MathS.NaN, limit.Evaled);
            Assert.Equal(Entity.Number.Real.PositiveInfinity, limit.Evaled);
        }

        /// <summary>
        /// A difference, where the expansion's *additive* error is what makes this sound at
        /// all: <c>ln(x!) - x*ln(x)</c> is <c>-x + ln(2*pi*x)/2</c> and diverges downwards.
        /// The asymptotic for <c>x!</c> itself has a merely relative error and says nothing
        /// here.
        /// </summary>
        [Fact]
        public void ADifferenceAgainstTheLeadingTermDivergesDownwards() =>
            AssertEquals("ln(x!) - x * ln(x)", Entity.Number.Real.NegativeInfinity);

        /// <summary>
        /// **The guard.** What Stirling drops is <c>1/(12f)</c>, and what that costs the answer
        /// is the rate at which the answer moves with the logarithm -- so the coefficient is
        /// read off by putting a variable where the logarithm is and differentiating, and the
        /// rewrite is refused unless <c>coefficient / f</c> vanishes. Here the coefficient is
        /// <c>x</c> and the ratio is 1, so it is refused and the limit is left unsettled.
        /// <para/>
        /// The guard is sufficient rather than necessary, and this expression shows the gap:
        /// its limit is <c>-oo</c>, which the dropped <c>1/12</c> would not have changed. It is
        /// refused because the same coefficient on
        /// <c>x * (ln(x!) - (x*ln(x) - x + ln(2*pi*x)/2))</c> gives <c>1/12</c> exactly -- an
        /// expression built out of the dropped term -- and nothing available here tells the two
        /// apart.
        /// </summary>
        [Fact]
        public void ACoefficientThatDoesNotVanishAgainstTheFactorialIsRefused()
        {
            var limit = "ln(x!) * x - x^2 * ln(x)".ToEntity()
                .Limit("x", Entity.Number.Real.PositiveInfinity);
            Assert.True(limit.Evaled is Entity.Limitf,
                $"the expansion should be refused here, and the limit came back {limit.Evaled}");
        }

        /// <summary>
        /// Expressions with no factorial in them, which must be untouched -- the rewrite is
        /// reached only where a diverging factorial's logarithm is actually present, and only
        /// once nothing else has answered.
        /// </summary>
        [Theory]
        [InlineData("ln(x) / x", "0")]
        [InlineData("ln(x^2) / ln(x)", "2")]
        [InlineData("(ln(x) + 1) / ln(x)", "1")]
        public void AnExpressionWithoutAFactorialIsUntouched(string expression, string expected) =>
            AssertLimit(expression, expected);
    }
}
