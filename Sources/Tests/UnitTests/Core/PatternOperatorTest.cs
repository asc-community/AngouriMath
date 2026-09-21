//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// The pattern operator, <c>...</c>: between shown terms it names the progression the shown
    /// terms determine, and the term after it names where it stops. The small half of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1437">#1437</a>: ranges,
    /// one-sided progressions, sums and products of an arithmetic progression of whole numbers,
    /// built from what exists -- <c>ZZ /\ [a; z]</c>, a residue class cut by an interval,
    /// <c>sum</c>, <c>product</c>.
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class PatternOperatorTest
    {
        private static Entity Evaluated(string expression) => expression.ToEntity().Evaled;

        /// <summary>A range: listed when the ends are numbers, <c>ZZ /\ [a; z]</c> when one is a symbol.</summary>
        [Theory]
        [InlineData("{1, 2, ..., 10}", "{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}")]
        [InlineData("{1, ..., 5}", "{1, 2, 3, 4, 5}")]
        [InlineData("{1, 2, ..., 9, 10}", "{1, 2, 3, 4, 5, 6, 7, 8, 9, 10}")]
        [InlineData("{1, 2, …, 4}", "{1, 2, 3, 4}")]
        [InlineData("{2, 4, ..., 20}", "{2, 4, 6, 8, 10, 12, 14, 16, 18, 20}")]
        [InlineData("{10, 8, ..., 0}", "{0, 2, 4, 6, 8, 10}")]
        [InlineData("{1, 2, ..., n}", "ZZ /\\ [1; n]")]
        [InlineData("{k - 2, ..., k + 2}", "ZZ /\\ [k - 2; k + 2]")]
        [InlineData("{2, 4, ..., 2 n}", "{ x in ZZ : x = 0 (mod 2) } /\\ [2; 2 n]")]
        public void ARangeIsTheProgressionShown(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, Evaluated(expression));

        /// <summary>One-sided: a residue class cut by a ray, which answers membership.</summary>
        [Theory]
        [InlineData("7 in {1, 3, 5, ...}", "True")]
        [InlineData("8 in {1, 3, 5, ...}", "False")]
        [InlineData("-1 in {1, 3, 5, ...}", "False")]
        [InlineData("15 in {5, 10, 15, ...}", "True")]
        [InlineData("-3 in {..., -1, 0}", "True")]
        [InlineData("1 in {..., -1, 0}", "False")]
        [InlineData("6 in {..., 4, 2}", "True")]
        [InlineData("0 in {10, 8, ...}", "True")]
        [InlineData("12 in {10, 8, ...}", "False")]
        public void AOneSidedProgressionAnswersMembership(string expression, string expected)
            => Assert.Equal(expected.ToEntity(), Evaluated(expression));

        /// <summary>Sums and products: <c>1 + 2 + ... + n</c> is <c>sum(k, k, 1, n)</c>, and Gauss's row evaluates.</summary>
        [Theory]
        [InlineData("1 + 2 + ... + 100", "5050")]
        [InlineData("1 * 2 * ... * 5", "120")]
        [InlineData("2 + 4 + ... + 20", "110")]
        [InlineData("1 + 2 + ... + n", "sum(k, k, 1, n)")]
        [InlineData("1 * 2 * ... * n", "product(k, k, 1, n)")]
        public void ASumOrProductOfTheProgression(string expression, string expected)
        {
            var parsed = expression.ToEntity();
            var wanted = expected.ToEntity();
            Assert.True(parsed == wanted || parsed.Evaled == wanted.Evaled, $"{expression} parsed as {parsed}, not {expected}");
        }

        /// <summary>The index under a binder is a range too, the book's <c>∀ i ∈ {1, …, n}</c>.</summary>
        [Fact]
        public void AQuantifierRangesOverAPattern()
        {
            Assert.Equal(Boolean.True, Evaluated("forall k in {1, 2, ..., 10} : k^2 <= 100"));
            Assert.Equal(Boolean.False, Evaluated("forall k in {1, 2, ..., 10} : k^2 < 100"));
            Assert.Equal("{-1, 0, 1, 2, 3, 4, 5}".ToEntity().Evaled, Evaluated("union({k - 2, ..., k + 2}, k in {1, 2, 3})"));
        }

        /// <summary>
        /// What is refused: terms that show no single step, a progression that is not of whole
        /// numbers, dots with nothing to determine them, and dots twice.
        /// </summary>
        [Theory]
        [InlineData("{1, 4, 9, ..., n}")]
        [InlineData("{1/2, 1, ..., n}")]
        [InlineData("{..., n}")]
        [InlineData("{1, 1, ..., n}")]
        [InlineData("{1, ..., 5, ..., 10}")]
        [InlineData("1 + ... + n")]
        [InlineData("1 + 2 + ...")]
        public void WhatIsNotAProgressionIsRefused(string expression)
            => Assert.Throws<InvalidArgumentParseException>(() => expression.ToEntity());

        /// <summary>A listed set and a plain sum are as they were.</summary>
        [Fact]
        public void NothingChangesWithoutDots()
        {
            Assert.Equal("{ 1, 2, 3 }".ToEntity(), "{1, 2, 3}".ToEntity());
            Assert.Equal("6".ToEntity(), Evaluated("1 + 2 + 3"));
            Assert.Equal("x * 2.5".ToEntity(), "x*2.5".ToEntity());
            Assert.Equal("{}".ToEntity(), "{}".ToEntity());
        }
    }
}
