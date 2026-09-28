//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// A definite integral whose antiderivative breaks between the bounds is taken piece by
    /// piece, each piece with its limit at the break from its own side. The antiderivative
    /// <c>-1/x</c> taken at the bounds alone made the integral of the positive <c>1/x^2</c> over
    /// <c>[-1, 1]</c> equal to <c>-2</c>.
    /// https://github.com/asc-community/AngouriMath/issues/1508
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class IntegralAcrossABreakTest
    {
        private static Entity Definite(string integrand, string from, string to)
            => MathS.Integral(integrand.ToEntity(), "x", from.ToEntity(), to.ToEntity()).Simplify();

        /// <summary>A pole the integral diverges across is <c>+oo</c> or <c>-oo</c>, with the range's direction.</summary>
        [Theory]
        [InlineData("1/x^2", "-1", "1", "+oo")]
        [InlineData("1/(x - 2)^2", "0", "3", "+oo")]
        [InlineData("tan(x)^2", "0", "3", "+oo")]
        [InlineData("sec(x)^2", "0", "pi", "+oo")]
        [InlineData("1/x^2", "1", "-1", "-oo")]
        public void APoleInsideTheRangeDiverges(string integrand, string from, string to, string value)
            => Assert.Equal(value.ToEntity(), Definite(integrand, from, to));

        /// <summary>
        /// Where the pieces diverge in opposite directions the integral has no value, and not
        /// the finite one the antiderivative at the bounds gave.
        /// </summary>
        [Theory]
        [InlineData("1/x", "-1", "1")]
        [InlineData("1/(x - 2)", "0", "3")]
        [InlineData("tan(x)", "0", "3")]
        [InlineData("x/(x^2 - 1)", "0", "2")]
        [InlineData("1/(x^2 - 4)", "-3", "3")]
        public void OppositeDivergencesHaveNoValue(string integrand, string from, string to)
            => Assert.Equal(MathS.NaN, Definite(integrand, from, to));

        /// <summary>
        /// A singularity the integral converges across is joined from both sides: <c>x ln|x| - x</c>
        /// tends to 0 at 0 from either one.
        /// </summary>
        [Fact]
        public void AnIntegrableSingularityKeepsItsValue() => Assert.Equal(-2, Definite("ln(abs(x))", "-1", "1"));

        /// <summary>
        /// A jump in the antiderivative where the integrand is continuous is not counted.
        /// <c>1/(2 + cos(x))</c> over <c>[0, 2 pi]</c>, through the half-angle antiderivative's
        /// <c>tan(x/2)</c>, which jumps from <c>+oo</c> to <c>-oo</c> at <c>pi</c>, was <c>0</c>.
        /// </summary>
        [Fact]
        public void AJumpInTheAntiderivativeIsNotCounted()
            => Assert.Equal("2 * pi / sqrt(3)".ToEntity().Simplify(), Definite("1/(2 + cos(x))", "0", "2 * pi"));

        /// <summary>A break outside the range, or at a bound, changes nothing.</summary>
        [Theory]
        [InlineData("1/x^2", "1", "2", "1/2")]
        [InlineData("1/(x^2 + 1)", "-1", "1", "pi / 2")]
        [InlineData("tan(x)", "0", "1", "-ln(cos(1))")]
        public void ABreakOutsideTheRangeChangesNothing(string integrand, string from, string to, string value)
            => Assert.Equal(value.ToEntity().Simplify(), Definite(integrand, from, to));
    }
}
