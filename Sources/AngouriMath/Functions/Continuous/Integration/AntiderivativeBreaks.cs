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

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// The points strictly inside a numeric range where an antiderivative can stop being
    /// continuous, so that a definite integral is taken through it piece by piece.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>F(b) - F(a)</c> is the integral only where <c>F</c> is continuous on <c>[a, b]</c>. A pole
    /// of the integrand between the bounds is a point where <c>F</c> is not even defined: the
    /// integral of <c>1/x^2</c> over <c>[-1, 1]</c> came back as <c>-1/x</c> at 1 less <c>-1/x</c>
    /// at -1, which is <c>-2</c>, a negative answer for a positive integrand whose integral
    /// diverges. An antiderivative can also jump where the integrand is continuous, and the
    /// answer through it is then wrong by the jump: <c>arccot(x)</c> jumps at 0 in this library's
    /// convention, and a half-angle substitution writes <c>tan(x/2)</c>.
    /// </para>
    /// <para>
    /// The places read are where a node of the antiderivative stops being continuous: the zeros
    /// of a denominator, of a base raised to a negative power, of a logarithm's argument, of the
    /// argument of <c>sgn</c>, <c>arccot</c>, <c>arcsec</c> and <c>arccsc</c>, and the zeros of the
    /// cosine or the sine under a tangent, secant, cotangent or cosecant of a linear argument.
    /// Where one of them cannot be listed -- a root with a symbol in it, a periodic argument
    /// that is not linear, more breaks than <see cref="MostBreaks"/> -- the caller is told so,
    /// and integrates as it did before.
    /// </para>
    /// <para>
    /// A break is carried as its exact point, where the one-sided limits are taken, and its
    /// decimal value, which only orders the breaks and places them in the range.
    /// https://github.com/asc-community/AngouriMath/issues/1508
    /// </para>
    /// </remarks>
    internal static class AntiderivativeBreaks
    {
        /// <summary>
        /// More breaks than this in one range are not listed: a tangent over a thousand
        /// periods has no integral to find, and listing its poles would be all the work.
        /// </summary>
        private const int MostBreaks = 64;

        /// <summary>
        /// The points strictly between <paramref name="lower"/> and <paramref name="upper"/>,
        /// ascending, where <paramref name="antiderivative"/> may break, or
        /// <see langword="null"/> where one of them cannot be listed.
        /// </summary>
        internal static List<(Entity At, Real Value)>? Inside(Entity antiderivative, Variable x, Real lower, Real upper)
        {
            var breaks = new List<(Entity At, Real Value)>();
            // A subexpression repeated in the antiderivative is read once.
            var read = new HashSet<Entity>();
            foreach (var node in antiderivative.Nodes)
            {
                if (!read.Add(node))
                    continue;
                var listed = node switch
                {
                    Divf(_, var denominator) when denominator.ContainsNode(x) => ZerosOf(denominator, x, lower, upper, breaks),
                    Powf(var @base, Real { IsNegative: true }) when @base.ContainsNode(x) => ZerosOf(@base, x, lower, upper, breaks),
                    Logf(_, var antilogarithm) when antilogarithm.ContainsNode(x) => ZerosOf(antilogarithm, x, lower, upper, breaks),
                    Signumf(var argument) when argument.ContainsNode(x) => ZerosOf(argument, x, lower, upper, breaks),
                    Arccotanf(var argument) when argument.ContainsNode(x) => ZerosOf(argument, x, lower, upper, breaks),
                    Arcsecantf(var argument) when argument.ContainsNode(x) => ZerosOf(argument, x, lower, upper, breaks),
                    Arccosecantf(var argument) when argument.ContainsNode(x) => ZerosOf(argument, x, lower, upper, breaks),
                    Tanf(var argument) when argument.ContainsNode(x) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: true, breaks),
                    Secantf(var argument) when argument.ContainsNode(x) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: true, breaks),
                    Cotanf(var argument) when argument.ContainsNode(x) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: false, breaks),
                    Cosecantf(var argument) when argument.ContainsNode(x) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: false, breaks),
                    _ => true
                };
                if (!listed || breaks.Count > MostBreaks)
                    return null;
            }
            breaks.Sort((a, b) => a.Value.CompareTo(b.Value));
            // The same point from two nodes -- a pole of -ln(cos(x)) is also a zero of the
            // cosine's -- is one break.
            var distinct = new List<(Entity At, Real Value)>();
            foreach (var point in breaks)
                if (distinct.Count == 0 || !Coincide(distinct[distinct.Count - 1].Value, point.Value))
                    distinct.Add(point);
            return distinct;
        }

        /// <summary>
        /// Adds the real zeros of <paramref name="expression"/> strictly inside the range, and
        /// says whether they could all be listed. A cosine or a sine is zero on a periodic set,
        /// which the solver writes with an integer parameter, so it is listed across the range
        /// instead: <c>-ln(cos(x))</c>, the antiderivative of <c>tan(x)</c>, has no tangent in it
        /// to read. The modulus and a positive power are zero where what they hold is, a product
        /// where one of its factors is, and a quotient where its numerator is.
        /// </summary>
        private static bool ZerosOf(Entity expression, Variable x, Real lower, Real upper, List<(Entity At, Real Value)> breaks)
            => expression switch
            {
                Cosf(var argument) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: true, breaks),
                Sinf(var argument) => PeriodicZeros(argument, x, lower, upper, halfPeriodShift: false, breaks),
                Absf(var inner) => ZerosOf(inner, x, lower, upper, breaks),
                Powf(var inner, Real { IsPositive: true }) => ZerosOf(inner, x, lower, upper, breaks),
                Mulf(var left, var right) =>
                    (!left.ContainsNode(x) || ZerosOf(left, x, lower, upper, breaks))
                    && (!right.ContainsNode(x) || ZerosOf(right, x, lower, upper, breaks)),
                // A quotient is zero where its numerator is; its denominator is read as one.
                Divf(var numerator, _) => !numerator.ContainsNode(x) || ZerosOf(numerator, x, lower, upper, breaks),
                _ => SolvedZeros(expression, x, lower, upper, breaks)
            };

        /// <summary>The real zeros the solver lists for <paramref name="expression"/>, strictly inside the range.</summary>
        private static bool SolvedZeros(Entity expression, Variable x, Real lower, Real upper, List<(Entity At, Real Value)> breaks)
        {
            if (expression.SolveEquation(x) is not FiniteSet roots)
                return false;
            foreach (var root in roots)
            {
                if (root.ContainsNode(x) || root.Vars.Any())
                    return false;
                switch (root.Evaled)
                {
                    case Real { EDecimal.IsFinite: true } value:
                        if (value > lower && value < upper)
                            breaks.Add((root, value));
                        break;
                    case Complex { ImaginaryPart.IsZero: false }:
                        break;
                    default:
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds the zeros of the cosine (<paramref name="halfPeriodShift"/>) or the sine of a
        /// linear argument strictly inside the range: where <c>a x + b</c> is <c>pi/2 + k pi</c>,
        /// or <c>k pi</c>, for a whole <c>k</c>.
        /// </summary>
        private static bool PeriodicZeros(Entity argument, Variable x, Real lower, Real upper, bool halfPeriodShift, List<(Entity At, Real Value)> breaks)
        {
            if (!TreeAnalyzer.TryGetPolyLinear(argument, x, out var a, out var b)
                || a.Evaled is not Real { EDecimal.IsFinite: true, IsZero: false } slope
                || b.Evaled is not Real { EDecimal.IsFinite: true } intercept
                || MathS.pi.Evaled is not Real pi)
                return false;
            if (!lower.EDecimal.IsFinite || !upper.EDecimal.IsFinite)
                return false;
            // x = (c + k pi - b)/a, so k runs over the whole numbers between the ends mapped back.
            var shift = halfPeriodShift ? pi / 2 : Integer.Zero;
            var atLower = (slope * lower + intercept - shift) / pi;
            var atUpper = (slope * upper + intercept - shift) / pi;
            var (from, to) = atLower < atUpper ? (atLower, atUpper) : (atUpper, atLower);
            var first = from.EDecimal.RoundToIntegerNoRoundedFlag(EContext.CliDecimal.WithRounding(ERounding.Floor)).ToEInteger();
            var last = to.EDecimal.RoundToIntegerNoRoundedFlag(EContext.CliDecimal.WithRounding(ERounding.Ceiling)).ToEInteger();
            if ((last - first).CompareTo(EInteger.FromInt32(MostBreaks)) > 0)
                return false;
            for (var k = first; k.CompareTo(last) <= 0; k += EInteger.One)
            {
                var at = ((halfPeriodShift ? MathS.pi / 2 : Integer.Zero) + Integer.Create(k) * MathS.pi - b) / a;
                if (at.InnerSimplified.Evaled is Real { EDecimal.IsFinite: true } value && value > lower && value < upper)
                    breaks.Add((at.InnerSimplified, value));
            }
            return true;
        }

        /// <summary>
        /// Two decimal values of the same point, reached by different exact spellings.
        /// </summary>
        private static bool Coincide(Real a, Real b)
            => a.EDecimal.Subtract(b.EDecimal).Abs().CompareTo(EDecimal.Create(1, -30)) < 0;
    }
}
