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
    /// <c>forall n in ZZ+ : sum(f, k, a, n) = g</c> decided by induction from the least member of
    /// the set: the statement at that member, and the statement at <c>n + 1</c> with the sum
    /// unfolded by one term and the sum to <c>n</c> replaced by <c>g</c>. A closed form the sum
    /// has already is read through the piecewise it comes as, and an identity is read over
    /// atoms -- <c>2^n</c>, <c>n!</c>, <c>(-1)^n</c> -- with a whole shift in an exponent or a
    /// factorial's argument unfolded. The rows are chapter 5 of Sullivan and Mackey's proofs
    /// book. <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>
    /// </summary>
    [Trait("Area", "Discrete")]
    public sealed class InductionTest
    {
        [Theory]
        // Sums the summation folds: the closed form comes as a piecewise on n >= 0, and over
        // ZZ+ that case holds at every member (Ex 5.2.5, §5.2.4 Try 1, Prob 5.7.1).
        [InlineData("forall n in ZZ+ : sum(k, k, 1, n) = n (n + 1) / 2")]
        [InlineData("forall n in ZZ+ : sum(2 k - 1, k, 1, n) = n^2")]
        [InlineData("forall n in ZZ+ : sum(k^2, k, 1, n) = n (n + 1) (2 n + 1) / 6")]
        [InlineData("forall n in ZZ+ : sum(k^3, k, 1, n) = (sum(k, k, 1, n))^2")]
        [InlineData("forall n in ZZ* : sum(2^k, k, 0, n) = 2^(n + 1) - 1")]
        [InlineData("forall n in ZZ* : sum(binomial(n, k), k, 0, n) = 2^n")]
        [InlineData("forall n in ZZ+ : product(k, k, 1, n) = n!")]
        // Both bounds depend on n (Prob 5.7.16), and the bound is a multiple of n.
        [InlineData("forall n in ZZ+ : sum(2 k + 1, k, n, 2 n - 1) = 3 n^2")]
        [InlineData("forall n in ZZ+ : sum(k, k, 1, 2 n) = n (2 n + 1)")]
        // Sums the summation leaves as written, proved by induction: telescoping (Prob 5.7.18's
        // shape), a factorial, an exponential, an alternating sign (§5.3.4 Try 6), a parameter.
        [InlineData("forall n in ZZ+ : sum(1 / (k (k + 1)), k, 1, n) = n / (n + 1)")]
        [InlineData("forall n in ZZ+ : sum(1 / (k (k + 1)), k, 1, n) = 1 - 1 / (n + 1)")]
        [InlineData("forall n in ZZ+ : sum(1 / (k (k + 1) (k + 2)), k, 1, n) = 1/4 - 1 / (2 (n + 1) (n + 2))")]
        [InlineData("forall n in ZZ+ : sum(k k!, k, 1, n) = (n + 1)! - 1")]
        [InlineData("forall n in ZZ+ : (n + 1)! - 1 = sum(k k!, k, 1, n)")]
        [InlineData("forall n in ZZ+ : sum(1 / 2^k, k, 1, n) = 1 - 1 / 2^n")]
        [InlineData("forall n in ZZ+ : sum(k * 2^k, k, 1, n) = (n - 1) * 2^(n + 1) + 2")]
        [InlineData("forall n in ZZ+ : sum(k^2 * 2^k, k, 1, n) = 2^(n + 1) * (n^2 - 2 n + 3) - 6")]
        [InlineData("forall n in ZZ+ : sum((-1)^(k - 1) * k^2, k, 1, n) = (-1)^(n - 1) * n (n + 1) / 2")]
        // Products.
        [InlineData("forall n in ZZ+ : product(1 + 1/k, k, 1, n) = n + 1")]
        [InlineData("forall n in ZZ+ : product(k / (k + 1), k, 1, n) = 1 / (n + 1)")]
        [InlineData("forall n in ZZ+ : product(1 - 1 / (k + 1)^2, k, 1, n) = (n + 2) / (2 (n + 1))")]
        [InlineData("forall n in ZZ+ : product((2 k - 1) / (2 k), k, 1, n) = (2 n)! / (4^n (n!)^2)")]
        // Induction from any base (Thm 5.3.1): the least member of the set is where it starts.
        [InlineData("forall n in ZZ* : sum(1 / (k (k + 1)), k, 1, n) = n / (n + 1)")]
        [InlineData("forall n in ZZ+ /\\ [2; +oo) : sum(1 / (k (k + 1)), k, 1, n) = n / (n + 1)")]
        [InlineData("forall n in ZZ+ /\\ (3/2; +oo) : sum(1 / (k (k + 1)), k, 1, n) = n / (n + 1)")]
        public void ProvedByInduction(string statement)
            => Assert.Equal(Entity.Boolean.True, statement.ToEntity().Evaled);

        [Theory]
        // A closed form over a denominator in a free parameter holds where the denominator is
        // not zero, and the answer says so; a denominator in the quantified name is decided
        // non-zero over the set instead, so those rows above carry no condition.
        [InlineData("forall n in ZZ+ : sum(k x^k, k, 1, n) = x (1 - (n + 1) x^n + n x^(n + 1)) / (1 - x)^2", "True provided not (1 - x)^2 = 0")]
        [InlineData("forall n in ZZ+ : sum(1 / (a k (k + 1)), k, 1, n) = n / (a (n + 1))", "True provided not a = 0")]
        public void ProvedWhereTheClosedFormExists(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);

        [Theory]
        // Wrong at the least member, which is a counterexample.
        [InlineData("forall n in ZZ+ : sum(k, k, 1, n) = n^2")]
        [InlineData("forall n in ZZ+ : sum(k, k, 1, n) = n (n + 1) / 2 + 1")]
        [InlineData("forall n in ZZ+ : sum(k k!, k, 1, n) = (n + 1)!")]
        [InlineData("forall n in ZZ+ : sum(1 / (k (k + 1)), k, 1, n) = n / (n + 2)")]
        [InlineData("forall n in ZZ+ : sum(1 / 2^k, k, 1, n) = 1 - 1 / 2^(n - 1)")]
        [InlineData("forall n in ZZ+ : sum(1 / k, k, 1, n) = n")]
        // The empty sum is 0, so over ZZ the closed form's other case is a counterexample.
        [InlineData("forall n in ZZ : sum(k, k, 1, n) = n (n + 1) / 2")]
        public void RefutedAtAMember(string statement)
            => Assert.Equal(Entity.Boolean.False, statement.ToEntity().Evaled);

        [Theory]
        // Not decided: a closed form with a case on a free parameter (q = 1 has its own), a wrong
        // closed form with a free parameter, an inequality, and a term the induction cannot unfold.
        [InlineData("forall n in ZZ+ : sum(q^k, k, 0, n - 1) = (q^n - 1) / (q - 1)")]
        [InlineData("forall n in ZZ+ : sum(k x^k, k, 1, n) = x (1 - (n + 1) x^n + n x^(n + 1)) / (1 - x)")]
        [InlineData("forall n in ZZ+ : sum(1 / k^2, k, 1, n) <= 2 - 1 / n")]
        [InlineData("forall n in ZZ+ : sum(1 / sqrt(k), k, 1, n) >= sqrt(n)")]
        // A denominator that is zero at a member: the identity is not claimed there.
        [InlineData("forall x in RR : x / (x - 1) - 1 / (x - 1) = 1")]
        [InlineData("forall n in ZZ : n / (n + 1) + 1 / ((n + 1) (n + 2)) = (n + 1) / (n + 2)")]
        public void LeftAsWritten(string statement)
            => Assert.IsType<Forallf>(statement.ToEntity().Evaled);

        [Theory]
        // An identity over atoms: whatever is not a polynomial connective is one indeterminate,
        // a whole shift in an exponent is a factor, and a factorial of a shifted argument is the
        // smaller factorial times the numbers in between.
        [InlineData("forall x in RR : (sin(x) + 1)^2 = sin(x)^2 + 2 sin(x) + 1", "True")]
        [InlineData("forall x in RR : (sin(x) + 1)^2 = sin(x)^2 + 2 sin(x) + 2", "False")]
        [InlineData("forall n in ZZ+ : -(1 - 2^(n + 1)) = 2^(n + 1) - 1", "True")]
        [InlineData("forall n in ZZ+ : 2^(n + 1) = 2 * 2^n", "True")]
        [InlineData("forall n in ZZ+ : x^(2 n) = (x^n)^2", "True")]
        [InlineData("forall n in ZZ+ : (n + 2)! = (n + 2) (n + 1) n!", "True")]
        [InlineData("forall n in ZZ+ : (n + 1)! + (n + 1) (n + 1)! = (n + 2)!", "True")]
        [InlineData("forall n in ZZ+ : n / (n + 1) + 1 / ((n + 1) (n + 2)) = (n + 1) / (n + 2)", "True")]
        // Over the whole numbers from 2 the body is closed: the set is not empty.
        [InlineData("forall n in ZZ+ /\\ [2; +oo) : (n + 1)^2 = n^2 + 2 n + 1", "True")]
        [InlineData("exists n in ZZ+ /\\ [2; 1] : n = n", "False")]
        public void AnIdentityOverAtoms(string statement, string expected)
            => Assert.Equal(expected.ToEntity(), statement.ToEntity().Evaled);
    }
}
