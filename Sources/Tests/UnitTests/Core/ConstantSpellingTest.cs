//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>π</c> and the script <c>ℯ</c> are the constants <c>pi</c> and <c>e</c>, as mathematics
    /// writes them, where <c>π</c> was a free variable and <c>ℯ</c> did not parse. The Cyrillic
    /// <c>е</c> and <c>х</c> are letters of an alphabet the grammar admits, and stay variables.
    /// https://github.com/asc-community/AngouriMath/issues/1260
    /// </summary>
    [Trait("Area", "Core")]
    public sealed class ConstantSpellingTest
    {
        [Theory]
        [InlineData("sin(π)", "0")]
        [InlineData("cos(π / 3)", "1/2")]
        [InlineData("π - pi", "0")]
        [InlineData("ln(ℯ)", "1")]
        [InlineData("ℯ^2 / e^2", "1")]
        public void TheConstantsReadInTheirSpellings(string written, string simplified)
            => Assert.Equal(simplified.ToEntity(), written.ToEntity().Simplify());

        [Fact]
        public void ASpellingIsTheConstantItself()
        {
            Assert.Equal("pi".ToEntity(), "π".ToEntity());
            Assert.Equal("e".ToEntity(), "ℯ".ToEntity());
        }

        [Fact]
        public void CyrillicLettersAreVariables()
        {
            // The Cyrillic e (U+0435) is a letter, not the constant, and x (U+0445) is another
            // letter from the Latin x: two variables that print alike.
            Assert.IsType<Entity.Variable>("е".ToEntity());
            Assert.Equal("ln(е)".ToEntity(), "ln(е)".ToEntity().Simplify());
            Assert.Equal(2, "х + x".ToEntity().Vars.Count());
        }
    }
}
