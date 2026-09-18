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
        /// The seven sets nest, <c>ZZ+ ⊂ ZZ* ⊂ ZZ ⊂ QQ ⊂ RR ⊂ CC</c> with <c>BB</c> beside them, and
        /// the set operators answer accordingly instead of leaving the expression as written.
        /// </summary>
        [Theory]
        [InlineData("ZZ* unite ZZ+", "ZZ*")]
        [InlineData("ZZ+ unite ZZ", "ZZ")]
        [InlineData("ZZ unite QQ", "QQ")]
        [InlineData("CC unite ZZ*", "CC")]
        [InlineData("RR unite RR", "RR")]
        [InlineData("ZZ* intersect ZZ+", "ZZ+")]
        [InlineData("QQ intersect ZZ*", "ZZ*")]
        [InlineData("RR intersect CC", "RR")]
        [InlineData("BB intersect ZZ", "{}")]
        [InlineData("ZZ* \\ ZZ+", "{ 0 }")]
        [InlineData("ZZ \\ ZZ*", "{ x in ZZ : x < 0 }")]
        [InlineData("ZZ \\ ZZ+", "{ x in ZZ : x <= 0 }")]
        [InlineData("QQ \\ ZZ", "{ x in QQ : not x in ZZ }")]
        [InlineData("RR \\ QQ", "{ x in RR : not x in QQ }")]
        [InlineData("ZZ+ \\ ZZ", "{}")]
        [InlineData("ZZ \\ ZZ", "{}")]
        [InlineData("BB \\ ZZ", "BB")]
        [InlineData("ZZ \\ BB", "ZZ")]
        public void TheSetsNest(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Evaled);

        /// <summary>
        /// A set builder written with its membership in the name position keeps the name as
        /// its bound variable and the membership as the first conjunct, so that a value can be
        /// substituted for the name: on 2.5.0 the name position held `x in ZZ` itself and
        /// `-3 in { x in ZZ : x < 0 }` was never decided.
        /// </summary>
        [Fact]
        public void AMembershipInTheNamePositionIsAConjunct()
        {
            var written = "{ x in ZZ : x < 0 }".ToEntity();
            var spelledOut = "{ x : x in ZZ and x < 0 }".ToEntity();
            Assert.Equal(spelledOut, written);
            Assert.Equal("{ x in ZZ : x < 0 }", written.ToString());
            Assert.Equal("{ x in ZZ : x < 0 }", spelledOut.ToString());
            Assert.Equal(@"\left\{ x \in \mathbb{Z} : x < 0 \right\}", written.Latexize());
            Assert.Equal(Boolean.True, "-3 in { x in ZZ : x < 0 }".ToEntity().Evaled);
            Assert.Equal(Boolean.False, "1/2 in { x in ZZ : x < 0 }".ToEntity().Evaled);
            // With nothing else asked of x, the set builder over ZZ is ZZ.
            Assert.Equal("ZZ", "{ x in ZZ : True }".ToEntity().Simplify().ToString());
        }

        [Theory]
        [InlineData("BB unite ZZ")]
        public void BooleansAndNumbersHaveNoNamedUnion(string input)
            => Assert.Equal(input.ToEntity(), input.ToEntity().Evaled);

        [Theory]
        [InlineData("-3 in ZZ \\ ZZ*", "True")]
        [InlineData("0 in ZZ \\ ZZ*", "False")]
        [InlineData("0 in ZZ \\ ZZ+", "True")]
        [InlineData("1/2 in QQ \\ ZZ", "True")]
        [InlineData("2 in QQ \\ ZZ", "False")]
        [InlineData("0 in ZZ* \\ ZZ+", "True")]
        public void AndTheDifferencesDecideMembership(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        /// <summary>
        /// Every special set is a codomain a node can be declared over, these two included:
        /// they are the <see cref="AngouriMath.Core.Domain"/> members below <c>Integer</c>, and
        /// a value outside them is <c>NaN</c>.
        /// </summary>
        [Theory]
        [InlineData("domain(x, ZZ*)", AngouriMath.Core.Domain.NonNegativeInteger)]
        [InlineData("domain(x, ZZ+)", AngouriMath.Core.Domain.PositiveInteger)]
        public void BothAreCodomains(string input, AngouriMath.Core.Domain codomain)
        {
            var entity = input.ToEntity();
            Assert.Equal(codomain, entity.Codomain);
            Assert.Equal(input, entity.ToString());
            Assert.Equal(entity, entity.ToString().ToEntity());
        }

        // The annotated node is the one containing x, as in the ZZ tests of Domains.cs: an
        // annotated variable is a different node from the bare one and is not what a
        // substitution for `x` reaches.
        [Theory]
        [InlineData("domain(2 * x, ZZ+)", "3", "6")]
        [InlineData("domain(2 * x, ZZ+)", "0", "NaN")]
        [InlineData("domain(x / 2, ZZ*)", "0", "0")]
        [InlineData("domain(x / 2, ZZ*)", "-4", "NaN")]
        [InlineData("domain(x / 2, ZZ*)", "1", "NaN")]
        [InlineData("domain(x - 1, ZZ+)", "1", "NaN")]
        [InlineData("domain(x - 1, ZZ*) + 1", "1", "1")]
        public void OutsideTheCodomainIsNaN(string input, string value, string expected)
            => Assert.Equal(expected == "NaN" ? MathS.NaN : expected.ToEntity(),
                input.ToEntity().Substitute("x", value.ToEntity()).Evaled);

        [Theory]
        [InlineData("ZZ+", AngouriMath.Core.Domain.PositiveInteger)]
        [InlineData("ZZ*", AngouriMath.Core.Domain.NonNegativeInteger)]
        public void TheDomainAndTheSetAgree(string set, AngouriMath.Core.Domain domain)
        {
            Assert.Equal(set.ToEntity(), Set.SpecialSet.Create(domain));
            Assert.True(domain < AngouriMath.Core.Domain.Integer);
            Assert.True(domain > AngouriMath.Core.Domain.Boolean);
        }

        /// <summary>
        /// The name that would have meant either set is refused with both spellings, rather than
        /// read as a variable that stands for nothing.
        /// </summary>
        [Theory]
        [InlineData("NN")]
        [InlineData("3 in NN")]
        [InlineData("{ k in NN : k > 3 }")]
        public void NNIsRefusedWithTheTwoSpellings(string input)
        {
            var thrown = Assert.Throws<InvalidArgumentParseException>(() => input.ToEntity());
            Assert.Contains("ZZ*", thrown.Message);
            Assert.Contains("ZZ+", thrown.Message);
        }

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
