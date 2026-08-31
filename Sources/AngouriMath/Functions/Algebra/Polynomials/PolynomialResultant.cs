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
    /// The resultant of two polynomials in several variables over the rationals, and the
    /// discriminant that follows from it. Eliminating a variable between two equations is
    /// what these are for: <c>Res(f, g)</c> taken in <c>y</c> vanishes exactly where
    /// <c>f</c> and <c>g</c> have a common <c>y</c>, so it is the condition on the
    /// remaining variables that the pair be solvable.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The resultant is defined here as the determinant of the Sylvester matrix, and it is
    /// computed as one. That is deliberate. The remainder-sequence formulations are faster,
    /// but each carries a factor <c>(-1)^k</c> and a power of a leading coefficient that has
    /// to be tracked through every step, and a sign convention got wrong there is a wrong
    /// answer that looks entirely plausible. Taken as a determinant, the sign convention is
    /// the one property that cannot be got wrong: <c>Res(f, g) = (-1)^(deg f * deg g)
    /// Res(g, f)</c> and <c>Res(f, g) = lc(f)^deg g * lc(g)^deg f * prod (a_i - b_j)</c>
    /// both fall out of the matrix rather than being imposed on it.
    /// </para>
    /// <para>
    /// The determinant is taken by one-step fraction-free elimination, which is the same
    /// idea as the subresultant remainder sequence in <see cref="PolynomialGcd"/> and for
    /// the same reason: every intermediate entry is a minor of the original matrix, so the
    /// division at each step comes out exact and the coefficients stay the size of those
    /// minors instead of compounding. Bareiss, <i>Sylvester's identity and multistep
    /// integer-preserving Gaussian elimination</i>, Math. Comp. 22 (1968); Geddes, Czapor
    /// and Labahn, <i>Algorithms for Computer Algebra</i>, §9.3; Knuth, <i>TAOCP</i> vol. 2,
    /// §4.6.1.
    /// </para>
    /// <para>
    /// The degenerate cases are the ones implementations usually differ on, and these were
    /// measured against SymPy 1.14 rather than recalled: a zero argument gives zero whatever
    /// the other side is, two arguments free of the main variable give one, and
    /// <c>Res(f, c) = c^deg f</c>. All three are what the Sylvester matrix already says once
    /// a polynomial free of the main variable is read as having degree zero — the matrix is
    /// then diagonal, or empty, and an empty determinant is one.
    /// </para>
    /// <para>
    /// Part of the polynomial layer of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>, item 43.
    /// </para>
    /// </remarks>
    internal static class PolynomialResultant
    {
        /// <summary>
        /// The bound on <c>deg f + deg g</c>. It exists to stop the matrix being built at
        /// all; what the elimination then costs is bounded by
        /// <see cref="MaxEliminationWork"/> instead, and the two are not the same question.
        /// </summary>
        /// <remarks>
        /// Size is the cheap axis, which is why this is generous. With scalar entries the
        /// elimination does <c>size^3 / 3</c> units of the work counted below, and 40 measured
        /// 31ms and 35MB of allocation — where the same size with three-term entries costs 22
        /// seconds and 79GB. So a ceiling on size alone can only be right for one shape of
        /// input at a time, and this one is set for the shape it can actually decide: 40
        /// admits the discriminant of anything up to degree 20, since <c>deg f + deg f'</c> is
        /// <c>2 deg f - 1</c>. https://github.com/asc-community/AngouriMath/issues/921
        /// </remarks>
        private const int MaxSylvesterSize = 40;

        /// <summary>
        /// The budget on the elimination itself, in units of one coefficient-pair
        /// multiplication: each step adds the product of the term counts of the two operands
        /// of each of its two products, which is what a sparse multiplication costs.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Measured rather than reasoned, across the two axes that matter — Sylvester size,
        /// and terms per entry. Over the whole range the unit predicts both resources
        /// linearly and tightly: <b>1.4KB of allocation and 0.4µs per unit</b>. So this budget
        /// is about a second, and about three gigabytes of garbage, and moving it moves both
        /// together.
        /// </para>
        /// <para>
        /// A bound on the input shape cannot do this job, which is the measurement's real
        /// finding. The cost is mild in size and violent in terms per entry — between the
        /// fifth and the eighth power of it over the range — so no product of the two is the
        /// right law. Worse, <b>the most expensive input is not the widest</b>: entries wide
        /// enough trip <see cref="MultivariatePolynomial.MaxTerms"/> early and the elimination
        /// declines cheaply, while three-term entries grow just slowly enough to spend 6
        /// seconds and 21GB before declining at size 24. The shape that costs the most is the
        /// one just inside the term ceiling, and no bound read off the degrees can see it.
        /// </para>
        /// <para>
        /// Set to admit every case that answered at all in the sweep bar one — two-term
        /// entries at size 40, which answers in 2.9 seconds having allocated 8.4GB — and to
        /// refuse the expensive refusals, which is where it earns its keep.
        /// </para>
        /// </remarks>
        private const long MaxEliminationWork = 2_500_000;

        [ConstantField] private static readonly ERational MinusOne = ERational.One.Negate();

        /// <summary>
        /// The resultant of two polynomials with respect to <paramref name="mainVariable"/>,
        /// itself a polynomial in the remaining variables; null where a step declines.
        /// </summary>
        /// <remarks>
        /// Identically zero exactly when the two share a factor of positive degree in
        /// <paramref name="mainVariable"/>. Where it is not identically zero it vanishes at
        /// a point of the remaining variables exactly where the two polynomials specialised
        /// there have a common root, or where both of their leading coefficients in
        /// <paramref name="mainVariable"/> vanish — the second is why an eliminant can carry
        /// a root the original system does not have.
        /// </remarks>
        internal static MultivariatePolynomial? Resultant(
            MultivariatePolynomial left, MultivariatePolynomial right,
            int mainVariable, IReadOnlyList<int> otherVariables)
        {
            var variableCount = left.VariableCount;

            // Zero is divisible by everything, so the two share a factor of every degree
            // there is; SymPy answers zero here even against a nonzero constant.
            if (left.IsZero || right.IsZero)
                return MultivariatePolynomial.Zero(variableCount);

            var leftDegree = left.DegreeIn(mainVariable);
            var rightDegree = right.DegreeIn(mainVariable);
            if (leftDegree == 0 && rightDegree == 0)
                return MultivariatePolynomial.One(variableCount);
            if (leftDegree + rightDegree > MaxSylvesterSize)
                return null;

            // Res(c * f, g) = c^deg g * Res(f, g) for a c free of the main variable, and
            // symmetrically on the right. Taking the content out first keeps the entries of
            // the matrix — and every minor built from them — clear of a factor that would
            // otherwise be carried through the whole elimination and raised to a power of
            // the size of the matrix.
            var leftContent = ContentOrOne(left, mainVariable, otherVariables);
            var rightContent = ContentOrOne(right, mainVariable, otherVariables);
            var primitiveLeft = left;
            var primitiveRight = right;
            var restored = MultivariatePolynomial.One(variableCount);
            if (!leftContent.IsConstant || !rightContent.IsConstant)
            {
                if (leftContent.Power(rightDegree) is not { } leftFactor
                    || rightContent.Power(leftDegree) is not { } rightFactor
                    || leftFactor.Multiply(rightFactor) is not { } factor
                    || !TryDivideOut(left, leftContent, out primitiveLeft)
                    || !TryDivideOut(right, rightContent, out primitiveRight))
                {
                    // Nothing is lost by leaving the content in: the split is worth doing
                    // and not worth refusing over.
                    primitiveLeft = left;
                    primitiveRight = right;
                }
                else
                    restored = factor;
            }

            if (SylvesterDeterminant(primitiveLeft, primitiveRight, mainVariable) is not { } determinant)
                return null;
            return restored.Multiply(determinant);
        }

        /// <summary>
        /// disc(f) with respect to <paramref name="mainVariable"/>, related to the resultant
        /// of f and its derivative by disc(f) = (-1)^(n(n-1)/2) * Res(f, f') / lc(f).
        /// </summary>
        /// <remarks>
        /// Zero exactly when f has a repeated factor in <paramref name="mainVariable"/> —
        /// including when it has no <paramref name="mainVariable"/> at all, where the
        /// derivative vanishes and the resultant with it.
        /// </remarks>
        internal static MultivariatePolynomial? Discriminant(
            MultivariatePolynomial poly, int mainVariable, IReadOnlyList<int> otherVariables)
        {
            var degree = poly.DegreeIn(mainVariable);
            if (Resultant(poly, poly.DerivativeIn(mainVariable), mainVariable, otherVariables)
                is not { } resultant)
                return null;
            if (resultant.IsZero)
                return resultant;

            // The quotient is a polynomial in the coefficients of f, so this division is
            // exact whenever the resultant was computed at all.
            if (resultant.DivideExact(poly.LeadingCoefficientIn(mainVariable)) is not { } quotient)
                return null;
            return degree * (degree - 1) / 2 % 2 == 0 ? quotient : quotient.ScaleBy(MinusOne);
        }

        /// <summary>
        /// The greatest common divisor of the coefficients in
        /// <paramref name="mainVariable"/>, or one where that could not be settled — the
        /// caller only wants it to make the elimination smaller.
        /// </summary>
        private static MultivariatePolynomial ContentOrOne(
            MultivariatePolynomial poly, int mainVariable, IReadOnlyList<int> otherVariables)
            => PolynomialGcd.ContentIn(poly, mainVariable, otherVariables, 0)
                ?? MultivariatePolynomial.One(poly.VariableCount);

        /// <summary>
        /// <paramref name="poly"/> divided by <paramref name="factor"/>, accepted only once
        /// multiplying it back has given the original — the same discipline
        /// <see cref="PolynomialGcd.TryCancel"/> applies, and for the same reason: a factor
        /// wrongly divided out is a wrong answer rather than a missing one.
        /// </summary>
        private static bool TryDivideOut(
            MultivariatePolynomial poly, MultivariatePolynomial factor, out MultivariatePolynomial quotient)
        {
            quotient = poly;
            if (poly.DivideExact(factor) is not { } divided
                || divided.Multiply(factor) is not { } remultiplied
                || !remultiplied.SameAs(poly))
                return false;
            quotient = divided;
            return true;
        }

        /// <summary>
        /// The determinant of the Sylvester matrix of the two polynomials, read in
        /// <paramref name="mainVariable"/>: as many rows of <paramref name="left"/>'s
        /// coefficients as <paramref name="right"/> has degree, then as many rows of
        /// <paramref name="right"/>'s as <paramref name="left"/> has degree, each row
        /// shifted one place right of the one above it. That order is what makes this
        /// <c>Res(left, right)</c> rather than <c>Res(right, left)</c>, and the two differ
        /// by <c>(-1)^(deg left * deg right)</c>.
        /// </summary>
        private static MultivariatePolynomial? SylvesterDeterminant(
            MultivariatePolynomial left, MultivariatePolynomial right, int mainVariable)
        {
            var variableCount = left.VariableCount;
            var zero = MultivariatePolynomial.Zero(variableCount);
            var leftDegree = left.DegreeIn(mainVariable);
            var rightDegree = right.DegreeIn(mainVariable);
            var size = leftDegree + rightDegree;

            var matrix = new MultivariatePolynomial[size][];
            for (var row = 0; row < size; row++)
            {
                matrix[row] = new MultivariatePolynomial[size];
                for (var column = 0; column < size; column++)
                    matrix[row][column] = zero;
            }
            var leftCoefficients = left.CoefficientsIn(mainVariable);
            for (var row = 0; row < rightDegree; row++)
                for (var power = 0; power <= leftDegree; power++)
                    if (leftCoefficients.TryGetValue(power, out var coefficient))
                        matrix[row][row + leftDegree - power] = coefficient;
            var rightCoefficients = right.CoefficientsIn(mainVariable);
            for (var row = 0; row < leftDegree; row++)
                for (var power = 0; power <= rightDegree; power++)
                    if (rightCoefficients.TryGetValue(power, out var coefficient))
                        matrix[rightDegree + row][row + rightDegree - power] = coefficient;

            // The elimination itself is shared with Entity.Matrix.Determinant, which
            // wants exactly this over an ordinary square matrix rather than a Sylvester
            // one. https://github.com/asc-community/AngouriMath/issues/999
            return FractionFreeDeterminant.Of(matrix, variableCount, MaxEliminationWork);
        }
    }
}
