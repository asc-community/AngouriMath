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
    /// <c>e ^ ln(a) = a</c>. The identity was already written for a numeric base — <c>2 ^ log(2, x)</c>
    /// has always simplified to <c>x</c> — and could not reach <c>e</c>, because <c>ln(a)</c> is stored
    /// as <c>log(e, a)</c> and <c>e</c> is a <c>Constant</c> rather than a <c>Number</c>, so the
    /// pattern's <c>Any&lt;Number&gt;</c> never bound it.
    /// <see href="https://github.com/asc-community/AngouriMath/issues/1138"/>,
    /// <see href="https://github.com/asc-community/AngouriMath/issues/994"/>.
    /// </summary>
    [Trait("Area", "Common")]
    public sealed class ExponentialOfALogarithmTest
    {
        [Theory]
        [InlineData("e ^ ln(x)", "x")]
        [InlineData("e ^ ln(2 * x)", "2 * x")]
        [InlineData("e ^ ln(x + 1)", "1 + x")]
        [InlineData("e ^ ln(x ^ 2)", "x ^ 2")]
        [InlineData("e ^ ln(sin(x))", "sin(x)")]
        public void TheExponentialUndoesTheLogarithm(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>The numeric base this was modelled on, which already worked.</summary>
        [Theory]
        [InlineData("2 ^ log(2, x)", "x")]
        [InlineData("10 ^ log(10, x + 1)", "1 + x")]
        public void ANumericBaseStillDoes(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>
        /// A symbolic base stays refused, and must: <c>b ^ log(b, a) = a</c> needs <c>ln(b)</c> to be
        /// non-zero, which is not decidable for a symbol, and at <c>b = 1</c> the left side is
        /// <c>1</c> rather than <c>a</c>. <c>e</c> is decidably neither <c>0</c> nor <c>1</c>, which
        /// is the whole of what the new rule relies on.
        /// </summary>
        [Fact]
        public void ASymbolicBaseIsNotFolded()
        {
            var expr = "a ^ log(a, x)".ToEntity();
            Assert.Equal(expr, expr.Simplify());
        }

        /// <summary>And the base it would be wrong for is still answered the way it was.</summary>
        [Fact]
        public void TheBaseOneCaseIsUnchanged()
            => Assert.Equal("NaN".ToEntity().Evaled, "1 ^ log(1, x)".ToEntity().Simplify().Evaled);

        /// <summary>
        /// The rewrite must not move a value anywhere the original had one, and in particular not on
        /// the negative reals, where it is the principal branch that makes it true:
        /// <c>ln(-3)</c> is <c>ln(3) + i*pi</c>, and <c>e ^ (ln(3) + i*pi)</c> is <c>-3</c>.
        /// </summary>
        [Theory]
        [InlineData("e ^ ln(x)", 3.0)]
        [InlineData("e ^ ln(x)", -3.0)]
        [InlineData("e ^ ln(x)", 0.25)]
        [InlineData("e ^ ln(2 * x)", -1.5)]
        [InlineData("e ^ ln(x + 1)", -4.0)]
        public void TheValueIsPreserved(string input, double at)
        {
            var original = input.ToEntity();
            var before = original.Substitute("x", at).Evaled;
            var after = original.Simplify().Substitute("x", at).Evaled;
            Assert.Equal(before, after);
        }

        /// <summary>
        /// At <c>a = 0</c> no definedness moves either, which is what the contract's O4 asks: this
        /// library reads <c>ln(0)</c> as <c>-oo</c> and <c>e ^ (-oo)</c> as <c>0</c>, so both sides
        /// are <c>0</c> rather than one being undefined.
        /// </summary>
        [Fact]
        public void NothingBecomesDefinedThatWasNot()
            => Assert.Equal("0".ToEntity().Evaled, "e ^ ln(0)".ToEntity().Simplify().Evaled);
    }
}
