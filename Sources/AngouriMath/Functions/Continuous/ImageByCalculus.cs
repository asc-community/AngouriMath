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
