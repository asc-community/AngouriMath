//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    /// <summary>
    /// The determinant of a matrix whose entries are polynomials over the rationals, by
    /// Bareiss' fraction-free elimination — <c>O(n^3)</c> where Laplace expansion is
    /// <c>O(n!)</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Laplace is not merely slower. For a <b>fully symbolic</b> matrix it is optimal, because
    /// the determinant genuinely has <c>n!</c> terms and no algorithm returns it smaller in
    /// expanded form. For a <b>numeric</b> one it is pure waste: the answer is a single number
    /// and <c>O(n^3)</c> work suffices. That is the split
    /// <a href="https://github.com/asc-community/AngouriMath/issues/999">#999</a> asks for, and
    /// what decides it is not the size but whether the entries are polynomials this can read —
    /// which is settled per matrix, by trying, rather than by a rule about <c>n</c>.
    /// </para>
    /// <para>
    /// <b>Why this cannot introduce a condition.</b> Bareiss divides, and the usual reason to
    /// distrust an elimination is that its divisions leave quotients excluding the points where
    /// a pivot vanishes — the defect
    /// <a href="https://github.com/asc-community/AngouriMath/issues/992">#992</a> was about. Its
    /// divisions are exact over an integral domain, and here they are exact <em>and checked</em>:
    /// the arithmetic happens in <see cref="MultivariatePolynomial"/>, which has no quotients to
    /// leave behind at all, and a division that does not come out returns null and sends the
    /// caller to Laplace. So the answer is a polynomial in the entries, or it is Laplace's.
    /// </para>
    /// <para>
    /// <b>What it declines.</b> An entry that is not a polynomial over the rationals — anything
    /// with <c>sin</c>, a symbolic exponent, a genuine <c>1/x</c> — a matrix in more than
    /// <see cref="MultivariatePolynomial.MaxVariables"/> indeterminates, and a matrix mentioning
    /// a <see cref="Entity.Constant"/>, since <c>e</c> and <c>pi</c> are values rather than
    /// indeterminates and this ring cannot hold them. Each is a refusal to try, not a wrong
    /// answer, and Laplace answers them exactly as before.
    /// </para>
    /// </remarks>
    internal static class PolynomialDeterminant
    {
        /// <summary>
        /// A ceiling on term-pair multiplications, past which this declines and Laplace answers
        /// instead.
        /// </summary>
        /// <remarks>
        /// Deliberately far above <c>PolynomialResultant</c>'s, and for a different reason.
        /// There, exceeding the budget means the resultant is not computed at all and a caller
        /// loses an answer. Here it means falling back to an algorithm that also answers, so a
        /// generous budget risks only spending longer before choosing the other method — and the
        /// case this exists for, a large mostly-numeric matrix, is one where Laplace would not
        /// have finished at all.
        /// </remarks>
        private const long MaxEliminationWork = 50_000_000;

        /// <summary>
        /// The determinant of the <paramref name="size"/>-by-<paramref name="size"/> matrix whose
        /// entries are <paramref name="at"/>, or <see langword="null"/> where this method does
        /// not apply and the caller should use another.
        /// </summary>
        internal static Entity? Of(int size, Func<int, int, Entity> at)
        {
            if (size == 0)
                return null;

            // The variables of the whole matrix, in a fixed order, so that every entry is read
            // into the same ring. Ordered by name rather than by encounter: the packed monomial
            // addresses a variable by index, so two entries disagreeing about which index a name
            // has would be a wrong answer rather than a failure.
            //
            // The Variable objects themselves are kept, not their names: a name is not
            // guaranteed to parse back to the variable it came from -- `x2` reads as `x ^ 2`,
            // which is the implicit-power rule the grammar has for a reason -- so going through
            // MathS.Var would throw on a name a caller is perfectly entitled to have built.
            var byName = new SortedSet<Variable>(
                Comparer<Variable>.Create(
                    (left, right) => string.CompareOrdinal(left.Name, right.Name)));
            for (var row = 0; row < size; row++)
                for (var column = 0; column < size; column++)
                    foreach (var variable in at(row, column).Vars)
                        byName.Add(variable);
            if (byName.Count > MultivariatePolynomial.MaxVariables)
                return null;

            var variables = byName.ToList();
            var index = new Dictionary<Variable, int>();
            for (var i = 0; i < variables.Count; i++)
                index[variables[i]] = i;

            var matrix = new MultivariatePolynomial[size][];
            for (var row = 0; row < size; row++)
            {
                matrix[row] = new MultivariatePolynomial[size];
                for (var column = 0; column < size; column++)
                {
                    if (MultivariatePolynomial.TryParse(at(row, column), index)
                        is not { } parsed)
                        return null;
                    matrix[row][column] = parsed;
                }
            }

            if (FractionFreeDeterminant.Of(matrix, variables.Count, MaxEliminationWork)
                is not { } determinant)
                return null;
            return determinant.ToEntity(variables);
        }
    }
}
