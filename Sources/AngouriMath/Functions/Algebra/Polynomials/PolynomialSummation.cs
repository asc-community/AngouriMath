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
    /// A summation whose summand is a polynomial in the index, written as a polynomial in the
    /// bounds: <c>sum(k, k, 1, n)</c> is <c>n^2/2 + n/2</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The operator writes itself out term by term where the bounds are concrete and few, and
    /// otherwise stayed as written — so a symbolic bound had no answer at all, and neither did a
    /// concrete range past a hundred terms. Both are the same gap: a sum of a polynomial has a
    /// closed form, and computing it is cheaper than the expansion it replaces.
    /// </para>
    /// <para>
    /// <b>No Bernoulli numbers.</b> The sum of a degree-<c>d</c> polynomial in the index is a
    /// polynomial of degree <c>d + 1</c> in the bound, which is the whole of what is needed: a
    /// polynomial of that degree is determined by <c>d + 2</c> of its values, and those values
    /// are sums of <c>d + 2</c> terms each, computed directly. Interpolating them recovers the
    /// coefficients exactly, in rational arithmetic, with no table to carry and no identity to
    /// get subtly wrong. Faulhaber's formula would give the same answer by a shorter route that
    /// needs Bernoulli numbers, which the library does not have.
    /// </para>
    /// <para>
    /// <b>The condition is not decoration.</b> <c>sum(k, k, 1, n)</c> is <em>not</em>
    /// <c>n^2/2 + n/2</c> for every <c>n</c>: at <c>n = -2</c> the range is empty and this
    /// library answers an empty range with <c>0</c>, while the polynomial gives <c>1</c>. The
    /// identity holds exactly when <c>to >= from - 1</c>, so that is what is attached, with the
    /// empty-range value as the other branch. Where the bounds are concrete the condition is
    /// decidable and the piecewise collapses to a number.
    /// </para>
    /// <para>
    /// SymPy answers the same input with the bare polynomial and is not making a mistake: it
    /// reads a reversed range as the negated sum over the flipped one, under which the identity
    /// is unconditional. This library defines an empty range as the operator's identity instead,
    /// and the condition is what that choice costs.
    /// </para>
    /// </remarks>
    internal static class PolynomialSummation
    {
        /// <summary>
        /// The degree of summand this will take. The work is cubic in it and the coefficients
        /// carry factorials of it, so the ceiling is about the size of the answer rather than
        /// about the method, which has no upper bound of its own.
        /// </summary>
        private const int MaxDegree = 16;

        /// <summary>
        /// <c>sum(expression, index, from, to)</c> written in closed form, or
        /// <see langword="null"/> where the summand is not a polynomial in the index.
        /// </summary>
        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            // A bound that is a number and not a whole one is not a bound this identity is about.
            // The index runs over the integers, so `sum(k, k, 1, 5/2)` is 1 + 2 = 3, while the
            // polynomial continued to 5/2 is 35/8 -- a different question, confidently answered.
            // `+oo` fails the same test, and so does every other non-integer number.
            if (!IsWholeOrSymbolic(from) || !IsWholeOrSymbolic(to))
                return null;
            if (!TreeAnalyzer.TryGetPolynomial(expression, index, out var monomials))
                return null;

            var degree = 0;
            foreach (var monomial in monomials)
            {
                if (monomial.Key.Sign < 0 || !monomial.Key.CanFitInInt32())
                    return null;
                var power = monomial.Key.ToInt32Checked();
                if (power > MaxDegree)
                    return null;
                // A coefficient that still mentions the index means the reading did not separate
                // them, and summing it as a constant would be answering a different question.
                if (monomial.Value.ContainsNode(index))
                    return null;
                if (power > degree)
                    degree = power;
            }

            // S(m) = sum over k from 1 to m of the summand, as a polynomial in m. One power sum
            // per power present, weighted by that power's coefficient.
            Entity AtBound(Entity bound)
            {
                Entity total = Integer.Create(0);
                foreach (var monomial in monomials)
                    total += monomial.Value * Evaluate(PowerSum(monomial.Key.ToInt32Checked()), bound);
                return total;
            }

            // sum from a to b is S(b) - S(a - 1), which holds for every pair of integers with
            // b >= a - 1 -- at b = a - 1 both sides are zero, and below it the range is empty
            // while the polynomial is not.
            var closed = (AtBound(to) - AtBound(from - 1)).InnerSimplified;
            var nonEmpty = new GreaterOrEqualf(to, from - 1);
            return MathS.Piecewise(new[]
            {
                new Providedf(closed, nonEmpty),
                new Providedf(Integer.Create(0), Entity.Boolean.True),
            }).InnerSimplified;
        }

        /// <summary>
        /// Whether a bound is one this identity can speak about: a whole number, or a name, of
        /// which the second is read as standing for a whole number because that is what the
        /// index of a summation ranges over.
        /// </summary>
        /// <remarks>
        /// Only a bound that <em>evaluates to a number</em> and is not whole is refused. That
        /// covers <c>5/2</c>, <c>+oo</c> and <c>1.5</c>, and leaves both a literal integer and
        /// anything symbolic to go through — the symbolic case being the one the closed form
        /// exists for.
        /// </remarks>
        private static bool IsWholeOrSymbolic(Entity bound)
            => bound.Evaled is Integer || bound.Evaled is not Number;

        /// <summary>
        /// The coefficients of <c>sum of k^power for k from 1 to m</c> as a polynomial in
        /// <c>m</c>, lowest power first.
        /// </summary>
        /// <remarks>
        /// A polynomial of degree <c>power + 1</c>, so <c>power + 2</c> of its values determine
        /// it. They are taken at <c>m = 0, 1, ..., power + 1</c>, where each is a sum of at most
        /// <c>power + 1</c> whole numbers, and Lagrange's formula turns them back into
        /// coefficients.
        /// </remarks>
        private static ERational[] PowerSum(int power)
        {
            var points = power + 2;
            var values = new ERational[points];
            var running = EInteger.Zero;
            for (var m = 0; m < points; m++)
            {
                if (m > 0)
                    running = running.Add(EInteger.FromInt32(m).Pow(power));
                values[m] = ERational.Create(running, EInteger.One);
            }
            return Interpolate(values);
        }

        /// <summary>
        /// The polynomial through <c>(i, values[i])</c> for each <c>i</c>, by Lagrange's formula,
        /// as coefficients lowest power first.
        /// </summary>
        private static ERational[] Interpolate(ERational[] values)
        {
            var result = new ERational[values.Length];
            for (var i = 0; i < result.Length; i++)
                result[i] = ERational.Zero;

            for (var i = 0; i < values.Length; i++)
            {
                // The basis polynomial for node i: the product of (x - j) over every other node,
                // divided by the same product evaluated at i.
                // Zeroed rather than left default: ERational is a class, so an unassigned entry
                // is null and the multiplication below reads every one of them.
                var basis = new ERational[values.Length];
                for (var k = 0; k < basis.Length; k++)
                    basis[k] = ERational.Zero;
                basis[0] = ERational.One;
                var filled = 1;
                var denominator = ERational.One;
                for (var j = 0; j < values.Length; j++)
                {
                    if (j == i)
                        continue;
                    // Multiply by (x - j), in place and from the top down so that a coefficient
                    // is read before it is overwritten.
                    for (var k = filled; k > 0; k--)
                        basis[k] = basis[k - 1].Subtract(basis[k].Multiply(ERational.FromInt32(j)));
                    basis[0] = basis[0].Multiply(ERational.FromInt32(-j));
                    filled++;
                    denominator = denominator.Multiply(ERational.FromInt32(i - j));
                }

                var scale = values[i].Divide(denominator);
                for (var k = 0; k < result.Length; k++)
                    result[k] = result[k].Add(basis[k].Multiply(scale));
            }
            return result;
        }

        /// <summary>
        /// A rational-coefficient polynomial evaluated at <paramref name="at"/>, as a flat sum of
        /// terms rather than by Horner: the answer is read by a person, and
        /// <c>n / 2 + n ^ 2 / 2</c> is the form this sum is known by, where the nested one is the
        /// same number written as nobody writes it.
        /// </summary>
        private static Entity Evaluate(ERational[] coefficients, Entity at)
        {
            Entity total = Integer.Create(0);
            for (var power = 0; power < coefficients.Length; power++)
            {
                if (coefficients[power].IsZero)
                    continue;
                var coefficient = Rational.Create(coefficients[power]);
                total += power switch
                {
                    0 => coefficient,
                    1 => coefficient * at,
                    _ => coefficient * MathS.Pow(at, Integer.Create(power)),
                };
            }
            return total;
        }
    }
}
