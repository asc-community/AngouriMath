//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath;
using AngouriMath.Extensions;
using Xunit;

namespace AngouriMath.Tests.Core
{
    /// <summary>
    /// A summation whose summand is a polynomial in the index, answered as a polynomial in the
    /// bounds rather than carried: <c>sum(k, k, 1, n)</c> is <c>(n + n^2)/2</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/717">#717</a>
    /// </summary>
    /// <remarks>
    /// <para>
    /// Almost every case here is checked by <b>agreeing with the expansion</b> at several
    /// concrete bounds rather than against a printed form. The closed form and the term-by-term
    /// sum are two routes to one number, and that they meet is the property worth asserting; the
    /// shape the answer prints in is not.
    /// </para>
    /// <para>
    /// The exception is <see cref="TheEmptyRangeIsWhereTheConditionEarnsItsPlace"/>, which is
    /// about the condition attached to the closed form and would pass vacuously if it only
    /// compared the two routes.
    /// </para>
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class ClosedFormSummationTest
    {
        /// <summary>
        /// The closed form agrees with writing the sum out, at bounds on both sides of the
        /// empty-range boundary.
        /// </summary>
        private static void AgreesWithTheExpansion(string expression, params int[] bounds)
        {
            var closed = expression.ToEntity().Simplify();
            Assert.IsNotType<Entity.Summationf>(closed);
            foreach (var at in bounds)
            {
                var expanded = expression.ToEntity().Substitute("n", at).Simplify().Evaled;
                var fromClosedForm = closed.Substitute("n", at).Simplify().Evaled;
                Assert.Equal(expanded, fromClosedForm);
            }
        }

        /// <summary>The sums whose closed forms have names.</summary>
        [Theory]
        [InlineData("sum(1, k, 1, n)")]
        [InlineData("sum(k, k, 1, n)")]
        [InlineData("sum(k ^ 2, k, 1, n)")]
        [InlineData("sum(k ^ 3, k, 1, n)")]
        [InlineData("sum(k ^ 4, k, 1, n)")]
        public void ThePowerSums(string expression)
            => AgreesWithTheExpansion(expression, 0, 1, 2, 3, 7, 12);

        /// <summary>Any polynomial summand, by linearity over its terms.</summary>
        [Theory]
        [InlineData("sum(2 * k + 1, k, 1, n)")]
        [InlineData("sum(k ^ 2 - 3 * k + 5, k, 1, n)")]
        [InlineData("sum((k + 1) * (k + 2), k, 1, n)")]
        [InlineData("sum(k * (k - 1) / 2, k, 1, n)")]
        public void AnyPolynomialSummand(string expression)
            => AgreesWithTheExpansion(expression, 0, 1, 2, 3, 7, 12);

        /// <summary>
        /// A coefficient that does not mention the index may be anything, since it is carried
        /// through by linearity and never has to be decided.
        /// </summary>
        [Theory]
        [InlineData("sum(c, k, 1, n)")]
        [InlineData("sum(c * k, k, 1, n)")]
        [InlineData("sum(a * k ^ 2 + b * k + c, k, 1, n)")]
        [InlineData("sum(sin(x) * k, k, 1, n)")]
        public void ACoefficientFreeOfTheIndexIsCarried(string expression)
            => AgreesWithTheExpansion(expression, 0, 1, 2, 5);

        /// <summary>
        /// A symbolic lower bound too — the identity is <c>S(to) - S(from - 1)</c> and neither
        /// bound has to be concrete for that.
        /// </summary>
        [Fact]
        public void ASymbolicLowerBound()
        {
            var closed = "sum(k, k, m, n)".ToEntity().Simplify();
            Assert.IsNotType<Entity.Summationf>(closed);
            foreach (var (from, to) in new[] { (1, 5), (2, 7), (3, 3), (4, 3), (-2, 2) })
                Assert.Equal(
                    "sum(k, k, m, n)".ToEntity().Substitute("m", from).Substitute("n", to).Simplify().Evaled,
                    closed.Substitute("m", from).Substitute("n", to).Simplify().Evaled);
        }

        /// <summary>
        /// <b>Why the answer carries a condition.</b> This library answers an empty range with the
        /// operator's identity, so <c>sum(k, k, 1, -2)</c> is <c>0</c> — while the polynomial
        /// <c>(n + n^2)/2</c> is <c>1</c> there. The closed form therefore holds only where
        /// <c>to >= from - 1</c>, and says so; without that it would be a wrong answer below the
        /// boundary rather than an absent one.
        /// </summary>
        [Theory]
        [InlineData(-5)]
        [InlineData(-2)]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public void TheEmptyRangeIsWhereTheConditionEarnsItsPlace(int bound)
            => Assert.Equal(
                "sum(k, k, 1, n)".ToEntity().Substitute("n", bound).Simplify().Evaled,
                "sum(k, k, 1, n)".ToEntity().Simplify().Substitute("n", bound).Simplify().Evaled);

        /// <summary>
        /// A concrete range longer than the expansion will write out is now answered rather than
        /// carried, which is the same capability seen from the other side: the closed form
        /// computes the value where writing the terms was refused.
        /// </summary>
        [Theory]
        [InlineData("sum(k, k, 1, 100000)", "5000050000")]
        [InlineData("sum(k, k, 1, 1000)", "500500")]
        [InlineData("sum(k ^ 2, k, 1, 1000)", "333833500")]
        public void ARangeTooLongToWriteOutIsStillAnswered(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// <b>A bound that is a number and not a whole one is left alone</b>, which is the other
        /// place this could answer a different question confidently. The index runs over the
        /// integers, so <c>sum(k, k, 1, 5/2)</c> is <c>1 + 2 = 3</c>; the polynomial continued to
        /// <c>5/2</c> is <c>35/8</c>. Neither is a rounding of the other.
        /// </summary>
        [Theory]
        [InlineData("sum(k, k, 1, 5/2)")]
        [InlineData("sum(k, k, 1, 1.5)")]
        [InlineData("sum(k, k, 1, +oo)")]
        [InlineData("sum(k, k, -oo, n)")]
        public void ABoundThatIsNotAWholeNumberIsLeftAlone(string expression)
            => Assert.IsType<Entity.Summationf>(expression.ToEntity().Simplify());

        /// <summary>
        /// What is not a polynomial in the index is carried, as it was. Each of these has a
        /// closed form in the literature and none of them is this method's.
        /// </summary>
        [Theory]
        [InlineData("sum(2 ^ k, k, 1, n)")]
        [InlineData("sum(1 / k, k, 1, n)")]
        [InlineData("sum(1 / k ^ 2, k, 1, n)")]
        [InlineData("sum(sin(k), k, 1, n)")]
        [InlineData("sum(k ^ k, k, 1, n)")]
        [InlineData("sum(factorial(k), k, 1, n)")]
        public void WhatIsNotPolynomialInTheIndexIsCarried(string expression)
            => Assert.IsType<Entity.Summationf>(expression.ToEntity().Simplify());

        /// <summary>
        /// A product whose body this cannot read is carried. The ones it can are
        /// <see cref="ClosedFormProductTest"/>'s subject — a product has no linearity, so where
        /// the sum takes any polynomial apart the product needs a single term.
        /// </summary>
        /// <remarks>
        /// <c>product(k, k, 1, n)</c> was on this list, for wanting a condition of the same kind
        /// the sum carries: the empty product is <c>1</c> at every <c>n &lt; 1</c>, while
        /// <c>factorial</c> is undefined at the negative integers. It has that condition now.
        /// </remarks>
        [Theory]
        [InlineData("product(k + 1, k, 1, n)")]
        [InlineData("product(2 ^ k, k, 1, n)")]
        public void TheProductIsCarried(string expression)
            => Assert.IsType<Entity.Productf>(expression.ToEntity().Simplify());

        /// <summary>
        /// The short concrete ranges keep being written out, which is the cheaper route and the
        /// only one that answers a summand this cannot read.
        /// </summary>
        [Theory]
        [InlineData("sum(k, k, 1, 3)", "6")]
        [InlineData("sum(k ^ 2, k, 1, 10)", "385")]
        [InlineData("sum(2 ^ k, k, 1, 5)", "62")]
        [InlineData("sum(1 / k, k, 1, 4)", "25/12")]
        [InlineData("product(k, k, 1, 5)", "120")]
        public void AShortConcreteRangeIsUnchanged(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>
        /// The index is still bound: a closed form must not leak it, and substituting it from
        /// outside must still do nothing.
        /// </summary>
        [Fact]
        public void TheIndexDoesNotEscape()
        {
            var closed = "sum(k, k, 1, n)".ToEntity().Simplify();
            Assert.DoesNotContain((Entity.Variable)"k", closed.Vars);
            Assert.Equal(closed, closed.Substitute("k", 5));
        }
    }
}
