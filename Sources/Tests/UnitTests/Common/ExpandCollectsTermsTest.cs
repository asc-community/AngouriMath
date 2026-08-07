//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// Expand adds up the terms it produces that differ only by a numeric factor.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class ExpandCollectsTermsTest
    {
        // https://github.com/asc-community/AngouriMath/issues/164
        // Reported as "cannot be expanded": multiplying it out gave sixteen terms, of
        // which only five are distinct.
        [Fact]
        public void Issue164_RepeatedFactorExpands() =>
            Assert.Equal("1 + 4 * x + 6 * x ^ 2 + 4 * x ^ 3 + x ^ 4",
                "(x+1)^2*(x+2-1)^2".ToEntity().Expand().Stringize());

        // The maintainer asked for this one on the issue as well.
        [Fact]
        public void Issue164_CubeOfAProductExpands() =>
            Assert.Equal("8 + (-36) * x + 66 * x ^ 2 + (-63) * x ^ 3 + 33 * x ^ 4 + (-9) * x ^ 5 + x ^ 6",
                "((x-1)*(x-2))^3".ToEntity().Expand().Stringize());

        [Theory]
        [InlineData("(x + 1) ^ 2", "1 + 2 * x + x ^ 2")]
        [InlineData("(x + 1) ^ 3", "1 + 3 * x + 3 * x ^ 2 + x ^ 3")]
        [InlineData("x * (x + 1)", "x ^ 2 + x")]
        public void OrdinaryProductsCollect(string input, string expected) =>
            Assert.Equal(expected, input.ToEntity().Expand().Stringize());

        // Nothing may be collected across different variables.
        [Fact]
        public void UnlikeTermsAreLeftApart() =>
            Assert.Equal("a * c + a * d + b * c + b * d", "(a+b)*(c+d)".ToEntity().Expand().Stringize());

        // Whatever comes out has to be the same expression.
        [Theory]
        [InlineData("(x+1)^2*(x+2-1)^2")]
        [InlineData("((x-1)*(x-2))^3")]
        [InlineData("(x + 1) ^ 3")]
        [InlineData("(x + y) ^ 2")]
        [InlineData("(x + 1) ^ 2 * (x + 2)")]
        public void ExpansionPreservesValue(string input)
        {
            var before = input.ToEntity();
            var after = before.Expand();
            foreach (var point in new[] { 0.37, 1.41, -0.8 })
            {
                var a = before.Substitute("x", point).Substitute("y", point + 0.5).EvalNumerical();
                var b = after.Substitute("x", point).Substitute("y", point + 0.5).EvalNumerical();
                Assert.Equal(a.RealPart.EDecimal.ToDouble(), b.RealPart.EDecimal.ToDouble(), 9);
            }
        }

        // Terms whose coefficients cancel are still written out, because the monomial may
        // carry a domain condition that dropping the term would throw away: this is zero
        // only where x is not.
        [Fact]
        public void CancellingTermsKeepTheirDomainCondition() =>
            Assert.Equal("0 provided not x = 0".ToEntity(),
                "(4a - 2) / (2x) + (1 - 2a) / x".ToEntity().Simplify());
    }
}
