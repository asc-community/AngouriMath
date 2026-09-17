//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A <c>provided</c> condition the expression states on its own is dropped: <c>x^x provided
    /// not x = 0</c> is <c>x^x</c>, since <c>0^0</c> has no value whether or not a condition says
    /// so. Only for an indeterminate form -- <c>0^0</c>, <c>0/0</c> -- and on the expression as it
    /// stands after simplification, so that <c>x/x</c>, which becomes <c>1</c>, keeps the
    /// condition it can no longer state. A pole is a value: <c>1/x provided not x = 0</c> has no
    /// value at zero where <c>1/x</c> has an infinite one, and keeps its condition.
    /// https://github.com/asc-community/AngouriMath/issues/1394
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ProvidedImpliedByTheExpressionTest
    {
        [Theory]
        [InlineData("(1 + ln(x)) * x^x provided not x = 0", "(1 + ln(x)) * x^x")]
        [InlineData("x^x provided not x = 0", "x^x")]
        [InlineData("(x^2)^(x * y) provided not x = 0", "(x^2)^(x * y)")]
        [InlineData("sin(x)/x + x^x provided not x = 0 and not y = 0", "sin(x)/x + x^x provided not y = 0")]
        public void TheConditionTheExpressionStatesIsDropped(string input, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), input.ToEntity().Simplify());

        /// <summary>
        /// The derivatives the comparison page shows: <c>x^x</c>'s carried the condition twice,
        /// <c>ln(x)/x</c>'s states a pole and the condition removes a value there, so it stays.
        /// </summary>
        [Theory]
        [InlineData("ln(x) / x", "(1 - ln(x)) / x^2 provided not x = 0")]
        [InlineData("x^x", "(1 + ln(x)) * x^x")]
        public void ADerivativeStatesItsOwnDomain(string function, string derivative)
            => Assert.Equal(derivative.ToEntity().Simplify(), function.ToEntity().Differentiate("x").Simplify());

        /// <summary>
        /// A condition the expression does not state stays: the one on a quotient that has
        /// cancelled, one about a symbol the expression no longer holds, one about a factor
        /// beside the one the quotient is by -- and every pole, since a pole is a value the
        /// condition takes away.
        /// </summary>
        [Theory]
        [InlineData("x / x provided not x = 0")]
        [InlineData("1 / (x^2 + 1) provided not x = 0")]
        [InlineData("y / x provided not a * x = 0")]
        [InlineData("1/x provided not x = 0")]
        [InlineData("x^(-2) provided not x = 0")]
        [InlineData("ln(x) provided not x = 0")]
        [InlineData("(1 - ln(x)) / x^2 provided not x = 0")]
        [InlineData("1/(x^2 + x) provided not x = 0 and not x + 1 = 0")]
        public void AConditionTheExpressionDoesNotStateStays(string input)
            => Assert.Contains("provided", input.ToEntity().Simplify().Stringize());
    }
}
