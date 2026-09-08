//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;
using static AngouriMath.Entity;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// <c>card(S)</c>: the number of elements of a set, as a node of its own.
    /// https://github.com/asc-community/AngouriMath/issues/1212
    /// </summary>
    public sealed class CardinalityTest
    {
        [Theory]
        [InlineData("card({ 1, 2, 3 })", "3")]
        [InlineData("card({ })", "0")]
        [InlineData("card({ 1, 1, 2 })", "2")]
        [InlineData("card({ 1/2, 0.5, 2 })", "2")]
        [InlineData("card({ 1, 2 } \\/ { 2, 3 })", "3")]
        [InlineData("card({ 1, 2, 3 } /\\ { 2, 3, 4 })", "2")]
        [InlineData("card({ 1, 2, 3 } \\ { 2 })", "2")]
        [InlineData("card({ pi, e, sqrt(2) })", "3")]
        [InlineData("card([1; 1])", "1")]
        [InlineData("card([2; 1])", "0")]
        [InlineData("card((1; 1))", "0")]
        [InlineData("card([1; 1))", "0")]
        [InlineData("card({ 1 } \\/ { 1 })", "1")]
        public void AFiniteSetOfNumbersIsCounted(string card, string expected)
            => Assert.Equal(expected.ToEntity(), card.ToEntity().Evaled);

        // {x, 1} has two elements unless x is 1: a set with a symbol in it is not counted until
        // its elements are known to be distinct.
        [Fact]
        public void ASetWithASymbolInItIsNotCountedUntilItsElementsAreKnown()
        {
            var card = "card({ x, 1 })".ToEntity();
            Assert.IsType<Cardf>(card.Simplify());
            Assert.Equal("2".ToEntity(), card.Substitute("x", 2).Evaled);
            Assert.Equal("1".ToEntity(), card.Substitute("x", 1).Evaled);
        }

        // Infinite sets have a cardinality the library has no number for, and a set builder is
        // not enumerated: both stay as written rather than being answered with anything.
        [Theory]
        [InlineData("card(RR)")]
        [InlineData("card(ZZ)")]
        [InlineData("card([0; 1])")]
        [InlineData("card((a; b))")]
        [InlineData("card({ x : x > 0 })")]
        [InlineData("card(S)")]
        public void WhatIsNotAFiniteSetOfNumbersIsLeftAsWritten(string card)
            => Assert.IsType<Cardf>(card.ToEntity().Simplify());

        // Canonical is the prefix `#`; `card( )` is accepted on input and prints as `#`.
        [Theory]
        [InlineData("card({ 1, 2 })", "#{ 1, 2 }")]
        [InlineData("#{ 1, 2 }", "#{ 1, 2 }")]
        [InlineData("card(S) + 1", "#S + 1")]
        [InlineData("#S + 1", "#S + 1")]
        [InlineData("card(A \\/ B)", "#(A \\/ B)")]
        [InlineData("#(A \\/ B)", "#(A \\/ B)")]
        [InlineData("#S", "#S")]
        public void ItParsesAndPrintsAsWritten(string written, string printed)
        {
            var entity = written.ToEntity();
            Assert.Equal(printed, entity.ToString());
            Assert.Equal(entity, printed.ToEntity());
        }

        // `#` binds as tightly as a function call: `#S + 1` is `(#S) + 1`, and a set expression
        // takes parentheses.
        [Fact]
        public void TheHashBindsLikeAFunctionCall()
        {
            Assert.Equal(new Sumf(new Cardf("S".ToEntity()), 1), "#S + 1".ToEntity());
            Assert.Equal("card(S)".ToEntity(), "#S".ToEntity());
            Assert.Equal("3".ToEntity(), "#{ 1, 2, 3 }".ToEntity().Evaled);
        }

        [Fact]
        public void ItPrintsAsAHashInLatex()
        {
            Assert.Equal(@"\#S", "card(S)".ToEntity().Latexize());
            Assert.Equal(@"\#S", "#S".ToEntity().Latexize());
        }

        [Fact]
        public void TheConvenienceSpellingAgreesWithTheParser()
        {
            Entity s = "S";
            Assert.Equal("card(S)".ToEntity(), MathS.Sets.Card(s));
            Assert.Equal("card({ 1, 2 })".ToEntity(), MathS.Sets.Card(MathS.Sets.Finite(1, 2)));
        }

        [Fact]
        public void ItRoundTripsThroughJson()
        {
            var card = "card({ 1, 2 } \\/ S)".ToEntity();
            var json = System.Text.Json.JsonSerializer.Serialize(card);
            Assert.Equal(card, System.Text.Json.JsonSerializer.Deserialize<Entity>(json));
        }

        // A count is a number, so it can be used as one.
        [Fact]
        public void ACountIsANumber()
            => Assert.Equal("6".ToEntity(), "card({ 1, 2, 3 }) * 2".ToEntity().Evaled);
    }
}
