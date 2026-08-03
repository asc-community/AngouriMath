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
    /// l'Hopital's rule was only applied when the destination was finite, so the ordinary
    /// competing-growth limits at infinity came back unevaluated.
    /// </summary>
    public sealed class LimitAtInfinityTest
    {
        private static Entity Limit(string expression, string destination) =>
            expression.ToEntity().Limit("x", destination.ToEntity());

        [Theory]
        [InlineData("ln(x) / x", "+oo", "0")]
        [InlineData("x / e ^ x", "+oo", "0")]
        [InlineData("x ^ 2 / e ^ x", "+oo", "0")]
        [InlineData("ln(x) / sqrt(x)", "+oo", "0")]
        [InlineData("e ^ x / (e ^ x + 1)", "+oo", "1")]
        [InlineData("ln(x) ^ 2 / x", "+oo", "0")]
        [InlineData("x / e ^ (2 * x)", "+oo", "0")]
        [InlineData("x / ln(x) ^ 2", "+oo", "+oo")]
        [InlineData("x ^ 3 / ln(x)", "+oo", "+oo")]
        [InlineData("ln(ln(x)) / ln(x)", "+oo", "0")]
        [InlineData("sqrt(x) / sqrt(x + 1)", "+oo", "1")]
        [InlineData("x ^ 10 / e ^ x", "+oo", "0")]
        public void CompetingGrowthAtInfinity(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity(), Limit(expression, destination).Simplify());

        // https://github.com/asc-community/AngouriMath/issues/596. The same limits written as
        // products with a reciprocal factor. Only the shape differs, and only the quotient
        // shape was read.
        [Theory]
        [InlineData("(x ^ (5 - 1)) * e ^ (-x)", "+oo", "0")]
        [InlineData("x ^ 4 * e ^ (-x)", "+oo", "0")]
        [InlineData("x * e ^ (-x)", "+oo", "0")]
        [InlineData("ln(x) * x ^ (-1)", "+oo", "0")]
        [InlineData("e ^ (-2 * x) * x ^ 3", "+oo", "0")]
        public void AProductWithAReciprocalFactorIsAQuotient(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity(), Limit(expression, destination).Simplify());

        // The forms that already had an answer must keep it, in particular the ones that do
        // not go through the rule at all.
        [Theory]
        [InlineData("x ^ 2 / (x ^ 2 + 1)", "+oo", "1")]
        [InlineData("(2 * x + 1) / (x - 3)", "+oo", "2")]
        [InlineData("1 / x", "+oo", "0")]
        [InlineData("x ^ 3", "+oo", "+oo")]
        [InlineData("sin(x) / x", "0", "1")]
        [InlineData("(1 + x) ^ (1/x)", "0", "e")]
        [InlineData("(x ^ 2 - 1) / (x - 1)", "1", "2")]
        [InlineData("sqrt(x + 1) / sqrt(x)", "+oo", "1")]
        [InlineData("sqrt(x + 5) / sqrt(x + 1)", "+oo", "1")]
        public void EstablishedLimitsAreUnaffected(string expression, string destination, string expected) =>
            Assert.Equal(expected.ToEntity(), Limit(expression, destination).Simplify());

        /// <summary>
        /// Differentiating both parts of x / sqrt(x^2 + 1) gives its own reciprocal, so the rule
        /// never settles and has to be stopped; x^20 / e^x would settle but only after more
        /// steps than the bound allows. Either way the limit is left unevaluated, which is
        /// honest. What must not happen is a hang, and the answer must not become NaN either,
        /// since NaN asserts that the limit does not exist.
        /// </summary>
        [Theory]
        [InlineData("x / sqrt(x ^ 2 + 1)")]
        [InlineData("(x + sin(x)) / x")]
        [InlineData("x ^ 20 / e ^ x")]
        public void AFormTheRuleCannotSettleTerminatesWithoutClaimingAnAnswer(string expression)
        {
            var task = Task.Run(() => Limit(expression, "+oo"));
            Assert.True(task.Wait(TimeSpan.FromSeconds(30)), "the limit did not terminate");
            Assert.IsType<Entity.Limitf>(task.Result);
        }
    }
}
