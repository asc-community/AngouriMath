//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Bareiss' fraction-free elimination: the determinant of a square matrix over an
    /// integral domain, in <c>O(n^3)</c> ring operations and without ever leaving the ring.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Ordinary Gaussian elimination divides by the pivot, so over a ring of polynomials it
    /// produces quotients — and an expression built from them is undefined wherever a pivot
    /// vanishes, at points where the determinant itself is perfectly well defined. That is
    /// what <a href="https://github.com/asc-community/AngouriMath/issues/992">#992</a> was
    /// about, and why <see cref="Entity.Matrix.Determinant"/> used Laplace expansion rather
    /// than elimination.
    /// </para>
    /// <para>
    /// Bareiss divides too, and the divisions are the point: each entry is divided by the
    /// <em>previous</em> pivot, and that division is <b>exact</b> — the quotient is a
    /// determinant of a minor and therefore back in the ring. So the intermediate entries do
    /// not swell the way Gaussian elimination's do, and nothing is ever left as a quotient to
    /// exclude a point.
    /// </para>
    /// <para>
    /// <b>Exactness is a fact about the ring, and is checked here anyway.</b>
    /// <see cref="MultivariatePolynomial.DivideExact(MultivariatePolynomial, int)"/> returns
    /// null rather than a remainder, so a division that does not come out — because the term
    /// ceiling was reached, or because an assumption above is wrong — stops the elimination
    /// instead of producing an answer that is quietly not the determinant. Every caller reads
    /// null as "use another method", never as "there is no determinant".
    /// </para>
    /// <para>
    /// Split out of <see cref="PolynomialResultant"/>, where it was written for the Sylvester
    /// matrix. Shared rather than written twice, deliberately: a sign convention or an
    /// off-by-one in an elimination is a wrong answer that looks entirely plausible, and one
    /// implementation exercised by two callers is tested by both.
    /// <a href="https://github.com/asc-community/AngouriMath/issues/999">#999</a>
    /// </para>
    /// </remarks>
    internal static class FractionFreeDeterminant
    {
        [ConstantField] private static readonly ERational MinusOne = ERational.One.Negate();

        /// <summary>
        /// The determinant of <paramref name="matrix"/>, or <see langword="null"/> where the
        /// elimination could not finish within <paramref name="maxWork"/> or a division came
        /// back inexact.
        /// </summary>
        /// <param name="matrix">
        /// Square, and <b>modified in place</b> — the caller owns it and must not need it
        /// afterwards. Elimination is destructive, and copying here would hide the cost of the
        /// one thing this exists to make cheap.
        /// </param>
        /// <param name="variableCount">
        /// How many variables the entries are over, needed to build the zero and the one of the
        /// same ring. Not derivable from the entries, since a matrix may be all constants.
        /// </param>
        /// <param name="maxWork">
        /// A ceiling on term-pair multiplications, charged <em>before</em> each multiplication
        /// rather than after, so that the budget cannot be overshot by the one step that was
        /// going to be the most expensive of them.
        /// </param>
        internal static MultivariatePolynomial? Of(
            MultivariatePolynomial[][] matrix, int variableCount, long maxWork)
        {
            var size = matrix.Length;
            if (size == 0)
                return MultivariatePolynomial.One(variableCount);

            var zero = MultivariatePolynomial.Zero(variableCount);
            var negated = false;
            var work = 0L;
            var previous = MultivariatePolynomial.One(variableCount);

            for (var pivot = 0; pivot + 1 < size; pivot++)
            {
                MultithreadingFunctional.ExitIfCancelled();
                if (matrix[pivot][pivot].IsZero)
                {
                    var replacement = -1;
                    for (var row = pivot + 1; row < size && replacement < 0; row++)
                        if (!matrix[row][pivot].IsZero)
                            replacement = row;
                    // Nothing below the pivot to bring up means the column is a combination
                    // of the ones before it, so the matrix is singular and its determinant
                    // is zero. That is an answer, not a failure.
                    if (replacement < 0)
                        return zero;
                    (matrix[pivot], matrix[replacement]) = (matrix[replacement], matrix[pivot]);
                    negated = !negated;
                }
                var head = matrix[pivot][pivot];
                for (var row = pivot + 1; row < size; row++)
                {
                    MultithreadingFunctional.ExitIfCancelled();
                    var leading = matrix[row][pivot];
                    for (var column = pivot + 1; column < size; column++)
                    {
                        work += (long)head.TermCount * matrix[row][column].TermCount
                            + (long)leading.TermCount * matrix[pivot][column].TermCount;
                        if (work > maxWork)
                            return null;
                        if (head.Multiply(matrix[row][column]) is not { } kept
                            || leading.Multiply(matrix[pivot][column]) is not { } removed
                            || kept.Subtract(removed).DivideExact(previous) is not { } reduced)
                            return null;
                        matrix[row][column] = reduced;
                    }
                    matrix[row][pivot] = zero;
                }
                previous = head;
            }

            var determinant = matrix[size - 1][size - 1];
            return negated ? determinant.ScaleBy(MinusOne) : determinant;
        }
    }
}
