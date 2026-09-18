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
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core.Sets
{
    /// <summary>
    /// <c>ZZ*</c> is <c>{0, 1, 2, ...}</c> and <c>ZZ+</c> is <c>{1, 2, 3, ...}</c>, MathWorld's
    /// spellings (https://mathworld.wolfram.com/N.html); there is no set named after the natural
    /// numbers, because the name means either of these depending on the author.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class NonNegativeAndPositiveIntegersTest
    {
        [Theory]
        [InlineData("0 in ZZ*", "True")]
        [InlineData("0 in ZZ+", "False")]
        [InlineData("7 in ZZ*", "True")]
        [InlineData("7 in ZZ+", "True")]
        [InlineData("-7 in ZZ*", "False")]
        [InlineData("-7 in ZZ+", "False")]
        [InlineData("1/2 in ZZ*", "False")]
        [InlineData("1/2 in ZZ+", "False")]
        [InlineData("2.5 in ZZ+", "False")]
        [InlineData("i in ZZ*", "False")]
        [InlineData("10^20 in ZZ+", "True")]
        // Arithmetic is evaluated before membership is decided.
        [InlineData("3 - 3 in ZZ*", "True")]
        [InlineData("3 - 3 in ZZ+", "False")]
        // A closed non-leaf is a member of none of the special sets, decidedly.
        [InlineData("{ 1 } in ZZ*", "False")]
        public void MembershipIsDecided(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        [Theory]
        [InlineData("x in ZZ*")]
        [InlineData("x in ZZ+")]
        [InlineData("x - 1 in ZZ+")]
        public void MembershipOfASymbolIsLeftAsWritten(string statement)
            => Assert.Equal(statement.ToEntity(), statement.ToEntity().Evaled);

        [Theory]
        [InlineData("ZZ*", "ZZ*")]
        [InlineData("ZZ+", "ZZ+")]
        [InlineData("x in ZZ*", "x in ZZ*")]
        [InlineData("{ k in ZZ+ : k > 3 }", "{ k in ZZ+ : k > 3 }")]
        public void PrintsAsWrittenAndReadsBack(string input, string printed)
        {
            var entity = input.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
        }

        [Fact]
        public void TheThreeSetsAreDistinctAndTheEntryPointsAgreeWithTheParser()
        {
            Assert.Equal(MathS.Sets.NonNegativeIntegers, "ZZ*".ToEntity());
            Assert.Equal(MathS.Sets.PositiveIntegers, "ZZ+".ToEntity());
            Assert.NotEqual(MathS.Sets.Z, MathS.Sets.NonNegativeIntegers);
            Assert.NotEqual(MathS.Sets.NonNegativeIntegers, MathS.Sets.PositiveIntegers);
            Assert.IsType<Set.SpecialSet.NonNegativeIntegers>(Set.SpecialSet.Create("NonNegativeIntegers"));
            Assert.IsType<Set.SpecialSet.PositiveIntegers>(Set.SpecialSet.Create("PositiveIntegers"));
        }

        [Theory]
        [InlineData("ZZ*", @"\mathbb{Z}^{*}")]
        [InlineData("ZZ+", @"\mathbb{Z}^{+}")]
        [InlineData("ZZ", @"\mathbb{Z}")]
        public void LatexUsesMathWorldsSuperscripts(string input, string latex)
            => Assert.Equal(latex, input.ToEntity().Latexize());

        [Theory]
        [InlineData("ZZ*", "sympy.S.Naturals0")]
        [InlineData("ZZ+", "sympy.S.Naturals")]
        public void SymPyHasBothSets(string input, string sympy)
            => Assert.Contains(sympy, MathS.ToSympyCode(input.ToEntity()));

        /// <summary>
        /// Neither set is a codomain: the <see cref="AngouriMath.Core.Domain"/> enum has no
        /// member for them, and reading <c>domain(x, ZZ*)</c> as <c>domain(x, ZZ)</c> would
        /// drop the sign, so the annotation is refused rather than widened.
        /// </summary>
        [Theory]
        [InlineData("domain(x, ZZ*)")]
        [InlineData("domain(x, ZZ+)")]
        public void NeitherIsACodomain(string input)
            => Assert.Throws<InvalidArgumentParseException>(() => input.ToEntity());

        /// <summary>
        /// The tokens win over <c>ZZ</c> followed by an operator, as the longest match does:
        /// <c>ZZ*2</c> is now the set <c>ZZ*</c> followed by a stray <c>2</c>, which does not
        /// parse — a product of a set and a number never meant anything, so nothing that
        /// meant something is lost. With a space the operator reading is unchanged.
        /// </summary>
        [Fact]
        public void TheTokenTakesTheStarAndThePlus()
        {
            Assert.Throws<UnhandledParseException>(() => "ZZ*2".ToEntity());
            Assert.Equal(MathS.Sets.Z * 2, "ZZ * 2".ToEntity());
        }
    }
}
