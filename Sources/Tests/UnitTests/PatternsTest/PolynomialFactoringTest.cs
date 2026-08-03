//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.PatternsTest
{
    /// <summary>
    /// Factoring a polynomial in one variable into linear factors with whole roots.
    /// The factored form is offered to the simplifier as a candidate, so what these pin
    /// is both that it is produced and that it is preferred where it should be.
    /// </summary>
    public sealed class PolynomialFactoringTest
    {
        // https://github.com/asc-community/AngouriMath/issues/177
        [Theory]
        [InlineData("x ^ 2 + 2 * x + 1", "(1 + x) ^ 2")]
        [InlineData("x ^ 3 + 3 * x ^ 2 + 3 * x + 1", "(1 + x) ^ 3")]
        [InlineData("x ^ 3 - 6 * x ^ 2 + 11 * x - 6", "(x - 1) * (x - 2) * (x - 3)")]
        [InlineData("2 * x ^ 2 + 4 * x + 2", "2 * (1 + x) ^ 2")]
        [InlineData("x ^ 2 + 2 * x", "x * (2 + x)")]
        public void Issue177_PolynomialsFactor(string input, string expected) =>
            Assert.Equal(expected, input.ToEntity().Simplify().Stringize());

        // Whatever form comes out, it has to be the same polynomial.
        [Theory]
        [InlineData("x ^ 2 + 2 * x + 1")]
        [InlineData("x ^ 3 + 3 * x ^ 2 + 3 * x + 1")]
        [InlineData("x ^ 3 - 6 * x ^ 2 + 11 * x - 6")]
        [InlineData("2 * x ^ 2 + 4 * x + 2")]
        [InlineData("x ^ 2 + 2 * x")]
        [InlineData("x ^ 4 - 1")]
        [InlineData("6 * x ^ 2 - 5 * x + 1")]
        public void FactoringPreservesValue(string input)
        {
            var difference = (input.ToEntity() - input.ToEntity().Simplify()).Simplify();
            while (difference is Entity.Providedf(var inner, _)) difference = inner;
            Assert.Equal(Entity.Number.Integer.Create(0), difference);
        }

        // Irrational and complex roots must not be invented. Factoring through every root
        // would answer (x - sqrt(2)) * (x + sqrt(2)) and (x - i) * (x + i), which is not
        // what factoring these means.
        [Theory]
        [InlineData("x ^ 2 - 2")]
        [InlineData("x ^ 2 + 1")]
        [InlineData("x ^ 2 + x + 1")]
        public void PolynomialsWithoutRationalRootsAreLeftAlone(string input) =>
            Assert.DoesNotContain("sqrt", input.ToEntity().Simplify().Stringize());

        // A polynomial that only splits part of the way is left as it was: a partly
        // factored answer is not obviously better than the sum it came from.
        [Fact]
        public void PartialSplitsAreNotOffered() =>
            Assert.Equal("x ^ 3 - x ^ 2 - 2".ToEntity().Expand().Simplify().Stringize(),
                "x ^ 3 - x ^ 2 - 2".ToEntity().Simplify().Stringize());

        // Fractional roots turn up mostly in the output of calculus, where the expanded
        // form is the conventional one.
        [Fact]
        public void AntiderivativesKeepTheirExpandedForm() =>
            Assert.Equal("x ^ 3 / 3 + x ^ 2 / 2", "x ^ 3 / 3 + x ^ 2 / 2".ToEntity().Simplify().Stringize());

        // Multivariate expressions go to the term-collecting rules, not here.
        [Fact]
        public void MultivariatePolynomialsAreNotAffected() =>
            Assert.Equal("4 * (x ^ 2 - y ^ 2)", "4 * x ^ 2 - 4 * y ^ 2".ToEntity().Simplify().Stringize());
    }
}
