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
    /// The binomial sums of chapter 8 of Sullivan and Mackey's <i>An Introduction to Proofs</i>
    /// that are not the binomial theorem itself, in closed form: a polynomial in the index
    /// beside the coefficient (Prop 8.4.4, <c>sum(k binomial(n, k), k, 0, n)</c> is
    /// <c>n 2^(n - 1)</c>), Vandermonde's convolution (Prob 8.9.16,
    /// <c>sum(binomial(a, i) binomial(b, k - i), i, 0, k)</c> is <c>binomial(a + b, k)</c>) and
    /// its square case (Prob 8.9.34, <c>sum(binomial(n, k)^2, k, 0, n)</c> is
    /// <c>binomial(2n, n)</c>), the summation identity (Thm 8.4.6,
    /// <c>sum(binomial(i, k), i, 0, n)</c> is <c>binomial(n + 1, k + 1)</c>), and the sums over
    /// the even or the odd indices (Ex 8.3.11, each <c>2^(n - 1)</c> for <c>n &gt;= 1</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>A polynomial beside the coefficient</b> is read in the falling factorial basis,
    /// <c>k^m = sum_j S(m, j) k (k - 1) ... (k - j + 1)</c> with the Stirling numbers of the second
    /// kind, because <c>k (k - 1) ... (k - j + 1) binomial(n, k)</c> is
    /// <c>n (n - 1) ... (n - j + 1) binomial(n - j, k - j)</c> -- the chairperson identity
    /// applied <c>j</c> times -- so <c>sum_k k^(j falling) binomial(n, k) x^k y^(n - k)</c> is
    /// <c>n^(j falling) x^j (x + y)^(n - j)</c> by the binomial theorem on what is left. Exact for
    /// every whole <c>n &gt;= 0</c> and every <c>x</c>, <c>y</c>; the empty range below zero is
    /// the piecewise <see cref="PolynomialSummation"/> attaches.
    /// </para>
    /// <para>
    /// <b>Vandermonde</b> holds as an identity in <c>a</c> and <c>b</c> for whole <c>a, b &gt;= 0</c>
    /// and a whole <c>k >= 0</c>, the terms outside <c>0 &lt;= i &lt;= min(a, k)</c> being zero, so the
    /// range may be written to <c>k</c>, to <c>a</c>, or to <c>b</c> where the second coefficient
    /// is <c>binomial(b, k - i)</c>. The square is the case <c>a = b = k = n</c> with
    /// <c>binomial(n, k) = binomial(n, n - k)</c>. The summation identity is Pascal's rule
    /// telescoped. Part of <a href="https://github.com/asc-community/AngouriMath/issues/1409">#1409</a>.
    /// </para>
    /// </remarks>
    internal static class BinomialIdentities
    {
        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            if (from.Evaled is not Integer { IsZero: true })
                return null;
            if (to.ContainsNode(index) || !PolynomialSummation.IsWholeOrSymbolic(to))
                return null;
            var upper = to.InnerSimplified;
            var summand = expression.InnerSimplified;

            return PolynomialBesideTheCoefficient(summand, index, upper)
                ?? Vandermonde(summand, index, upper)
                ?? SummationIdentity(summand, index, upper)
                ?? OverTheEvenOrTheOddIndices(expression, index, to);
        }

        /// <summary>
        /// <c>p(k) binomial(n, k) x^k y^(n - k)</c> summed to <c>n</c>, for a polynomial <c>p</c> of
        /// degree at least one; the degree-zero case is <see cref="BinomialSum"/>'s.
        /// </summary>
        private static Entity? PolynomialBesideTheCoefficient(Entity summand, Variable index, Entity upper)
        {
            Entity? polynomial = null;
            var sawCoefficient = false;
            // ...or the coefficient as its three factorials, n!/(k! (n - k)!), the way
            // BinomialSum reads it.
            var sawTop = false;
            var sawBottomK = false;
            var sawBottomRest = false;
            Entity? powerOfK = null;
            Entity? powerOfRest = null;
            Entity constant = Integer.One;
            foreach (var factor in Mulf.LinearChildren(summand))
            {
                if (!factor.ContainsNode(index))
                {
                    if (!sawTop && factor is Factorialf(var topFactorial) && topFactorial == upper)
                        sawTop = true;
                    else
                        constant *= factor;
                }
                else if (factor is Binomialf(var top, var bottom) && top == upper && bottom == index && !sawCoefficient)
                    sawCoefficient = true;
                else if (factor is Powf(Factorialf(var argument), var power) && power == Integer.MinusOne && argument == index && !sawBottomK)
                    sawBottomK = true;
                else if (factor is Powf(Factorialf(var argument2), var power2) && power2 == Integer.MinusOne
                         && argument2 is Minusf(var upperLeft, var indexRight) && upperLeft == upper && indexRight == index && !sawBottomRest)
                    sawBottomRest = true;
                else if (factor is Powf(var b, var e) && !b.ContainsNode(index) && e == index && powerOfK is null)
                    powerOfK = b;
                else if (factor is Powf(var b2, var e2) && !b2.ContainsNode(index) && e2 is Minusf(var left, var right) && left == upper && right == index && powerOfRest is null)
                    powerOfRest = b2;
                else
                    polynomial = polynomial is null ? factor : polynomial * factor;
            }
            // The top factorial is a constant the identity does not need, and a concrete one
            // has folded into a number before the factors are read: 130! is not a Factorialf
            // by then, so the two below on their own are the coefficient over n!.
            var overTheTopFactorial = false;
            if (sawBottomK && sawBottomRest)
                (sawCoefficient, overTheTopFactorial) = (true, !sawTop);
            else if (sawTop || sawBottomK || sawBottomRest)
                return null;
            if (!sawCoefficient || polynomial is null)
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(polynomial, index, out var monomials) || monomials.Count == 0
                || monomials.Keys.Any(power => power.Sign < 0 || !power.CanFitInInt32()))
                return null;
            var degree = monomials.Keys.Max()!.ToInt32Checked();
            if (degree < 1 || degree > 12)
                return null;

            // p in the falling factorial basis: the coefficient of k^(j falling) is
            // sum_m p_m S(m, j).
            var stirling = StirlingSecondKind(degree);
            var x = powerOfK ?? Integer.One;
            var y = powerOfRest ?? Integer.One;
            var xPlusY = (x + y).InnerSimplified;
            Entity closed = Integer.Zero;
            for (var j = 0; j <= degree; j++)
            {
                Entity coefficient = Integer.Zero;
                for (var m = j; m <= degree; m++)
                    if (monomials.TryGetValue(EInteger.FromInt32(m), out var pm))
                        coefficient += Integer.Create(stirling[m][j]) * pm;
                coefficient = coefficient.InnerSimplified;
                if (coefficient == Integer.Zero)
                    continue;
                // n^(j falling) x^j (x + y)^(n - j)
                Entity falling = Integer.One;
                for (var i = 0; i < j; i++)
                    falling *= i == 0 ? upper : upper - Integer.Create(i);
                Entity term = coefficient * falling;
                if (j > 0 && x != Integer.One)
                    term *= MathS.Pow(x, j);
                term *= MathS.Pow(xPlusY, j == 0 ? upper : (upper - Integer.Create(j)).InnerSimplified);
                closed += term;
            }
            if (overTheTopFactorial)
                closed /= MathS.Factorial(upper);
            return Ranged((constant * closed).InnerSimplified, upper);
        }

        /// <summary>
        /// <c>binomial(a, i) binomial(b, c - i)</c>, or the square <c>binomial(n, i)^2</c>, summed
        /// from zero to <c>c</c>, <c>a</c> or <c>b</c>.
        /// </summary>
        private static Entity? Vandermonde(Entity summand, Variable index, Entity upper)
        {
            Entity? a = null;
            Entity? b = null;
            Entity? c = null;
            Entity constant = Integer.One;
            foreach (var factor in Mulf.LinearChildren(summand))
            {
                if (!factor.ContainsNode(index))
                    constant *= factor;
                else if (factor is Powf(Binomialf(var top, var bottom), Integer two) && two == Integer.Create(2) && bottom == index && !top.ContainsNode(index) && a is null && b is null)
                    (a, b, c) = (top, top, top);
                else if (factor is Binomialf(var top2, var bottom2) && !top2.ContainsNode(index) && bottom2 == index && a is null)
                    a = top2;
                else if (factor is Binomialf(var top3, var bottom3) && !top3.ContainsNode(index) && bottom3 is Minusf(var left, var right) && right == index && !left.ContainsNode(index) && b is null)
                    (b, c) = (top3, left);
                else
                    return null;
            }
            if (a is null || b is null || c is null)
                return null;
            if (upper != c && upper != a && upper != b)
                return null;
            return Ranged((constant * MathS.Binomial((a == b ? Integer.Create(2) * a : a + b).InnerSimplified, c)).InnerSimplified, upper);
        }

        /// <summary><c>binomial(i, k)</c> summed over <c>i</c> from zero to <c>n</c>: <c>binomial(n + 1, k + 1)</c>.</summary>
        private static Entity? SummationIdentity(Entity summand, Variable index, Entity upper)
        {
            Entity constant = Integer.One;
            Entity? k = null;
            foreach (var factor in Mulf.LinearChildren(summand))
            {
                if (!factor.ContainsNode(index))
                    constant *= factor;
                else if (factor is Binomialf(var top, var bottom) && top == index && !bottom.ContainsNode(index) && k is null)
                    k = bottom;
                else
                    return null;
            }
            if (k is null)
                return null;
            return Ranged((constant * MathS.Binomial((upper + Integer.One).InnerSimplified, (k + Integer.One).InnerSimplified)).InnerSimplified, upper);
        }

        /// <summary>
        /// <c>binomial(n, 2 l)</c> summed to <c>floor(n/2)</c>, and <c>binomial(n, 2 l + 1)</c> to
        /// <c>floor((n - 1)/2)</c>: each is <c>2^(n - 1)</c> for <c>n &gt;= 1</c>, the two halves of
        /// <c>2^n</c> being equal since their difference is <c>(1 - 1)^n</c>; at <c>n = 0</c> the
        /// even sum is <c>1</c> and the odd one is empty.
        /// </summary>
        private static Entity? OverTheEvenOrTheOddIndices(Entity expression, Variable index, Entity to)
        {
            if (expression is not Binomialf(var n, var bottom) || n.ContainsNode(index))
                return null;
            bool? odd = bottom switch
            {
                Mulf(Integer two, var l) when two == Integer.Create(2) && l == index => false,
                Mulf(var l, Integer two) when two == Integer.Create(2) && l == index => false,
                Sumf(Mulf(Integer two, var l), Integer one) when two == Integer.Create(2) && l == index && one == Integer.One => true,
                Sumf(Mulf(var l, Integer two), Integer one) when two == Integer.Create(2) && l == index && one == Integer.One => true,
                _ => null,
            };
            if (odd is null)
                return null;
            Entity expectedTop = odd.Value ? MathS.Floor((n - Integer.One) / Integer.Create(2)) : MathS.Floor(n / Integer.Create(2));
            if (to.InnerSimplified != expectedTop.InnerSimplified && to != expectedTop)
                return null;
            var half = MathS.Pow(Integer.Create(2), (n - Integer.One).InnerSimplified);
            if (n.Evaled is Integer whole)
                return whole.EInteger.Sign > 0 ? half.InnerSimplified : (odd.Value ? Integer.Zero : Integer.One);
            return MathS.Piecewise(new[]
            {
                new Providedf(half, new GreaterOrEqualf(n, Integer.One)),
                new Providedf(odd.Value ? Integer.Zero : Integer.One, new Equalsf(n, Integer.Zero)),
                new Providedf(Integer.Zero, Entity.Boolean.True),
            }).InnerSimplified;
        }

        /// <summary>The closed form over a range that is empty below <c>upper = 0</c>.</summary>
        private static Entity Ranged(Entity closed, Entity upper)
            => upper.Evaled is Integer
                ? closed
                : MathS.Piecewise(new[]
                {
                    new Providedf(closed, new GreaterOrEqualf(upper, Integer.Zero)),
                    new Providedf(Integer.Zero, Entity.Boolean.True),
                }).InnerSimplified;

        /// <summary>The Stirling numbers of the second kind <c>S(m, j)</c> for <c>m, j</c> up to <paramref name="degree"/>.</summary>
        private static EInteger[][] StirlingSecondKind(int degree)
        {
            var s = new EInteger[degree + 1][];
            for (var m = 0; m <= degree; m++)
            {
                s[m] = new EInteger[degree + 1];
                for (var j = 0; j <= degree; j++)
                    s[m][j] = EInteger.Zero;
            }
            s[0][0] = EInteger.One;
            for (var m = 1; m <= degree; m++)
                for (var j = 1; j <= m; j++)
                    s[m][j] = EInteger.FromInt32(j).Multiply(s[m - 1][j]).Add(s[m - 1][j - 1]);
            return s;
        }
    }
}
