//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Threading.Tasks;
using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// Limits at infinity by Gruntz's algorithm, after D. Gruntz, "On Computing Limits in a
    /// Symbolic Manipulation System", ETH 1996. What it adds over the rules that came before
    /// it are the expressions whose parts cancel to every order, which no amount of
    /// differentiating both halves reaches.
    /// </summary>
    public sealed class GruntzTest
    {
        private static void AssertLimit(string expression, string destination, string expected) =>
            Assert.Equal(
                expected.ToEntity().Evaled,
                expression.ToEntity().Limit("x", destination.ToEntity()).Evaled);

        /// <summary>
        /// The example the algorithm is usually shown with. Expanding the two exponentials
        /// separately gives two divergent series whose difference cancels entirely; rewriting
        /// the whole expression in w = e^(-x) gives (e^w - 1)/w, whose leading term is 1.
        /// </summary>
        [Theory]
        [InlineData("e ^ (x + e ^ (-x)) - e ^ x", "+oo", "1")]
        [InlineData("e ^ (x + e ^ (-x)) / e ^ x", "+oo", "1")]
        [InlineData("sqrt(x ^ 2 + 3 * x) - sqrt(x ^ 2 + 1)", "+oo", "3/2")]
        [InlineData("x ^ 20 / e ^ x", "+oo", "0")]
        [InlineData("x / sqrt(x ^ 2 + 1)", "+oo", "1")]
        [InlineData("sqrt(x ^ 2 - x) / x", "+oo", "1")]
        public void WhatNothingBeforeItCouldReach(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        // The ordinary competing growths, which have to come out the same as they did.
        [Theory]
        [InlineData("ln(x) / x", "+oo", "0")]
        [InlineData("x / e ^ x", "+oo", "0")]
        [InlineData("e ^ x / x", "+oo", "+oo")]
        [InlineData("x ^ 2 / (x ^ 2 + 1)", "+oo", "1")]
        [InlineData("(2 * x + 1) / (x - 3)", "+oo", "2")]
        [InlineData("ln(ln(x)) / ln(x)", "+oo", "0")]
        [InlineData("x / ln(x) ^ 2", "+oo", "+oo")]
        [InlineData("e ^ x / (e ^ x + 1)", "+oo", "1")]
        [InlineData("(1 + 1/x) ^ x", "+oo", "e")]
        [InlineData("e ^ x - x", "+oo", "+oo")]
        [InlineData("sqrt(x ^ 2 + x) - x", "+oo", "1/2")]
        [InlineData("sqrt(x ^ 2 + x) + x", "-oo", "-1/2")]
        public void TheOrdinaryOnesAreUnchanged(string expression, string destination, string expected) =>
            AssertLimit(expression, destination, expected);

        /// <summary>
        /// The algorithm is stated for the exp-log functions -- what can be built from x and
        /// the rationals with the four operations, exp and log -- because those are eventually
        /// monotone and so comparable at all. A sine is not: it has no limit at infinity and no
        /// comparability class, so this declines rather than guesses.
        /// </summary>
        [Theory]
        [InlineData("sin(x)")]
        [InlineData("(x + sin(x)) / x")]
        [InlineData("x * cos(x)")]
        public void AnOscillationIsDeclinedRatherThanGuessedAt(string expression)
        {
            var task = Task.Run(() => expression.ToEntity().Limit("x", "+oo".ToEntity()));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }
    }
}
