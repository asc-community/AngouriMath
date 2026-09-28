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
    /// A definite integral's value at a bound is the antiderivative's value there, and where that
    /// is undefined, its limit from inside the range, which is what an improper integral is.
    /// <c>-(x + 1) e^(-x)</c> at <c>+oo</c> is <c>-oo * 0</c>, so the integral of <c>x e^(-x)</c>
    /// over <c>[0, +oo)</c> was <c>NaN</c>; <c>x ln(x) - x</c> at 0 is <c>0 * -oo</c>, so the
    /// integral of <c>ln(x)</c> over <c>[0, 1]</c> was too.
    /// https://github.com/asc-community/AngouriMath/issues/1507
    /// </summary>
    [Trait("Area", "Calculus")]
    public sealed class ImproperIntegralTest
    {
        private static Entity Definite(string integrand, string from, string to)
            => MathS.Integral(integrand.ToEntity(), "x", from.ToEntity(), to.ToEntity()).Simplify();

        [Theory]
        [InlineData("x * e^(-x)", "0", "+oo", "1")]
        [InlineData("x^2 * e^(-x)", "0", "+oo", "2")]
        [InlineData("x^3 * e^(-x^2)", "0", "+oo", "1/2")]
        [InlineData("ln(x) / x^2", "1", "+oo", "1")]
        [InlineData("e^(-x) * sin(x)", "0", "+oo", "1/2")]
        [InlineData("x * e^x", "-oo", "0", "-1")]
        public void AnInfiniteBoundIsALimit(string integrand, string from, string to, string value)
            => Assert.Equal(value.ToEntity(), Definite(integrand, from, to));

        /// <summary>
        /// A finite bound is approached from the side the other bound is on, so the range may
        /// run either way.
        /// </summary>
        [Theory]
        [InlineData("ln(x)", "0", "1", "-1")]
        [InlineData("x * ln(x)", "0", "1", "-1/4")]
        [InlineData("ln(x)", "1", "0", "1")]
        [InlineData("x * e^(-x)", "+oo", "0", "-1")]
        // A bound written as a constant is placed by its value: the half-angle antiderivative's
        // tan(x/2) is undefined at pi.
        [InlineData("1/(2 + cos(x))", "0", "pi", "pi / sqrt(3)")]
        public void ABoundWhereTheAntiderivativeIsUndefinedIsALimit(string integrand, string from, string to, string value)
            => Assert.Equal(value.ToEntity().Simplify(), Definite(integrand, from, to));

        /// <summary>A divergent integral is <c>+oo</c> where the limit is.</summary>
        [Fact]
        public void ADivergentIntegralIsItsLimit() => Assert.Equal("+oo".ToEntity(), Definite("1/x^2", "0", "1"));

        /// <summary>
        /// And <c>NaN</c> where there is no limit: <c>-cos(x)</c> does not settle as <c>x</c> grows.
        /// </summary>
        [Fact]
        public void AnAntiderivativeWithoutALimitLeavesNaN() => Assert.Equal(MathS.NaN, Definite("sin(x)", "0", "+oo"));

        /// <summary>A bound the substitution answers is answered as it was.</summary>
        [Theory]
        [InlineData("e^(-x)", "0", "+oo", "1")]
        [InlineData("1/(x^2 + 1)", "-oo", "+oo", "pi")]
        [InlineData("x * e^(-x^2)", "0", "+oo", "1/2")]
        [InlineData("1/sqrt(x)", "0", "1", "2")]
        [InlineData("1/x", "1", "+oo", "+oo")]
        public void ABoundWithAValueKeepsIt(string integrand, string from, string to, string value)
            => Assert.Equal(value.ToEntity(), Definite(integrand, from, to));
    }
}
