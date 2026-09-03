//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A product whose body is a monomial in the index, written in closed form:
    /// <c>product(k, k, 1, n)</c> is <c>factorial(n)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The sibling of <see cref="PolynomialSummation"/> and a narrower thing than it, because a
    /// product has no linearity to take apart: a sum of two terms is the sum of their sums, and a
    /// product of two terms is not the product of their products in any way that helps. What is
    /// left is the body that <em>is</em> a single term — <c>c * k^p</c> — over which the product
    /// separates into a power of the constant and a power of the factorial.
    /// </para>
    /// <para>
    /// <b>The condition is the same one, for the same reason.</b> An empty range multiplies to
    /// <c>1</c>, so <c>product(k, k, 1, n)</c> is <c>1</c> at every <c>n &lt; 1</c>, while
    /// <c>factorial(n)</c> is not — it is undefined at the negative integers, being the gamma
    /// function's poles. Answering the one with the other unconditionally would turn a value into
    /// an undefinedness, which is the failure the contract's O4 is about. The identity holds
    /// where <c>to >= from - 1</c>, and that is what is attached.
    /// </para>
    /// <para>
    /// <b>A positive lower bound where the index is in the body</b>, and that one cannot go in the
    /// condition. <c>product(k, k, a, b)</c> is <c>b! / (a-1)!</c> only where <c>a >= 1</c>; below
    /// that the range runs through zero and the product is <c>0</c>, while <c>(a-1)!</c> is
    /// undefined. But <c>a &lt; 1</c> does not mean the range is empty, so it cannot share a
    /// branch with the empty-range case — a piecewise saying "identity otherwise" would be wrong
    /// there. So it is decided before anything is built, and a lower bound that is not a concrete
    /// integer of at least one is declined instead. A constant body has no such restriction,
    /// there being no factorial in its answer.
    /// </para>
    /// </remarks>
    internal static class MonomialProduct
    {
        /// <summary>
        /// The largest power of the index this will take. The answer carries the factorial to
        /// that power, so the ceiling is about how big an expression is worth returning.
        /// </summary>
        private const int MaxPower = 16;

        /// <summary>
        /// <c>product(expression, index, from, to)</c> written in closed form, or
        /// <see langword="null"/> where the body is not a monomial in the index, or the bounds
        /// are not ones the identity speaks about.
        /// </summary>
        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            if (!PolynomialSummation.IsWholeOrSymbolic(from) || !PolynomialSummation.IsWholeOrSymbolic(to))
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(expression, index, out var monomials))
                return null;

            // One term, or this is not a monomial and the product does not separate.
            if (monomials.Count != 1)
                return null;
            var only = System.Linq.Enumerable.Single(monomials);
            if (only.Key.Sign < 0 || !only.Key.CanFitInInt32())
                return null;
            var power = only.Key.ToInt32Checked();
            if (power > MaxPower)
                return null;
            var coefficient = only.Value;
            if (coefficient.ContainsNode(index))
                return null;

            // How many times the body is multiplied: to - from + 1, which the condition below
            // guarantees is not negative.
            var count = (to - from + 1).InnerSimplified;
            Entity closed = MathS.Pow(coefficient, count);

            if (power > 0)
            {
                // b! / (a-1)! needs a - 1 to be a whole number that factorial is defined at,
                // which is decided here rather than asked in the condition -- see the remarks.
                if (from.Evaled is not Integer lower || lower.EInteger.Sign < 1)
                    return null;
                var factorials = (MathS.Factorial(to) / MathS.Factorial(from - 1)).InnerSimplified;
                closed *= power == 1 ? factorials : MathS.Pow(factorials, Integer.Create(power));
            }

            // `to >= from` and not `to >= from - 1`, which is where the sum's condition sits.
            // The two agree everywhere but the empty range itself, and there the closed form is
            // c^0 -- which is 1 for every c except 0, where it is undefined, while the empty
            // product is 1 for every c including 0. Handing the empty range to the identity
            // branch instead keeps a value from becoming an undefinedness, and costs nothing:
            // both branches say 1 there.
            return MathS.Piecewise(new[]
            {
                new Providedf(closed.InnerSimplified, new GreaterOrEqualf(to, from)),
                new Providedf(Integer.Create(1), Entity.Boolean.True),
            }).InnerSimplified;
        }
    }
}
