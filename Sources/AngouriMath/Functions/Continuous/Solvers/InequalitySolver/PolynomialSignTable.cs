//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    /// <summary>
    /// <c>p(x) &gt; 0</c> for a univariate polynomial over <c>Q</c> of any degree, answered by
    /// the sign of <c>p</c> between consecutive real roots.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A polynomial has one sign on each open interval between consecutive real roots, so the
    /// answer is the union of those intervals where the sign is positive. Everything hard is
    /// in the word *consecutive*: the intervals are only the answer if the list of real roots
    /// is <b>complete</b>, and a missed root merges two intervals of opposite sign into one
    /// and reports the wrong half of it as the solution. So this refuses wherever completeness
    /// cannot be established, rather than answering from whatever roots were found.
    /// </para>
    /// <para>
    /// Completeness is established algebraically and in two steps. First
    /// <see cref="PolynomialFactorization.FactorPrimitive"/> writes the polynomial as a
    /// product of powers of irreducibles over <c>Q</c>, and it verifies that the factors
    /// multiply back to what it was given — so every real root of the whole is a real root of
    /// exactly one factor, an irreducible being square-free and two distinct irreducibles
    /// being coprime. Second, the number of real roots of each factor is read off its
    /// <see cref="PolynomialResultant.Discriminant"/>: a factor of degree two has two real
    /// roots where its discriminant is positive and none where it is negative, and one of
    /// degree three has three and one respectively. A factor of degree four needs two more
    /// quantities alongside the discriminant, and there is no such criterion at five — nor a
    /// formula for the roots — which is where this stops. That count is what makes a root
    /// list a complete root list rather than a list of the roots that happened to come back.
    /// </para>
    /// <para>
    /// The roots themselves come from the solver reached through <c>SolveEquation</c>,
    /// which is exact, and their <i>order</i> is decided numerically, which is not. So the
    /// ordering is checked rather than trusted: the sign of the polynomial at an exact
    /// rational point in each interval is computed in exact integer arithmetic, and the
    /// resulting sequence has to change sign at every root of odd multiplicity and keep it at
    /// every root of even multiplicity. A misordered or merged root shows up as a sequence
    /// that does not, and is refused. The two outermost sample points are placed beyond
    /// Cauchy's bound, so no root can lie outside the sampled range.
    /// </para>
    /// <para>
    /// Part of the polynomial layer of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/746">#746</a>, item 43 —
    /// and the consumer that the resultant was waiting for.
    /// </para>
    /// </remarks>
    internal static class PolynomialSignTable
    {
        /// <summary>
        /// The highest degree an irreducible factor may have for its number of real roots to
        /// be settled.
        /// </summary>
        /// <remarks>
        /// Up to three the sign of the discriminant is the whole answer. At four it is half
        /// of it — a negative discriminant means exactly two real roots, and a positive one
        /// means four or none — and the two auxiliary quantities of the standard criterion
        /// decide between those. At five there is no such criterion, and there is no formula
        /// for the roots either.
        /// </remarks>
        private const int MaxFactorDegree = 4;

        /// <summary>
        /// Two consecutive roots closer than this, relative to their size, are not separated
        /// with confidence by a double-precision midpoint, and the sign table declines rather
        /// than places a sample point on the wrong side of one.
        /// </summary>
        /// <remarks>
        /// The exact sign check below would catch such a mistake in almost every case; this
        /// is the cheaper guard in front of it, so that the failure is a refusal for a stated
        /// reason rather than a failed consistency check.
        /// </remarks>
        private const double LeastRelativeGap = 1e-9;

        /// <summary>
        /// The solution set of <c>expr &gt; 0</c>, or <see langword="false"/> where
        /// <paramref name="expr"/> is not a univariate polynomial over <c>Q</c> in
        /// <paramref name="x"/> of degree at least three, or where the real roots could not
        /// be established completely.
        /// </summary>
        internal static bool TrySolve(Entity expr, Variable x, [NotNullWhen(true)] out Set? solution)
        {
            solution = null;
            // Degree three and up only: the linear and quadratic branches of the caller carry
            // symbolic coefficients and a case split on their signs, which this does not, so
            // taking their inputs from them would be a regression rather than a widening.
            if (!PolynomialFactoring.TryGetRationalCoefficients(
                    expr, x, leastTerms: 1, leastDegree: 3, IntegerPolynomial.MaxDegree,
                    out var rational))
                return false;

            // Cleared of denominators. The multiplier is a positive integer, so the sign of
            // the polynomial is the sign of the expression at every point.
            var denominator = EInteger.One;
            foreach (var coefficient in rational)
                denominator = Lcm(denominator, coefficient.Denominator);
            var whole = new EInteger[rational.Length];
            for (var i = 0; i < whole.Length; i++)
                whole[i] = rational[i].Numerator.Multiply(denominator.Divide(rational[i].Denominator));

            var poly = IntegerPolynomial.Create(whole);
            if (poly.Degree < 3)
                return false;

            var primitive = poly.PrimitivePart();
            if (PolynomialFactorization.FactorPrimitive(primitive) is not { } parts)
                return false;

            var roots = new List<Root>();
            foreach (var part in parts)
            {
                if (!TryRealRootsOf(part.Factor, x, roots, part.Multiplicity))
                    return false;
            }

            roots.Sort(static (left, right) => left.Approximation.CompareTo(right.Approximation));
            for (var i = 1; i < roots.Count; i++)
            {
                var gap = roots[i].Approximation - roots[i - 1].Approximation;
                var scale = Math.Max(1.0, Math.Max(Math.Abs(roots[i].Approximation),
                                                   Math.Abs(roots[i - 1].Approximation)));
                if (!(gap > LeastRelativeGap * scale))
                    return false;
            }

            if (SampleSigns(poly, roots) is not { } signs)
                return false;

            // The sign either turns around at a root or does not, and which of those is the
            // parity of its multiplicity. Anything else means the roots are not the ones the
            // samples were placed around.
            for (var i = 0; i < roots.Count; i++)
                if ((signs[i] != signs[i + 1]) != (roots[i].Multiplicity % 2 == 1))
                    return false;

            var pieces = new List<Set>();
            for (var i = 0; i <= roots.Count; i++)
            {
                if (signs[i] <= 0)
                    continue;
                var left = i == 0 ? (Entity)Real.NegativeInfinity : roots[i - 1].Value;
                var right = i == roots.Count ? (Entity)Real.PositiveInfinity : roots[i].Value;
                pieces.Add(new Interval(left, false, right, false));
            }

            if (pieces.Count == 0)
            {
                solution = Empty;
                return true;
            }
            Set united = pieces[0];
            for (var i = 1; i < pieces.Count; i++)
                united = united.Unite(pieces[i]);
            solution = united;
            return true;
        }

        /// <summary>A real root of an irreducible factor, with the multiplicity that factor carries.</summary>
        private readonly struct Root
        {
            internal Root(Entity value, double approximation, int multiplicity)
            {
                Value = value;
                Approximation = approximation;
                Multiplicity = multiplicity;
            }

            internal Entity Value { get; }

            internal double Approximation { get; }

            internal int Multiplicity { get; }
        }

        /// <summary>
        /// Appends the real roots of an irreducible <paramref name="factor"/> to
        /// <paramref name="into"/>, or answers <see langword="false"/> where how many of them
        /// there are could not be settled.
        /// </summary>
        private static bool TryRealRootsOf(
            IntegerPolynomial factor, Variable x, List<Root> into, int multiplicity)
        {
            var degree = factor.Degree;
            if (degree < 1 || degree > MaxFactorDegree)
                return false;

            if (degree == 1)
            {
                var root = ERational.Create(factor[0].Negate(), factor[1]).ToLowestTerms();
                into.Add(new Root(Rational.Create(root), root.ToDouble(), multiplicity));
                return true;
            }

            if (RealRootCount(factor, x, degree) is not { } expected)
                return false;
            if (expected == 0)
                return true;

            if (factor.ToEntity(x).SolveEquation(x) is not FiniteSet solved || solved.Count != degree)
                return false;

            var candidates = new List<(Entity Value, double Real, double Imaginary)>(degree);
            foreach (var candidate in solved)
            {
                if (candidate.EvalNumerical() is not Complex numeric)
                    return false;
                candidates.Add((candidate,
                                numeric.RealPart.EDecimal.ToDouble(),
                                Math.Abs(numeric.ImaginaryPart.EDecimal.ToDouble())));
            }
            foreach (var candidate in candidates)
                if (double.IsNaN(candidate.Real) || double.IsInfinity(candidate.Real)
                    || double.IsNaN(candidate.Imaginary))
                    return false;

            if (expected == degree)
            {
                // Every root is real, so there is nothing to choose between them and the
                // imaginary parts are the rounding of a cancellation rather than evidence.
                foreach (var candidate in candidates)
                    into.Add(new Root(candidate.Value, candidate.Real, multiplicity));
                return true;
            }

            // Some of the roots are real and the rest are conjugate pairs off the axis. The
            // real ones are those whose imaginary part is rounding rather than value, and
            // they are taken only where the two groups are far enough apart that saying
            // which is which is not a close call.
            candidates.Sort(static (left, right) => left.Imaginary.CompareTo(right.Imaginary));
            var scale = 1.0;
            for (var i = 0; i < expected; i++)
                scale = Math.Max(scale, Math.Abs(candidates[i].Real));
            if (!(candidates[expected - 1].Imaginary < 1e-9 * scale)
                || !(candidates[expected].Imaginary > 1e-3 * scale))
                return false;
            for (var i = 0; i < expected; i++)
                into.Add(new Root(candidates[i].Value, candidates[i].Real, multiplicity));
            return true;
        }

        /// <summary>
        /// How many distinct real roots an irreducible <paramref name="factor"/> of degree
        /// two, three or four has, or <see langword="null"/> where that could not be settled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The sign of the discriminant decides it outright up to degree three. At four it
        /// separates two real roots (negative) from four or none (positive), and the two
        /// auxiliary quantities <c>P = 8ac - 3b^2</c> and
        /// <c>D = 64a^3 e - 16a^2 c^2 + 16a b^2 c - 16a^2 b d - 3b^4</c> decide between
        /// those: four real where both are negative, none where either is positive. Where
        /// neither of those holds — one of them zero and the other not positive — the
        /// criterion says nothing, and neither does this. Rees, <i>A note on the quartic</i>,
        /// Amer. Math. Monthly 29 (1922); Lazard, <i>Quantifier elimination: optimal solution
        /// for two classical examples</i>, J. Symbolic Comput. 5 (1988).
        /// </para>
        /// <para>
        /// A zero discriminant means a repeated factor, which an irreducible polynomial does
        /// not have — so it is a sign that the input was not what it was taken to be, and the
        /// sign table declines rather than proceeds.
        /// </para>
        /// </remarks>
        private static int? RealRootCount(IntegerPolynomial factor, Variable x, int degree)
        {
            var index = new Dictionary<Variable, int> { [x] = 0 };
            if (MultivariatePolynomial.TryParse(factor.ToEntity(x), index) is not { } parsed)
                return null;
            if (PolynomialResultant.Discriminant(parsed, 0, Array.Empty<int>()) is not { } discriminant)
                return null;
            if (!discriminant.IsConstant)
                return null;
            var sign = discriminant.CoefficientOf(0).Sign;
            if (sign == 0)
                return null;
            switch (degree)
            {
                case 2: return sign > 0 ? 2 : 0;
                case 3: return sign > 0 ? 3 : 1;
                case 4:
                    if (sign < 0)
                        return 2;
                    var a = factor[4];
                    var b = factor[3];
                    var c = factor[2];
                    var d = factor[1];
                    var e = factor[0];
                    var p = EInteger.FromInt32(8).Multiply(a).Multiply(c)
                        .Subtract(EInteger.FromInt32(3).Multiply(b).Multiply(b));
                    var big = EInteger.FromInt32(64).Multiply(a).Multiply(a).Multiply(a).Multiply(e)
                        .Subtract(EInteger.FromInt32(16).Multiply(a).Multiply(a).Multiply(c).Multiply(c))
                        .Add(EInteger.FromInt32(16).Multiply(a).Multiply(b).Multiply(b).Multiply(c))
                        .Subtract(EInteger.FromInt32(16).Multiply(a).Multiply(a).Multiply(b).Multiply(d))
                        .Subtract(EInteger.FromInt32(3).Multiply(b).Multiply(b).Multiply(b).Multiply(b));
                    if (p.Sign < 0 && big.Sign < 0)
                        return 4;
                    if (p.Sign > 0 || big.Sign > 0)
                        return 0;
                    return null;
                default: return null;
            }
        }

        /// <summary>
        /// The exact sign of <paramref name="poly"/> at one rational point in each of the
        /// intervals the <paramref name="roots"/> cut the line into, or <see langword="null"/>
        /// where a sample landed on a root.
        /// </summary>
        /// <remarks>
        /// The outermost two points are placed beyond Cauchy's bound
        /// <c>1 + max|a_i| / |a_n|</c>, outside which the leading term dominates the sum of
        /// all the others and the polynomial cannot vanish. So the two unbounded intervals
        /// are sampled where they are genuinely unbounded, rather than wherever the outermost
        /// root happened to be.
        /// </remarks>
        private static int[]? SampleSigns(IntegerPolynomial poly, IReadOnlyList<Root> roots)
        {
            var bound = CauchyBound(poly);
            var signs = new int[roots.Count + 1];
            for (var i = 0; i <= roots.Count; i++)
            {
                ERational point;
                if (roots.Count == 0)
                    point = ERational.Zero;
                else if (i == 0)
                    point = Below(roots[0].Approximation, bound);
                else if (i == roots.Count)
                    point = Above(roots[roots.Count - 1].Approximation, bound);
                else
                    point = Midpoint(roots[i - 1].Approximation, roots[i].Approximation);
                var value = Evaluate(poly, point);
                if (value.IsZero)
                    return null;
                signs[i] = value.Sign;
            }
            return signs;
        }

        private static ERational Midpoint(double left, double right)
            => ERational.FromDouble(left).Add(ERational.FromDouble(right))
                        .Divide(ERational.FromInt32(2)).ToLowestTerms();

        private static ERational Below(double leastRoot, ERational bound)
        {
            var candidate = ERational.FromDouble(leastRoot).Subtract(ERational.One);
            var outside = bound.Negate().Subtract(ERational.One);
            return candidate.CompareTo(outside) < 0 ? candidate : outside;
        }

        private static ERational Above(double greatestRoot, ERational bound)
        {
            var candidate = ERational.FromDouble(greatestRoot).Add(ERational.One);
            var outside = bound.Add(ERational.One);
            return candidate.CompareTo(outside) > 0 ? candidate : outside;
        }

        /// <summary>Cauchy's bound: every real root is strictly inside it.</summary>
        private static ERational CauchyBound(IntegerPolynomial poly)
        {
            var leading = poly.Leading.Abs();
            var greatest = EInteger.Zero;
            for (var power = 0; power < poly.Degree; power++)
            {
                var magnitude = poly[power].Abs();
                if (magnitude.CompareTo(greatest) > 0)
                    greatest = magnitude;
            }
            return ERational.One.Add(ERational.Create(greatest, leading)).ToLowestTerms();
        }

        /// <summary>
        /// <paramref name="poly"/> at <paramref name="point"/>, by Horner and in exact
        /// rational arithmetic, so that the sign it comes back with is the sign it has.
        /// </summary>
        private static ERational Evaluate(IntegerPolynomial poly, ERational point)
        {
            var result = ERational.Zero;
            for (var power = poly.Degree; power >= 0; power--)
                result = result.Multiply(point).Add(ERational.Create(poly[power], EInteger.One));
            return result.ToLowestTerms();
        }

        private static EInteger Lcm(EInteger left, EInteger right)
            => left.Divide(left.Gcd(right)).Multiply(right);
    }
}
