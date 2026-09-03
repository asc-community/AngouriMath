//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Common
{
    /// <summary>
    /// The arrow spelling of a lambda — <c>a => a + 3</c> — which
    /// <a href="https://github.com/asc-community/AngouriMath/issues/495">#495</a> specifies and
    /// which was a parse error.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <c>Lambda</c> and <c>Application</c> nodes, beta reduction and currying were all
    /// already there; what that issue's plan describes and the library did not have is the
    /// syntax. This is the half of it that costs nothing — <c>a => a + 3</c> raised
    /// <c>UnhandledParseException</c>, so there is no reading of it to break. The juxtaposition
    /// half (<c>f a b</c>, <c>sin x</c>) is a change to what implicit multiplication means and is
    /// the same decision as
    /// <a href="https://github.com/asc-community/AngouriMath/issues/286">#286</a>.
    /// </para>
    /// <para>
    /// Every case is asserted against its <c>lambda(...)</c> spelling rather than against a
    /// printed form, because the point is that the two produce the same entity.
    /// </para>
    /// </remarks>
    [Trait("Area", "Common")]
    public sealed class LambdaArrowSyntaxTest
    {
        /// <summary>The arrow and the call agree, at every arity the plan gives.</summary>
        [Theory]
        [InlineData("a => a + 3", "lambda(a, a + 3)")]
        [InlineData("a => a + b", "lambda(a, a + b)")]
        [InlineData("a b => a + b", "lambda(a, b, a + b)")]
        [InlineData("a b c => 3", "lambda(a, b, c, 3)")]
        [InlineData("x => x ^ 2", "lambda(x, x ^ 2)")]
        public void TheArrowAndTheCallAgree(string arrow, string call)
            => Assert.Equal(call.ToEntity(), arrow.ToEntity());

        /// <summary>
        /// Several parameters are the curried form, which is what the plan says: <c>a b => a + b</c>
        /// is <c>a => b => a + b</c>.
        /// </summary>
        [Fact]
        public void SeveralParametersAreCurried()
            => Assert.Equal("a => b => a + b".ToEntity(), "a b => a + b".ToEntity());

        /// <summary>
        /// The body runs to the end rather than stopping at the first operator, so the arrow is
        /// looser than everything else.
        /// </summary>
        [Theory]
        [InlineData("a => a + 3", "lambda(a, a + 3)")]
        [InlineData("a => a > 3", "lambda(a, a > 3)")]
        [InlineData("a => a provided b", "lambda(a, a provided b)")]
        [InlineData("a => a implies b", "lambda(a, a implies b)")]
        public void TheBodyRunsToTheEnd(string arrow, string call)
            => Assert.Equal(call.ToEntity(), arrow.ToEntity());

        /// <summary>It reduces, which is the point of writing one.</summary>
        [Theory]
        [InlineData("apply(a => a + 3, 5)", "8")]
        [InlineData("apply(x => x ^ 2, 3)", "9")]
        [InlineData("apply(apply(a b => a + b, 1), 2)", "3")]
        [InlineData("apply(apply(apply(a b c => a + b + c, 1), 2), 3)", "6")]
        public void AnArrowLambdaReduces(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        /// <summary>
        /// An index called <c>i</c> is the name and not the imaginary unit, which the arrow gets
        /// for free by reading its parameters exactly as <c>lambda(...)</c> reads them — through
        /// <c>Binding</c>. <c>i</c> lexes as a number, so it could never have arrived as a name
        /// token at all. <see href="https://github.com/asc-community/AngouriMath/issues/976"/>
        /// </summary>
        [Fact]
        public void AnIndexCalledIIsTheName()
            => Assert.Equal("lambda(i, i + 1)".ToEntity(), "i => i + 1".ToEntity());

        /// <summary>
        /// A parameter has to be a name. The plan writes <c>a 3 => 3</c> down as invalid, and it
        /// is refused rather than read as a lambda over <c>a</c> and <c>3</c>.
        /// </summary>
        [Theory]
        [InlineData("a 3 => 3")]
        [InlineData("2 => 3")]
        [InlineData("x + 1 => 2")]
        [InlineData("sin(x) => 2")]
        public void AParameterThatIsNotANameIsRefused(string input)
            => Assert.Throws<InvalidArgumentParseException>(() => input.ToEntity());

        /// <summary>
        /// The printed form is unchanged: a lambda prints as its call, and what prints is what
        /// reads back. The arrow is an input spelling only.
        /// </summary>
        [Theory]
        [InlineData("a => a + 3")]
        [InlineData("a b => a + b")]
        public void TheArrowIsNotPrinted(string input)
        {
            var printed = input.ToEntity().Stringize();
            Assert.DoesNotContain("=>", printed);
            Assert.Equal(input.ToEntity(), printed.ToEntity());
        }

        /// <summary>
        /// What the new token must not have disturbed. <c>=</c> and <c>&gt;</c> next to each other
        /// were a parse error before, so nothing valid changed meaning — but the comparisons that
        /// share their characters are worth pinning.
        /// </summary>
        [Theory]
        [InlineData("x >= 3", "x >= 3")]
        [InlineData("x = 3", "x = 3")]
        [InlineData("x <= 3", "x <= 3")]
        [InlineData("x > 3", "x > 3")]
        [InlineData("a implies b", "a implies b")]
        [InlineData("a -> b", "a implies b")]
        [InlineData("a b", "a * b")]
        [InlineData("a * b", "a * b")]
        [InlineData("x2", "x ^ 2")]
        public void TheNeighbouringOperatorsAreUnchanged(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity());
    }
}
