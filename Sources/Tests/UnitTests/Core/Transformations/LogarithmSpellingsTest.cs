//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using AngouriMath;
using AngouriMath.Core.Transformations.Matching;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core.Transformations
{
    /// <summary>
    /// <c>ln(x)</c> is <c>log(e, x)</c> with the base a constant rather than a number, so a rule
    /// matching a logarithm can quietly stop matching the one spelling while it matches the
    /// other. Every rule that reads a logarithm is exercised here on both: an identity written
    /// in base 2 and the same identity in base e, and a rule fires on one exactly when it fires
    /// on the other. <a href="https://github.com/asc-community/AngouriMath/issues/994">#994</a>
    /// </summary>
    [Trait("Area", "Transformations")]
    public sealed class LogarithmSpellingsTest
    {
        [Theory]
        [InlineData("2 ^ log(2, x)", "e ^ ln(x)")]
        [InlineData("x ^ (n / log(2, x))", "x ^ (n / ln(x))")]
        [InlineData("log(2, x ^ 3)", "ln(x ^ 3)")]
        [InlineData("log(2, 8)", "ln(8)")]
        [InlineData("log(2, 2)", "ln(e)")]
        [InlineData("log(2, 1 / x)", "ln(1 / x)")]
        [InlineData("log(2, x) + log(2, y)", "ln(x) + ln(y)")]
        [InlineData("log(2, x) - log(2, y)", "ln(x) - ln(y)")]
        // A reciprocal base has to be written as a quotient to be one -- 1/2 is a number -- and
        // the rule asks that the base be decidably positive, which a symbol is not; so the pair
        // here is the other constant against e.
        [InlineData("log(1 / pi, x)", "log(1 / e, x)")]
        [InlineData("log(1 / pi, 1 / x)", "log(1 / e, 1 / x)")]
        public void EveryLogarithmRuleSetReadsBothSpellings(string inBaseTwo, string natural)
        {
            var two = inBaseTwo.ToEntity();
            var e = natural.ToEntity();
            // Per set rather than per rule: a set may read a numeric base and a constant one
            // through two rules, and what is owed is that the set answers both.
            var disagreeing = MatchedRules.All
                .Where(set => !ReferenceEquals(set.ApplyHere(two), two) != !ReferenceEquals(set.ApplyHere(e), e))
                .Select(set => set.Name)
                .ToList();
            Assert.True(disagreeing.Count == 0,
                $"these rule sets rewrite one spelling and not the other of {inBaseTwo} / {natural}: " + string.Join(", ", disagreeing));
        }

        [Theory]
        // b ^ log(b, a) = a wherever the logarithm is defined, whatever the base.
        [InlineData("2 ^ log(2, x)", "x")]
        [InlineData("e ^ ln(x)", "x")]
        [InlineData("pi ^ log(pi, x)", "x")]
        [InlineData("(1/2) ^ log(1/2, x)", "x")]
        [InlineData("a ^ log(a, x)", "x provided not a = 0 and not a = 1")]
        [InlineData("(x + 1) ^ log(x + 1, y)", "y provided not 1 + x = 0 and not 1 + x = 1")]
        // A base of 1 or 0 is not a base, and the identity is not asserted of it.
        [InlineData("1 ^ log(1, x)", "NaN")]
        public void APowerUndoesALogarithmOfItsOwnBase(string input, string expected)
            => Assert.Equal(expected.ToEntity(), input.ToEntity().Simplify());

        [Fact]
        public void TheBaseOfLnIsNotAMentionOfE()
        {
            Assert.Equal("ln(x)".ToEntity(), "ln(x)".ToEntity().Substitute("e", 3));
            Assert.Equal("exp(x)".ToEntity(), "exp(x)".ToEntity().Substitute("e", 3));
            // Where the writer did name e, the name is substituted.
            Assert.Equal("log(3, x)".ToEntity(), "log(e, x)".ToEntity().Substitute("e", 3));
            Assert.Equal("3 ^ x".ToEntity(), "e ^ x".ToEntity().Substitute("e", 3));
        }
    }
}
