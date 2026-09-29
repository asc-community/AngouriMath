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
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions
{
    /// <summary>
    /// The image of a set of reals under a quotient of polynomials, <c>{ f(x) : x in S }</c>, read
    /// off the function's critical points and its limits: Sullivan and Mackey's §7.3.5 Try 1,
    /// <c>x/(1 + x)</c> over <c>RR \ {-1}</c>, is <c>RR \ {1}</c>, and Prob 7.8.8's
    /// <c>(2x - 1)/(2x (1 - x))</c> over <c>(0, 1)</c> is <c>RR</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A continuous function maps an interval onto an interval, and a differentiable one is
    /// monotone between the zeros of its derivative, so the image of an interval without a pole is
    /// the interval between the least and the greatest of the values at the critical points inside
    /// it, the values at its closed ends and the one-sided limits at its open ones. An end of the
    /// image is closed where the value is taken, at a critical point or a closed end, and open
    /// where it is only approached. The set is cut into such intervals first: at the points a
    /// difference removes, and at the poles, where the function is not defined.
    /// </para>
    /// <para>
    /// The derivative's zeros have to be all of them, or a turning point is missed and the image
    /// comes out too small. So the numerator of the derivative, and the denominator whose zeros
    /// are the poles, have to be polynomials of degree four or less, which the solver answers in
    /// radicals completely; the coefficients have to be rational, so that a coefficient which
    /// cancels is zero rather than a residue; and every root has to evaluate, a real one to a
    /// candidate and a non-real one to nothing. A real root can come out of the radicals with an
    /// imaginary residue near <c>1e-99</c>, which is read as zero below the downcasting tolerance
    /// whether downcasting is on or not.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class ImageByCalculus
    {
        /// <summary>The highest degree whose roots the solver lists completely, in radicals.</summary>
        private const int MaxDegree = 4;

        /// <summary>The largest whole exponent read, so that nothing large is expanded to find a degree.</summary>
        private const int MaxExponent = 8;

        internal static Set? Of(Entity f, Variable x, Entity over)
        {
            if (!f.ContainsNode(x) || !IsRational(f, x) || Pieces(over) is not { } pieces)
                return null;
            var (numerator, denominator) = SingleQuotient.Of(f);
            // The quotient rule's numerator: its zeros are the critical points.
            var slope = numerator.Differentiate(x) * denominator - numerator * denominator.Differentiate(x);
            if (Polynomial(denominator, x) is not { } bottom || Polynomial(slope, x) is not { } top
                || RealRoots(bottom, x) is not { } poles || RealRoots(top, x) is not { } critical)
                return null;
            // Each piece's image once, from the left, so that the union reads in order and two
            // pieces with one image are written once.
            var parts = new List<(Set Part, Real From)>();
            foreach (var piece in pieces.SelectMany(piece => CutAt(piece, poles)))
            {
                if (Of(f, x, piece, critical) is not { } part)
                    return null;
                if (Lowest(part) is { } from && parts.All(other => other.Part != part))
                    parts.Add((part, from));
            }
            Set image = Empty;
            foreach (var (part, _) in parts.OrderBy(part => part.From))
                image = image.Unite(part).InnerSimplified is Set united ? united : (Set)image.Unite(part);
            return image;
        }

        /// <summary>Where an image of one piece starts, or <see langword="null"/> for an empty one.</summary>
        private static Real? Lowest(Set part)
            => part switch
            {
                Interval interval => AsReal(interval.Left),
                FiniteSet { Count: > 0 } listed => AsReal(listed.First()),
                _ => null,
            };

        /// <summary>
        /// Whether a quotient of polynomials is one-to-one on an interval, or <see langword="null"/>
        /// where that is not settled here. A continuous function on an interval is one-to-one
        /// exactly where it is strictly monotone: <c>x^3</c> on <c>RR</c> is, and <c>x^2</c> on
        /// <c>RR</c> is not. Sullivan and Mackey's Def 7.4.1.
        /// </summary>
        internal static bool? OneToOne(Entity f, Variable x, Entity over)
            => Monotone(f, x, over) switch
            {
                null => null,
                0 => false,
                _ => true,
            };

        /// <summary>
        /// The direction a quotient of polynomials runs in on an interval: <c>1</c> where it is
        /// strictly increasing, <c>-1</c> where strictly decreasing, <c>0</c> where it is neither,
        /// and <see langword="null"/> where that is not settled here. A differentiable function is
        /// strictly monotone exactly where its derivative keeps one sign away from the points
        /// where it is zero: the derivative of <c>x^3</c> is zero at 0 and positive on either
        /// side, and that of <c>x^2</c> changes sign at 0. A pole inside the interval leaves it
        /// unsettled.
        /// </summary>
        internal static int? Monotone(Entity f, Variable x, Entity over)
        {
            if (!f.ContainsNode(x) || !IsRational(f, x) || Pieces(over) is not { Count: 1 } pieces)
                return null;
            var piece = pieces[0];
            var (numerator, denominator) = SingleQuotient.Of(f);
            var slope = numerator.Differentiate(x) * denominator - numerator * denominator.Differentiate(x);
            if (Polynomial(denominator, x) is not { } bottom || Polynomial(slope, x) is not { } top
                || RealRoots(bottom, x) is not { } poles || RealRoots(top, x) is not { } critical
                || AsReal(piece.Left) is not { } left || AsReal(piece.Right) is not { } right || left.CompareTo(right) >= 0
                || poles.Any(pole => pole.Value.CompareTo(left) > 0 && pole.Value.CompareTo(right) < 0))
                return null;
            // The derivative's sign between each two consecutive of the ends and the zeros
            // inside, which is its numerator's, the denominator being a square.
            var ends = new List<Real> { left };
            ends.AddRange(critical.Select(point => point.Value).Where(at => at.CompareTo(left) > 0 && at.CompareTo(right) < 0).OrderBy(at => at));
            ends.Add(right);
            int? sign = null;
            for (var i = 0; i + 1 < ends.Count; i++)
            {
                if (AsReal(top.Substitute(x, Between(ends[i], ends[i + 1]))) is not { IsFinite: true } value)
                    return null;
                var here = value.EDecimal.Sign;
                // Zero on a whole stretch is a constant there, and a change of sign a turn.
                if (here == 0 || sign is { } before && before != here)
                    return 0;
                sign = here;
            }
            return sign;
        }

        /// <summary>
        /// A union or an intersection of a family of intervals whose ends are quotients of
        /// polynomials in the index, monotone over its range, as the interval between the extremes
        /// of the ends; <see langword="null"/> where that is not settled here. Sullivan and
        /// Mackey's §3.9.5: the intersection of <c>(-1/n, 1/n)</c> over the positive whole numbers
        /// is <c>{0}</c>, the union of <c>(x, x + 1)</c> over <c>(0, 1)</c> is <c>(0, 2)</c>, and
        /// the union of <c>[0, (n - 1)/n)</c> is <c>[0, 1)</c>, the intersection of <c>(-1/n, 1)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// An intersection of intervals is an interval, from the supremum of the left ends to the
        /// infimum of the right ones. An end of it is closed where the family's ends are, and where
        /// the extreme is only approached: every <c>(-1/n, 1/n)</c> holds 0, where both ends tend.
        /// </para>
        /// <para>
        /// A union is an interval from the infimum of the left ends to the supremum of the right
        /// ones where the family is a chain, each member inside the next -- ends moving apart, or
        /// one still -- and where the index runs over an interval and every member is not empty,
        /// so that the members overlap as it moves. Over the whole numbers a sliding family,
        /// <c>(k, k + 1)</c>, leaves gaps, and is not read. An end of the union is closed where the
        /// family's ends are and the extreme is reached.
        /// </para>
        /// <para>
        /// A monotone end has its extremes at the ends of the index's range: at a least or a
        /// greatest member, where it is reached, and in the limit, where it is not.
        /// </para>
        /// </remarks>
        internal static Set? Family(bool union, Interval member, Variable k, Entity over)
        {
            if (Range(over) is not var (range, whole)
                || Extremes(member.Left, k, range) is not var (leftLow, leftHigh, leftDirection)
                || Extremes(member.Right, k, range) is not var (rightLow, rightHigh, rightDirection))
                return null;
            (Entity Value, bool Closed) lower, upper;
            if (union)
            {
                var chain = leftDirection <= 0 && rightDirection >= 0 || leftDirection >= 0 && rightDirection <= 0;
                if (!chain && (whole || !NeverEmpty(member, k, range)))
                    return null;
                lower = (leftLow.Value, member.LeftClosed && leftLow.Reached);
                upper = (rightHigh.Value, member.RightClosed && rightHigh.Reached);
            }
            else
            {
                lower = (leftHigh.Value, member.LeftClosed || !leftHigh.Reached);
                upper = (rightLow.Value, member.RightClosed || !rightLow.Reached);
            }
            if (AsReal(lower.Value) is not { IsNaN: false } from || AsReal(upper.Value) is not { IsNaN: false } to)
                return null;
            // Simplified, so that [0; 0] is the point it is and (0; 0) nothing.
            return new Interval(lower.Value, lower.Closed && from.IsFinite, upper.Value, upper.Closed && to.IsFinite).InnerSimplified as Set;
        }

        /// <summary>The index's range as an interval, and whether the index is a whole number.</summary>
        private static (Interval Range, bool Whole)? Range(Entity over)
            => over switch
            {
                SpecialSet.PositiveIntegers => (new Interval(Integer.One, true, Real.PositiveInfinity, false), true),
                SpecialSet.NonNegativeIntegers => (new Interval(Integer.Zero, true, Real.PositiveInfinity, false), true),
                SpecialSet.Integers => (new Interval(Real.NegativeInfinity, false, Real.PositiveInfinity, false), true),
                SpecialSet.Reals => (new Interval(Real.NegativeInfinity, false, Real.PositiveInfinity, false), false),
                Interval interval when interval.IsNumeric => (interval, false),
                _ => null,
            };

        /// <summary>
        /// The least and the greatest of an end over the range, each with whether it is reached,
        /// and the end's direction, <c>0</c> for one that does not move; <see langword="null"/>
        /// where the end is not monotone there.
        /// </summary>
        private static ((Entity Value, bool Reached) Low, (Entity Value, bool Reached) High, int Direction)? Extremes(Entity end, Variable k, Interval range)
        {
            if (!end.ContainsNode(k))
                return ((end, true), (end, true), 0);
            if (Monotone(end, k, range) is not { } direction || direction == 0)
                return null;
            if (At(end, k, range.Left, range.LeftClosed, Core.ApproachFrom.Right) is not { } atStart
                || At(end, k, range.Right, range.RightClosed, Core.ApproachFrom.Left) is not { } atEnd)
                return null;
            return direction > 0 ? (atStart, atEnd, 1) : (atEnd, atStart, -1);
        }

        /// <summary>The end's value at an end of the range, reached, or its limit there, not reached.</summary>
        private static (Entity Value, bool Reached)? At(Entity end, Variable k, Entity at, bool included, Core.ApproachFrom side)
        {
            if (AsReal(at) is not { } point)
                return null;
            if (included && point.IsFinite)
                return (end.Substitute(k, at).Simplify(), true);
            var limit = point.IsFinite ? end.Limit(k, at, side) : end.Limit(k, at);
            return AsReal(limit) is { IsNaN: false } ? (limit, false) : null;
        }

        /// <summary>Whether every member of a family is not empty: its right end above its left one over the whole range.</summary>
        private static bool NeverEmpty(Interval member, Variable k, Interval range)
        {
            var width = (member.Right - member.Left).Simplify();
            if (!width.ContainsNode(k))
                return AsReal(width) is { } constant && constant.EDecimal.Sign > 0;
            return Of(width, k, range) switch
            {
                Interval { Left: var least, LeftClosed: var reached } => AsReal(least) is { } bound && (bound.EDecimal.Sign > 0 || bound.EDecimal.Sign == 0 && !reached),
                FiniteSet { Count: 1 } single => AsReal(single.First()) is { } only && only.EDecimal.Sign > 0,
                _ => false,
            };
        }

        /// <summary>A point strictly between two ends, either of which may be infinite.</summary>
        private static Entity Between(Real low, Real high)
            => (low.IsFinite, high.IsFinite) switch
            {
                (true, true) => ((Entity)low + high) / 2,
                (true, false) => (Entity)low + 1,
                (false, true) => (Entity)high - 1,
                _ => Integer.Zero,
            };

        /// <summary>The image of one interval with no pole inside.</summary>
        private static Set? Of(Entity f, Variable x, Interval piece, List<(Entity Root, Real Value)> critical)
        {
            if (AsReal(piece.Left) is not { } left || AsReal(piece.Right) is not { } right)
                return null;
            if (left.CompareTo(right) > 0 || left == right && !(piece.LeftClosed && piece.RightClosed))
                return Empty;
            // Each candidate is a value, what it evaluates to, and whether it is taken.
            var candidates = new List<(Entity Value, Real Number, bool Taken)>();
            foreach (var (root, at) in critical)
                if (at.CompareTo(left) > 0 && at.CompareTo(right) < 0)
                {
                    var value = f.Substitute(x, root);
                    if (AsReal(value) is not { IsFinite: true } number)
                        return null;
                    candidates.Add((value, number, true));
                }
            if (End(f, x, piece.Left, left, piece.LeftClosed, Core.ApproachFrom.Right) is not { } atLeft
                || End(f, x, piece.Right, right, piece.RightClosed, Core.ApproachFrom.Left) is not { } atRight)
                return null;
            candidates.Add(atLeft);
            candidates.Add(atRight);
            var least = candidates.Min(candidate => candidate.Number)!;
            var greatest = candidates.Max(candidate => candidate.Number)!;
            // Only the two that bound the image are simplified.
            var lower = candidates.First(candidate => Same(candidate.Number, least)).Value.Simplify();
            var upper = candidates.First(candidate => Same(candidate.Number, greatest)).Value.Simplify();
            var lowerTaken = least.IsFinite && candidates.Any(candidate => Same(candidate.Number, least) && candidate.Taken);
            var upperTaken = greatest.IsFinite && candidates.Any(candidate => Same(candidate.Number, greatest) && candidate.Taken);
            // A function constant on the interval takes its one value at every point of it.
            if (Same(least, greatest))
                return new FiniteSet(lower);
            return new Interval(lower, lowerTaken, upper, upperTaken);
        }

        /// <summary>The value at a closed end, taken, or the one-sided limit at an open one, approached.</summary>
        private static (Entity Value, Real Number, bool Taken)? End(Entity f, Variable x, Entity end, Real at, bool closed, Core.ApproachFrom side)
        {
            if (closed && at.IsFinite)
            {
                var value = f.Substitute(x, end);
                return AsReal(value) is { IsFinite: true } number ? (value, number, true) : null;
            }
            // A limit that is not found stays a limit node, which does not evaluate to a number.
            var limit = at.IsFinite ? f.Limit(x, end, side) : f.Limit(x, end);
            return AsReal(limit) is { IsNaN: false } approached ? (limit, approached, false) : null;
        }

        /// <summary>
        /// The expression as a polynomial of degree at most <see cref="MaxDegree"/>, its monomials
        /// gathered and those that cancel dropped, or <see langword="null"/>.
        /// </summary>
        private static Entity? Polynomial(Entity expr, Variable x)
        {
            if (!TreeAnalyzer.TryGetPolynomial(expr, x, out var monomials))
                return null;
            Entity sum = Integer.Zero;
            foreach (var monomial in monomials)
            {
                var power = monomial.Key;
                if (monomial.Value.Evaled is not Rational exact)
                    return null;
                if (exact.IsZero)
                    continue;
                if (power.Sign < 0 || power.CompareTo(EInteger.FromInt32(MaxDegree)) > 0)
                    return null;
                // The constant term as itself: x^0 is 1 only where x is not 0.
                sum += power.IsZero ? exact : exact * MathS.Pow(x, Integer.Create(power));
            }
            return sum.InnerSimplified;
        }

        /// <summary>
        /// The real roots of a polynomial with each one's value, or <see langword="null"/> where the
        /// solver does not list them all as numbers.
        /// </summary>
        private static List<(Entity Root, Real Value)>? RealRoots(Entity polynomial, Variable x)
        {
            if (!polynomial.ContainsNode(x))
                return new List<(Entity, Real)>();
            // The equation solver answers a polynomial it cannot list with a set that is not
            // listed, rather than throwing.
            if (polynomial.SolveEquation(x) is not FiniteSet listed)
                return null;
            var real = new List<(Entity, Real)>();
            foreach (var root in listed)
                if (AsReal(root) is { } value)
                {
                    if (!value.IsFinite)
                        return null;
                    real.Add((root, value));
                }
                else if (root.Evaled is not Complex { RealPart.IsFinite: true, ImaginaryPart.IsFinite: true })
                    return null;
            return real;
        }

        /// <summary>
        /// What the expression evaluates to as a real number, or <see langword="null"/> where it is
        /// not one. An imaginary part below the downcasting tolerance is the residue of exact
        /// cancellation and is read as zero, as downcasting would read it.
        /// </summary>
        private static Real? AsReal(Entity expr)
            => expr.Evaled switch
            {
                Real real => real,
                Complex complex when complex.ImaginaryPart.EDecimal.IsFinite
                    && complex.ImaginaryPart.EDecimal.Abs().LessThan(MathS.Settings.DowncastingTolerance) => complex.RealPart,
                _ => null,
            };

        /// <summary>Two values that differ by less than the downcasting tolerance are one.</summary>
        private static bool Same(Real a, Real b)
            => a == b || a.IsFinite && b.IsFinite && (a - b).EDecimal.Abs().LessThan(MathS.Settings.DowncastingTolerance);

        /// <summary>The set as intervals, or <see langword="null"/> where it is not a union of intervals and points removed.</summary>
        private static List<Interval>? Pieces(Entity over)
            => over switch
            {
                SpecialSet.Reals => new List<Interval> { new Interval(Real.NegativeInfinity, false, Real.PositiveInfinity, false) },
                Interval interval => new List<Interval> { interval },
                Unionf(var left, var right) => Pieces(left) is { } l && Pieces(right) is { } r ? l.Concat(r).ToList() : null,
                SetMinusf(var from, FiniteSet removed) when removed.All(point => AsReal(point) is { IsFinite: true })
                    => Pieces(from) is { } pieces ? pieces.SelectMany(piece => CutAt(piece, removed.Select(point => (point, AsReal(point)!)).ToList())).ToList() : null,
                _ => null,
            };

        /// <summary>The interval cut open at each of the points inside it.</summary>
        private static IEnumerable<Interval> CutAt(Interval piece, List<(Entity Root, Real Value)> points)
        {
            if (AsReal(piece.Left) is not { } left || AsReal(piece.Right) is not { } right)
                return new[] { piece };
            var inside = points.Where(point => point.Value.CompareTo(left) > 0 && point.Value.CompareTo(right) < 0)
                .OrderBy(point => point.Value).ToList();
            if (inside.Count == 0)
                return new[] { piece };
            var cut = new List<Interval>();
            Entity from = piece.Left;
            var fromClosed = piece.LeftClosed;
            foreach (var (root, _) in inside)
            {
                cut.Add(new Interval(from, fromClosed, root, false));
                (from, fromClosed) = (root, false);
            }
            cut.Add(new Interval(from, fromClosed, piece.Right, piece.RightClosed));
            return cut;
        }

        /// <summary>
        /// Whether the expression is built from the variable, rational constants and the four
        /// operations with whole powers no larger than <see cref="MaxExponent"/>.
        /// </summary>
        private static bool IsRational(Entity f, Variable x)
            => f switch
            {
                _ when !f.ContainsNode(x) => f.Evaled is Rational,
                Variable => true,
                Sumf(var a, var b) => IsRational(a, x) && IsRational(b, x),
                Minusf(var a, var b) => IsRational(a, x) && IsRational(b, x),
                Mulf(var a, var b) => IsRational(a, x) && IsRational(b, x),
                Divf(var a, var b) => IsRational(a, x) && IsRational(b, x),
                Powf(var a, Integer power) when power.EInteger.Abs().CompareTo(EInteger.FromInt32(MaxExponent)) <= 0 => IsRational(a, x),
                _ => false,
            };
    }
}
