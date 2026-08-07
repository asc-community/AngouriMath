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
    /// The two earlier readings of Stirling's expansion both need the factorial to be under a
    /// logarithm already -- one supplies the logarithm itself, by turning <c>f^g</c> into
    /// <c>e^(g * ln f)</c>, and the other rewrites a logarithm that is written down. Neither
    /// reaches <c>x! / x^x</c>, which has no logarithm anywhere in it.
    ///
    /// Here the logarithm is supplied instead: for a positive expression <c>lim H</c> is
    /// <c>e^(lim ln H)</c>, and <c>ln H</c> is where the expansion applies. Not by substituting
    /// <c>e^(Stirling(f))</c> for the factorial, which is the obvious move and measured much
    /// worse -- it puts an <c>e</c> to a large exponent into the expression, the machinery
    /// evaluates that constant to a hundred-digit decimal, and <c>(x!/e^x)^(1/x)</c> went from
    /// half a second to over a minute.
    ///
    /// The guard is the power of the factorial the expression depends on, which is exactly the
    /// coefficient <c>ln(f!)</c> carries in <c>ln H</c>, so the dropped <c>1/(12f)</c> has to
    /// vanish against it. For <c>(x!)^x</c> that power is <c>x</c>, and <c>x/(12x)</c> does
    /// not vanish.
    ///
    /// https://github.com/asc-community/AngouriMath/issues/754
    /// </summary>
    public sealed class StirlingFactorialItselfTest
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
        /// The factorial compared against the things it is usually compared against. Each was
        /// checked numerically before being written here: <c>x!/x^x</c> is 4.0e-433 at
        /// <c>x = 1000</c>, and <c>x!/e^x</c> is 2.0e+2133.
        /// </summary>
        [Fact]
        public void AFactorialAgainstItsOwnPowerVanishes() =>
            AssertLimit("x! / x^x", "0");

        [Theory]
        [InlineData("x^x / x!")]
        [InlineData("x! / e^x")]
        [InlineData("x! / (x-1)!")]
        public void AFactorialAgainstAWeakerGrowthDiverges(string expression) =>
            AssertEquals(expression, Entity.Number.Real.PositiveInfinity);

        /// <summary>
        /// The logarithm of a quotient holding a factorial, which the rewrite that reads a
        /// logarithm now takes apart rather than leaving as one opaque node. Substituting for
        /// the factorial alone would leave <c>ln(e^S / x^x)</c>, which nothing here simplifies
        /// -- so these have to go through the logarithm's expansion and not this one.
        /// </summary>
        [Theory]
        [InlineData("ln(x! / x^x) / x", "-1")]
        [InlineData("ln(x! / x^x) / (x * ln(x))", "0")]
        [InlineData("ln(x^x / x!) / x", "1")]
        public void ALogarithmOfAQuotientHoldingAFactorialIsTakenApart(string expression, string expected) =>
            AssertLimit(expression, expected);

        [Theory]
        [InlineData("ln(x! / x^x)")]
        [InlineData("x * ln(x! / x^x)")]
        [InlineData("ln(x! / x^x) / ln(x)")]
        public void ALogarithmOfAVanishingQuotientDivergesDownwards(string expression) =>
            AssertEquals(expression, Entity.Number.Real.NegativeInfinity);

        /// <summary>
        /// **The guard.** <c>(x!)^x</c> depends on its factorial through the power <c>x</c>,
        /// so the dropped <c>1/(12f)</c> would arrive as <c>x/(12x)</c>, which does not
        /// vanish. It is refused there, and the answer is still <c>+oo</c>, reached without
        /// any of this.
        /// </summary>
        [Theory]
        [InlineData("(x!) ^ x")]
        [InlineData("(x!) ^ (x^2)")]
        public void APowerThatGrowsAgainstTheFactorialIsRefused(string expression) =>
            AssertEquals(expression, Entity.Number.Real.PositiveInfinity);

        /// <summary>
        /// Nothing without a factorial in it is touched -- the rewrite is reached only where a
        /// diverging factorial is present, and only once nothing else has answered.
        /// </summary>
        [Theory]
        [InlineData("x^x / e^x")]
        [InlineData("e^x / x^x")]
        [InlineData("x^20 / e^x")]
        public void AnExpressionWithoutAFactorialIsUntouched(string expression)
        {
            var limit = LimitOf(expression).Evaled;
            Assert.True(limit == Entity.Number.Real.PositiveInfinity || limit == Entity.Number.Integer.Create(0),
                $"{expression} came back {limit}");
        }
    }
}
