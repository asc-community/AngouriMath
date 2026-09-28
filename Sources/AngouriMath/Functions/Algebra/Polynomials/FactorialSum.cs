//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using System.Linq;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A polynomial times the factorial of the index, summed in closed form where the sum
    /// telescopes: <c>sum(k k!, k, 1, n)</c> is <c>(n + 1)! - 1</c>, Sullivan and Mackey's Prob 2.7.17.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>P(k) k!</c> is <c>T(k + 1) - T(k)</c> for <c>T(k) = Q(k) k!</c> exactly where
    /// <c>(k + 1) Q(k + 1) - Q(k) = P(k)</c>, and then the sum from <c>a</c> to <c>b</c> is
    /// <c>T(b + 1) - T(a)</c>. The equation is triangular in <c>Q</c>'s coefficients from the top
    /// down, the coefficient of <c>k^(m + 1)</c> fixing <c>q_m</c>, and its constant term is one
    /// condition more: <c>q_1 + q_2 + ... = p_0</c>. Where that fails, no polynomial <c>Q</c> exists
    /// and the sum is declined: <c>sum(k!, k, 0, n)</c>, the left factorial, has no closed form in
    /// factorials. This is Gosper's algorithm for the one family, where the certificate is a
    /// polynomial.
    /// </para>
    /// <para>
    /// Over <c>a &gt;= 0</c>, where <c>a!</c> is defined; a range that runs backwards is empty and
    /// the sum zero, which is what the piecewise says, while the closed form at <c>b = a - 1</c> is
    /// zero already.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class FactorialSum
    {
        private const int MaxDegree = 12;

        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            if (!PolynomialSummation.IsWholeOrSymbolic(from) || !PolynomialSummation.IsWholeOrSymbolic(to)
                || from.ContainsNode(index) || to.ContainsNode(index) || from.Evaled is Integer { IsNegative: true })
                return null;
            Entity? polynomial = null;
            var sawFactorial = false;
            foreach (var factor in Mulf.LinearChildren(expression.InnerSimplified))
            {
                if (factor is Factorialf(var argument) && argument == index && !sawFactorial)
                    sawFactorial = true;
                else
                    polynomial = polynomial is null ? factor : polynomial * factor;
            }
            if (!sawFactorial || polynomial is null || !polynomial.ContainsNode(index))
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(polynomial, index, out var monomials)
                || monomials.Keys.Any(power => power.Sign < 0 || !power.CanFitInInt32()))
                return null;
            var degree = monomials.Keys.Max()!.ToInt32Checked();
            if (degree < 1 || degree > MaxDegree)
                return null;
            Entity P(int m) => monomials.TryGetValue(EInteger.FromInt32(m), out var c) ? c : Integer.Zero;

            // q_d .. q_0 from the coefficients of k^(d + 1) .. k^1 of (k + 1) Q(k + 1) - Q(k),
            // which is sum_j q_j sum_i C(j + 1, i) k^i - sum_j q_j k^j.
            var d = degree - 1;
            var q = new Entity[d + 1];
            for (var m = d + 1; m >= 1; m--)
            {
                // The coefficient of k^m: q_(m - 1) + sum_(j >= m) q_j C(j + 1, m) - q_m.
                Entity known = P(m);
                for (var j = m; j <= d; j++)
                    known -= Integer.Create(Binomial(j + 1, m)) * q[j];
                if (m <= d)
                    known += q[m];
                q[m - 1] = known.InnerSimplified;
            }
            // The constant term: sum_(j >= 1) q_j = p_0, or there is no polynomial certificate.
            Entity rest = Integer.Zero;
            for (var j = 1; j <= d; j++)
                rest += q[j];
            if (PartialFractions.Bare((rest - P(0)).Simplify()) is not Integer { IsZero: true })
                return null;
            // The constant term written as itself: at a = 0 a power 0^0 would stand in for it.
            Entity Q(Entity at)
            {
                Entity value = q[0];
                for (var j = 1; j <= d; j++)
                    value += q[j] * MathS.Pow(at, Integer.Create(j));
                return value;
            }
            var next = (to + Integer.One).InnerSimplified;
            var closed = (Q(next) * MathS.Factorial(next) - Q(from) * MathS.Factorial(from)).InnerSimplified;
            if (to.Evaled is Integer && from.Evaled is Integer)
                return closed;
            return MathS.Piecewise(new[]
            {
                new Providedf(closed, new GreaterOrEqualf(to, (from - Integer.One).InnerSimplified)),
                new Providedf(Integer.Zero, Entity.Boolean.True),
            }).InnerSimplified;
        }

        private static EInteger Binomial(int n, int k)
        {
            var result = EInteger.One;
            for (var i = 0; i < k; i++)
                result = result.Multiply(EInteger.FromInt32(n - i)).Divide(EInteger.FromInt32(i + 1));
            return result;
        }
    }
}
