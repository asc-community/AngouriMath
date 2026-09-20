//
// Copyright (c) 2019-2026 Angouri.
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
    /// <c>c ^ (k * log(c, a)) = a ^ k</c>: the antilogarithm of <see cref="ExponentialOfALogarithmTest"/>
    /// with a multiplier on the logarithm. <c>c ^ z</c> is <c>exp(z * ln(c))</c> and <c>log(c, a)</c>
    /// is <c>ln(a) / ln(c)</c>, so the two <c>ln(c)</c> cancel and <c>exp(k * ln(a))</c> is left,
    /// which is what <c>a ^ k</c> means -- on the principal branch, for every complex <c>a</c> other
    /// than <c>0</c>. The integrator's exponential substitution writes its antiderivatives in this
    /// shape, <c>e ^ (-3/2 * ln(u))</c>, and nothing folded them.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/994"/>,
    /// <see href="https://github.com/asc-community/AngouriMath/issues/718"/>.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class PowerOfALogarithmMultipleTest
    {
        [Theory]
        [InlineData("e ^ (2 * ln(x))", "x ^ 2")]
        [InlineData("e ^ (ln(x) * 2)", "x ^ 2")]
        [InlineData("e ^ (-3/2 * ln(u))", "u ^ (-3/2)")]
        [InlineData("e ^ (k * ln(x + 1))", "(1 + x) ^ k")]
        [InlineData("2 ^ (k * log(2, x))", "x ^ k")]
        [InlineData("10 ^ (3 * log(10, x))", "x ^ 3")]
        [InlineData("pi ^ (k * log(pi, x))", "x ^ k")]
        public void TheMultiplierBecomesTheExponent(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>
        /// A symbolic base needs <c>ln(b)</c> non-zero, which a symbol cannot promise, so the
        /// logarithm's own condition travels with the answer.
        /// </summary>
        [Fact]
        public void ASymbolicBaseFoldsWhereTheLogarithmIsDefined()
            => Assert.Equal("x ^ k provided not a = 0 and not a = 1".ToEntity(), "a ^ (k * log(a, x))".ToEntity().Simplify());

        /// <summary>And the base it would be wrong for is still answered the way it was.</summary>
        [Fact]
        public void TheBaseOneCaseIsUnchanged()
            => Assert.Equal("NaN".ToEntity().Evaled, "1 ^ (2 * log(1, x))".ToEntity().Simplify().Evaled);

        /// <summary>
        /// The rewrite must not move a value anywhere the original had one, on the negative reals
        /// included, where the principal branch is what makes it true: <c>ln(-3)</c> is
        /// <c>ln(3) + i*pi</c>, and <c>e ^ (1/2 * (ln(3) + i*pi))</c> is <c>i * sqrt(3)</c>, which is
        /// the principal <c>(-3) ^ (1/2)</c>.
        /// </summary>
        [Theory]
        [InlineData("e ^ (2 * ln(x))", 3.0)]
        [InlineData("e ^ (2 * ln(x))", -3.0)]
        [InlineData("e ^ (1/2 * ln(x))", -3.0)]
        [InlineData("e ^ (-3/2 * ln(x))", 0.25)]
        [InlineData("e ^ (-3/2 * ln(x))", -1.5)]
        [InlineData("2 ^ (3 * log(2, x))", -4.0)]
        public void TheValueIsPreserved(string input, double at)
        {
            var original = input.ToEntity();
            var before = original.Substitute("x", at).Evaled;
            var after = original.Simplify().Substitute("x", at).Evaled;
            Assert.True(before is Entity.Number.Complex b && after is Entity.Number.Complex a
                && (b - a).Abs().EvalNumerical().RealPart.EDecimal.CompareTo(PeterO.Numbers.EDecimal.FromDouble(1e-9)) < 0,
                $"{input} at {at}: {before} became {after}");
        }
    }
}
