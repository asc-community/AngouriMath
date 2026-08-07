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
    /// Reading a limit off the value at the destination is right wherever the expression is
    /// continuous there, and an indeterminate form is exactly where it is not. Nearly all of
    /// them decline without any help, because they evaluate to NaN and every caller reads NaN
    /// as "no limit": <c>0 * oo</c>, <c>oo - oo</c>, <c>oo / oo</c>, <c>0^0</c>.
    ///
    /// The two that do not are <c>oo^0</c> and <c>1^oo</c>, which this library's arithmetic
    /// answers with 1 -- the same convention SymPy and IEEE 754's <c>pow</c> use, and one this
    /// change does not touch. So a limit that assembled either of them read that 1 off as its
    /// answer, and was definite about limits it had no reading of:
    ///
    ///     lim x->+oo (x!)^(1/x)      was 1, is +oo
    ///     lim x->+oo (x!)^(1/ln(x))  was 1, is +oo
    ///     lim x->+oo (e^x)^(1/ln(x)) was 1, is +oo
    ///     lim x->+oo (x^2)^(1/ln(x)) was 1, is e^2
    ///
    /// https://github.com/asc-community/AngouriMath/issues/754
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class IndeterminatePowerSubstitutionTest
    {
        private static Entity LimitOf(string expression, string destination) =>
            expression.ToEntity().Limit("x", destination.ToEntity()).Simplify();

        /// <summary>
        /// The mathematics rather than the printed form, and rather than the decimal either:
        /// the machinery answers <c>((x - 5)/x)^x</c> with <c>1/e^5</c> where the expectation
        /// is written <c>e^(-5)</c>, and evaluating those two reaches the same number by
        /// different roundings, so they disagree in the last digit while being one value.
        /// </summary>
        private static void AssertLimit(string expression, string destination, string expected)
        {
            var difference = (LimitOf(expression, destination) - expected.ToEntity()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        /// <summary>
        /// An infinite limit, where the difference above says nothing: (+oo) - (+oo) is NaN.
        /// </summary>
        private static void AssertDiverges(string expression, string destination) =>
            Assert.Equal(Entity.Number.Real.PositiveInfinity, LimitOf(expression, destination).Evaled);

        /// <summary>
        /// Left unsettled: the expression comes back as the limit of itself, which is the
        /// library's way of saying "no rule found one". Deliberately distinct from NaN, which
        /// would claim the limit does not exist -- each of these has one.
        /// </summary>
        private static void AssertNotSettled(string expression, string destination)
        {
            var limit = expression.ToEntity().Limit("x", destination.ToEntity()).Evaled;
            Assert.True(limit is Entity.Limitf,
                $"expected lim x->{destination} {expression} to be left unsettled, got {limit}");
        }

        /// <summary>
        /// The reported wrong answer and its relatives. Every one of these was <c>1</c>,
        /// assembled as <c>(+oo)^0</c> and read off. Three are <c>+oo</c> and one is
        /// <c>e^2</c>, so the <c>1</c> was not merely imprecise.
        /// </summary>
        [Theory]
        [InlineData("(e^x) ^ (1/ln(x))")]
        [InlineData("(x^2) ^ (1/ln(x))")]
        [InlineData("(x!) ^ (1/x)")]
        [InlineData("(x!) ^ (1/ln(x))")]
        public void APowerOverALogarithmIsNoLongerRead(string expression) =>
            Assert.NotEqual(Entity.Number.Integer.Create(1), LimitOf(expression, "+oo").Evaled);

        /// <summary>
        /// And the two of those the machinery can now finish, which it reaches through
        /// <c>SolveAsIndeterminatePower</c> once the substitution stops answering first.
        /// <c>(x^2)^(1/ln x)</c> is <c>e^(2*ln(x)/ln(x))</c>, that is <c>e^2</c> at every x,
        /// and <c>(e^x)^(1/ln x)</c> is <c>e^(x/ln x)</c>, which diverges.
        /// </summary>
        [Fact]
        public void ALogarithmicExponentOverAPolynomialIsAnsweredProperly() =>
            AssertLimit("(x^2) ^ (1/ln(x))", "+oo", "e^2");

        [Fact]
        public void ALogarithmicExponentOverAnExponentialDiverges() =>
            AssertDiverges("(e^x) ^ (1/ln(x))", "+oo");

        /// <summary>
        /// The factorial cases from the report, which were left unsettled when this guard
        /// landed and are answered now that Stirling's expansion of <c>ln(f!)</c> reaches them.
        /// <c>(x!)^(1/x)</c> grows like <c>x/e</c> -- <c>(100!)^(1/100)</c> is already 37.99 --
        /// and it is the value the substitution used to read off as <c>1</c>.
        /// </summary>
        [Theory]
        [InlineData("(x!) ^ (1/x)")]
        [InlineData("(x!) ^ (1/ln(x))")]
        public void AFactorialUnderAVanishingExponentDiverges(string expression) =>
            AssertDiverges(expression, "+oo");

        /// <summary>
        /// **What this used to cost, and no longer does.** <c>(x!)^(1/x^2)</c> is 1 -- its
        /// logarithm is <c>ln(x!)/x^2 ~ ln(x)/x</c>, which vanishes -- and the old substitution
        /// landed on that 1 for the same reason it landed on 1 for <c>(x!)^(1/x)</c>, where the
        /// answer is <c>+oo</c>. It was right by luck rather than by reading, so it was
        /// withdrawn along with the case that was wrong. Stirling's expansion answers it by
        /// reading, which is what makes keeping it worth anything.
        /// </summary>
        [Fact]
        public void TheFactorialCaseThatWasAccidentallyRightIsAnsweredByReading() =>
            AssertLimit("(x!) ^ (1/x^2)", "+oo", "1");

        /// <summary>
        /// <c>0^0</c> is deliberately **not** guarded. It evaluates to NaN, and that NaN is
        /// how the library says <c>lim x->0 x^x</c> does not exist -- <c>x^x</c> is not real to
        /// the left of 0 -- which <c>LimitTest.TestNoLimit</c> pins. Declining it would turn a
        /// considered "does not exist" into "not settled".
        /// </summary>
        [Fact]
        public void ZeroToTheZeroKeepsItsConsideredNaN()
        {
            var limit = "limit(x^x, x, 0)".ToEntity();
            Assert.Equal(MathS.NaN, limit.InnerSimplified);
            Assert.Equal(MathS.NaN, limit.Evaled);
        }

        /// <summary>
        /// The powers that were right before and have to stay so. The first four are
        /// <c>1^oo</c>, which the second remarkable limit reads before the substitution is ever
        /// reached, and the rest are <c>oo^0</c> and <c>0^0</c> answered through
        /// <c>SolveAsIndeterminatePower</c>. These are the ones a guard applied too eagerly
        /// would silence.
        /// </summary>
        [Theory]
        [InlineData("(1 + 1/x) ^ x", "+oo", "e")]
        [InlineData("(x - 5) ^ x / x ^ x", "+oo", "e^(-5)")]
        [InlineData("(1 - 1/x^2) ^ (x^2)", "+oo", "1/e")]
        [InlineData("((x+1)/(x+2)) ^ x", "+oo", "1/e")]
        [InlineData("(1 + x) ^ (1/x)", "0", "e")]
        [InlineData("x ^ (1/x)", "+oo", "1")]
        [InlineData("(1/x) ^ (1/x)", "+oo", "1")]
        [InlineData("1 ^ x", "+oo", "1")]
        public void ThePowersThatWereAlreadyRightAreUndisturbed(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        /// <summary>
        /// An exponent that diverges rather than vanishing, so no indeterminate form is
        /// assembled and the guard has no business firing.
        /// </summary>
        [Theory]
        [InlineData("(1 + 1/x) ^ (x^2)")]
        [InlineData("x ^ x")]
        [InlineData("(x^2) ^ x")]
        public void ADivergingPowerIsUntouched(string expression) =>
            AssertDiverges(expression, "+oo");

        /// <summary>
        /// The one-sided readings of the two-sided limits this change stops answering. Both
        /// are unchanged, and they are the meaningful ones: <c>lim x->0 1/x</c> has no
        /// two-sided limit at all, and <c>(1/x)^x</c> is not real to the left of 0, so the
        /// <c>1</c> the two-sided limit used to give came from the complex continuation by way
        /// of an assembled <c>(+oo)^0</c>.
        /// </summary>
        [Theory]
        [InlineData("(1/x) ^ x", "1")]
        [InlineData("(1/x) ^ (x^2)", "1")]
        [InlineData("(1/x) ^ (1/ln(x))", "1/e")]
        public void TheOneSidedReadingsAreUnchanged(string expression, string expected) =>
            Assert.Equal(expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", 0, ApproachFrom.Right).Simplify().Evaled);
    }
}
