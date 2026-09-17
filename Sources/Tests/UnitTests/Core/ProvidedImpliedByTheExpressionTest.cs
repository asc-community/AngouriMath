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
    /// A <c>provided</c> condition the expression states on its own is dropped: <c>1/x provided
    /// not x = 0</c> is <c>1/x</c>, since the quotient has no value at zero whether or not a
    /// condition says so. Only where the expression proves it -- a quotient, a negative power, a
    /// logarithm, <c>0^0</c> -- and on the expression as it stands after simplification, so that
    /// <c>x/x</c>, which becomes <c>1</c>, keeps the condition it can no longer state.
    /// https://github.com/asc-community/AngouriMath/issues/1394
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ProvidedImpliedByTheExpressionTest
    {
        [Theory]
        [InlineData("(1 - ln(x)) / x^2 provided not x = 0", "(1 - ln(x)) / x^2")]
        [InlineData("(1 + ln(x)) * x^x provided not x = 0", "(1 + ln(x)) * x^x")]
        [InlineData("1/x provided not x = 0", "1/x")]
        [InlineData("x^(-2) provided not x = 0", "1/x^2")]
        [InlineData("ln(x) provided not x = 0", "ln(x)")]
        [InlineData("1/(x * y) provided not x = 0 and not y = 0", "1/(x * y)")]
        [InlineData("1/(x^2 + x) provided not x = 0 and not x + 1 = 0", "1/(x^2 + x)")]
        [InlineData("1/(x^2 + x) provided not x = 0 and not y = 0", "1/(x^2 + x) provided not y = 0")]
        public void TheConditionTheExpressionStatesIsDropped(string input, string expected)
            => Assert.Equal(expected.ToEntity().Simplify(), input.ToEntity().Simplify());

        /// <summary>
        /// The derivatives the comparison page shows, which carried the condition twice.
        /// </summary>
        [Theory]
        [InlineData("ln(x) / x", "(1 - ln(x)) / x^2")]
        [InlineData("x^x", "(1 + ln(x)) * x^x")]
        public void ADerivativeStatesItsOwnDomain(string function, string derivative)
        {
            var got = function.ToEntity().Differentiate("x").Simplify();
            Assert.DoesNotContain("provided", got.Stringize());
            Assert.Equal(derivative.ToEntity().Simplify(), got);
        }

        /// <summary>
        /// A condition the expression does not state stays: the one on a quotient that has
        /// cancelled, and one about a symbol the expression no longer holds, and one about a
        /// factor beside the one the quotient is by.
        /// </summary>
        [Theory]
        [InlineData("x / x provided not x = 0")]
        [InlineData("1 / (x^2 + 1) provided not x = 0")]
        [InlineData("y / x provided not a * x = 0")]
        public void AConditionTheExpressionDoesNotStateStays(string input)
            => Assert.Contains("provided", input.ToEntity().Simplify().Stringize());
    }
}
