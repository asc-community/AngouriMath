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

namespace AngouriMath.Tests.Calculus
{
    /// <summary>
    /// The binomial sums of chapter 8 of Sullivan and Mackey's <i>An Introduction to Proofs</i>
    /// beyond the binomial theorem: a polynomial beside the coefficient (Prop 8.4.4, §8.4.5
    /// Try 2), Vandermonde (Prob 8.9.16) and <c>sum binomial(n, k)^2 = binomial(2n, n)</c>
    /// (Prob 8.9.34), the summation identity (Thm 8.4.6), and the sums over the even or the
    /// odd indices (Ex 8.3.11). <see href="https://github.com/asc-community/AngouriMath/issues/1409"/>
    /// </summary>
    public sealed class BinomialIdentitiesTest
    {
        /// <summary>The closed form against the sum written out, at small bounds and one past the expansion's hundred terms.</summary>
        [Theory]
        [InlineData("sum(k * binomial(n, k), k, 0, n)", "n * 2^(n - 1)")]
        [InlineData("sum(k^2 * binomial(n, k), k, 0, n)", "n * (n + 1) * 2^(n - 2)")]
        [InlineData("sum(k * binomial(n, k) * x^k, k, 0, n)", "n * x * (x + 1)^(n - 1)")]
        [InlineData("sum((k^2 - k) * binomial(n, k) * 2^k * 3^(n - k), k, 0, n)", "n * (n - 1) * 4 * 5^(n - 2)")]
        [InlineData("sum(n! / (k! * (n - k)!) * k, k, 0, n)", "n * 2^(n - 1)")]
        [InlineData("sum(binomial(n, k)^2, k, 0, n)", "binomial(2 n, n)")]
        [InlineData("sum(binomial(n, i) * binomial(n + 3, 5 - i), i, 0, 5)", "binomial(2 n + 3, 5)")]
        [InlineData("sum(binomial(i, 3), i, 0, n)", "binomial(n + 1, 4)")]
        [InlineData("sum(binomial(n, 2 * l), l, 0, floor(n / 2))", "2^(n - 1)")]
        [InlineData("sum(binomial(n, 2 * l + 1), l, 0, floor((n - 1) / 2))", "2^(n - 1)")]
        public void AnIdentityIsClosed(string sum, string expected)
        {
            var closed = sum.ToEntity().Simplify();
            Assert.IsNotType<Summationf>(closed);
            foreach (var n in new[] { 1, 2, 3, 6, 11, 130 })
            {
                var want = expected.ToEntity().Substitute("n", n).Substitute("x", "3/7").Evaled;
                Assert.Equal(want, closed.Substitute("n", n).Substitute("x", "3/7").Evaled);
                Assert.Equal(want, sum.ToEntity().Substitute("n", n).Substitute("x", "3/7").Evaled);
            }
        }

        /// <summary>Vandermonde with both parameters free, and its range written to either of them.</summary>
        [Theory]
        [InlineData("sum(binomial(a, i) * binomial(b, k - i), i, 0, k)")]
        [InlineData("sum(binomial(a, i) * binomial(b, k - i), i, 0, a)")]
        public void VandermondeWithBothParametersFree(string sum)
        {
            var closed = sum.ToEntity().Simplify();
            Assert.IsNotType<Summationf>(closed);
            foreach (var (a, b, k) in new[] { (4, 5, 3), (7, 2, 6), (3, 3, 3), (10, 12, 8) })
            {
                var want = "binomial(a + b, k)".ToEntity().Substitute("a", a).Substitute("b", b).Substitute("k", k).Evaled;
                Assert.Equal(want, closed.Substitute("a", a).Substitute("b", b).Substitute("k", k).Evaled);
            }
        }

        /// <summary>Ex 8.3.11 at n = 0: the even sum is the one empty subset, the odd sum is empty.</summary>
        [Fact]
        public void TheParitySumsAtZero()
        {
            Assert.Equal(Number.Integer.One, "sum(binomial(n, 2 * l), l, 0, floor(n / 2))".ToEntity().Simplify().Substitute("n", 0).Evaled);
            Assert.Equal(Number.Integer.Zero, "sum(binomial(n, 2 * l + 1), l, 0, floor((n - 1) / 2))".ToEntity().Simplify().Substitute("n", 0).Evaled);
        }

        /// <summary>The identities the book proves, decided from the closed forms.</summary>
        [Theory]
        [InlineData("forall n in ZZ+ : sum(k * binomial(n, k), k, 0, n) = n * 2^(n - 1)")]
        [InlineData("forall n in ZZ* : sum(binomial(n, k)^2, k, 0, n) = binomial(2 n, n)")]
        [InlineData("forall n in ZZ* : sum(binomial(i, k), i, 0, n) = binomial(n + 1, k + 1)")]
        public void TheIdentityIsDecided(string statement) => Assert.Equal(Boolean.True, statement.ToEntity().Evaled);
    }
}
