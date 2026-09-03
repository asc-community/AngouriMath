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
    /// A product whose body is a monomial in the index, answered in closed form:
    /// <c>product(k, k, 1, n)</c> is <c>factorial(n)</c>.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/717">#717</a>
    /// </summary>
    /// <remarks>
    /// Narrower than the sum's closed form, and not by omission: a sum of two terms is the sum of
    /// their sums, and a product of two terms is not the product of their products in any way
    /// that helps. What separates is the body that <em>is</em> one term.
    /// </remarks>
    [Trait("Area", "Core")]
    public sealed class ClosedFormProductTest
    {
        /// <summary>
        /// The closed form agrees with writing the product out, at bounds either side of the
        /// empty-range boundary — which is where the two could disagree and did.
        /// </summary>
        private static void AgreesWithTheExpansion(string expression, params int[] bounds)
        {
            var closed = expression.ToEntity().Simplify();
            Assert.IsNotType<Entity.Productf>(closed);
            foreach (var at in bounds)
                Assert.Equal(
                    expression.ToEntity().Substitute("n", at).Simplify().Evaled,
                    closed.Substitute("n", at).Simplify().Evaled);
        }

        /// <summary>The factorial, which is the row the survey names.</summary>
        [Theory]
        [InlineData("product(k, k, 1, n)")]
        [InlineData("product(k ^ 2, k, 1, n)")]
        [InlineData("product(k ^ 3, k, 1, n)")]
        public void ThePowerOfAFactorial(string expression)
            => AgreesWithTheExpansion(expression, -2, -1, 0, 1, 2, 3, 5, 8);

        /// <summary>A constant factor comes out as a power of itself, one per term.</summary>
        [Theory]
        [InlineData("product(2, k, 1, n)")]
        [InlineData("product(c, k, 1, n)")]
        [InlineData("product(2 * k, k, 1, n)")]
        [InlineData("product(c * k, k, 1, n)")]
        [InlineData("product(c * k ^ 2, k, 1, n)")]
        public void AConstantFactor(string expression)
            => AgreesWithTheExpansion(expression, -2, -1, 0, 1, 2, 3, 5);

        /// <summary>
        /// <b>The empty range is where the boundary had to move.</b> The sum's condition is
        /// <c>to >= from - 1</c>; the product's is <c>to >= from</c>, because at the empty range
        /// itself the closed form is <c>c^0</c> — which is <c>1</c> for every <c>c</c> but zero,
        /// and undefined there, while the empty product is <c>1</c> for every <c>c</c> including
        /// zero. Handing that one point to the identity branch keeps a value from becoming an
        /// undefinedness.
        /// </summary>
        [Fact]
        public void TheEmptyProductIsOneEvenWhereTheBodyIsZero()
        {
            var closed = "product(c, k, 1, n)".ToEntity().Simplify();
            var atTheBoundary = closed.Substitute("n", 0).Substitute("c", 0).Simplify();
            Assert.Equal(Entity.Number.Integer.Create(1), atTheBoundary.Evaled);
            Assert.False(atTheBoundary.Evaled.IsNaN);
        }

        /// <summary>A symbolic lower bound, where the body has no index to need a factorial.</summary>
        [Fact]
        public void ASymbolicLowerBoundWithAConstantBody()
        {
            var closed = "product(c, k, m, n)".ToEntity().Simplify();
            Assert.IsNotType<Entity.Productf>(closed);
            foreach (var (from, to) in new[] { (1, 4), (2, 5), (3, 3), (4, 3), (-1, 2) })
                Assert.Equal(
                    "product(c, k, m, n)".ToEntity().Substitute("m", from).Substitute("n", to)
                        .Substitute("c", 3).Simplify().Evaled,
                    closed.Substitute("m", from).Substitute("n", to).Substitute("c", 3).Simplify().Evaled);
        }

        /// <summary>
        /// A concrete range too long to write out is answered rather than carried, the same as
        /// for the sum.
        /// </summary>
        [Fact]
        public void ARangeTooLongToWriteOutIsStillAnswered()
            => Assert.Equal(
                MathS.Pow(2, 500).Evaled,
                "product(2, k, 1, 500)".ToEntity().Simplify().Evaled);

        /// <summary>
        /// <b>A lower bound that is not a concrete positive integer is declined where the index
        /// is in the body</b>, and that cannot be a condition instead. <c>b!/(a-1)!</c> holds
        /// only for <c>a >= 1</c>; below it the range runs through zero and the product is
        /// <c>0</c> while <c>(a-1)!</c> is undefined — and <c>a &lt; 1</c> does not mean the
        /// range is empty, so it cannot share a branch with the empty-range case.
        /// </summary>
        [Theory]
        [InlineData("product(k, k, 0, n)")]
        [InlineData("product(k, k, -3, n)")]
        [InlineData("product(k, k, m, n)")]
        public void ALowerBoundThatIsNotAConcretePositiveIntegerIsDeclined(string expression)
            => Assert.IsType<Entity.Productf>(expression.ToEntity().Simplify());

        /// <summary>
        /// What is not a monomial in the index is carried. A product has no linearity, so a sum
        /// of terms in the body is not something this takes apart.
        /// </summary>
        [Theory]
        [InlineData("product(k + 1, k, 1, n)")]
        [InlineData("product(k ^ 2 + k, k, 1, n)")]
        [InlineData("product(2 ^ k, k, 1, n)")]
        [InlineData("product(sin(k), k, 1, n)")]
        [InlineData("product(1 / k, k, 1, n)")]
        public void WhatIsNotAMonomialIsCarried(string expression)
            => Assert.IsType<Entity.Productf>(expression.ToEntity().Simplify());

        /// <summary>
        /// A bound that is a number and not a whole one is carried, as for the sum: the index
        /// runs over the integers.
        /// </summary>
        [Theory]
        [InlineData("product(k, k, 1, 5/2)")]
        [InlineData("product(k, k, 1, +oo)")]
        public void ABoundThatIsNotAWholeNumberIsCarried(string expression)
            => Assert.IsType<Entity.Productf>(expression.ToEntity().Simplify());

        /// <summary>The short concrete ranges are written out as they were.</summary>
        [Theory]
        [InlineData("product(k, k, 1, 5)", "120")]
        [InlineData("product(k, k, 3, 6)", "360")]
        [InlineData("product(k ^ 2, k, 1, 4)", "576")]
        [InlineData("product(k, k, 5, 1)", "1")]
        [InlineData("product(2 ^ k, k, 1, 4)", "1024")]
        public void AShortConcreteRangeIsUnchanged(string expression, string expected)
            => Assert.Equal(expected.ToEntity().Evaled, expression.ToEntity().Simplify().Evaled);

        /// <summary>The index stays bound.</summary>
        [Fact]
        public void TheIndexDoesNotEscape()
        {
            var closed = "product(k, k, 1, n)".ToEntity().Simplify();
            Assert.DoesNotContain((Entity.Variable)"k", closed.Vars);
        }
    }
}
