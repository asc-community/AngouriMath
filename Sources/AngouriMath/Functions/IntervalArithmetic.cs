//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions
{
    /// <summary>
    /// The image of an interval under a function that is monotone on it, and the product and
    /// quotient of two intervals: what <c>ln((0; 1))</c>, <c>e^[0; 1]</c>, <c>[1; 2]^2</c> and
    /// <c>(1; 2) * (3; 4)</c> are as sets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A function that is monotone on an interval maps it to the interval between the images of
    /// its ends, in the same order when it is increasing and the reverse when it is decreasing,
    /// each end attained exactly when the end it comes from is -- so the openness travels with
    /// the end. An end whose image is infinite is never attained: <c>ln((0; 1))</c> is
    /// <c>(-oo; 0)</c>, and <c>ln([0; 1))</c> is the same set, since <c>ln(0)</c> is not a
    /// value. That is the whole of the method, and the only question for each function is on
    /// which intervals it is monotone, which is answered here for the ones the library has
    /// nodes for and declined for the rest: a sine over an interval longer than half a period
    /// is not an interval between two images, and a sign that cannot be read is not guessed.
    /// </para>
    /// <para>
    /// Only numeric ends are read. An interval with a symbolic end could be handled by the same
    /// rule under a condition on the end, and is left as written instead: the answer would be
    /// a piecewise over the sign of something, which is not what a caller asking for a set
    /// wants back.
    /// </para>
    /// <para>
    /// A product of two intervals is the interval between the least and the greatest of the
    /// four products of ends, an end attained where both its factors are, and a quotient is a
    /// product by the reciprocal, which is an interval exactly when the divisor does not
    /// contain zero.
    /// https://github.com/asc-community/AngouriMath/issues/322
    /// </para>
    /// </remarks>
    internal static class IntervalArithmetic
    {
        private static (Real from, Real to)? Ends(Interval interval)
            => interval.Left.Evaled is Real from && interval.Right.Evaled is Real to && !from.IsNaN && !to.IsNaN
                ? (from, to) : null;

        private static int Compare(Real left, Real right) => left.EDecimal.CompareTo(right.EDecimal);

        /// <summary>
        /// Whether the interval lies within <c>[lower; upper]</c>. An end sitting on a bound
        /// where the function is not defined maps to an infinite value, which
        /// <see cref="Monotone"/> leaves open, so the bounds are read closed here.
        /// </summary>
        private static bool Within(Real from, Real to, Real lower, Real upper)
            => Compare(from, lower) >= 0 && Compare(to, upper) <= 0;

        /// <summary>
        /// The image of <paramref name="interval"/> under a function monotone on it, the ends
        /// mapped by <paramref name="map"/>.
        /// </summary>
        internal static Entity? Monotone(Interval interval, bool increasing, Func<Entity, Entity> map, bool isExact)
        {
            if (Ends(interval) is not (var from, var to) || Compare(from, to) > 0)
                return null;
            var lower = map(interval.Left).InnerSimplified(isExact);
            var upper = map(interval.Right).InnerSimplified(isExact);
            if (lower.Evaled is not Real low || upper.Evaled is not Real high || low.IsNaN || high.IsNaN)
                return null;
            var lowerClosed = interval.LeftClosed && low.IsFinite;
            var upperClosed = interval.RightClosed && high.IsFinite;
            return increasing
                ? new Interval(lower, lowerClosed, upper, upperClosed)
                : new Interval(upper, upperClosed, lower, lowerClosed);
        }

        /// <summary>The image under a function monotone on <c>[lower; upper]</c> only.</summary>
        internal static Entity? MonotoneOn(Interval interval, Real lower, Real upper, bool increasing, Func<Entity, Entity> map, bool isExact)
            => Ends(interval) is (var from, var to) && Within(from, to, lower, upper)
                ? Monotone(interval, increasing, map, isExact) : null;

        /// <summary><c>b ^ I</c> for a positive real base: increasing above 1, decreasing below.</summary>
        internal static Entity? Exponential(Entity @base, Interval exponent, bool isExact)
            => @base.Evaled is Real b && b.IsFinite && b.IsPositive && Compare(b, Integer.One) != 0
                ? Monotone(exponent, Compare(b, Integer.One) > 0, e => @base.Pow(e), isExact)
                : null;

        /// <summary><c>log_b(I)</c> for a positive real base other than 1 and an interval of positive numbers.</summary>
        internal static Entity? Logarithm(Entity @base, Interval antilogarithm, bool isExact)
            => @base.Evaled is Real b && b.IsFinite && b.IsPositive && Compare(b, Integer.One) != 0
                ? MonotoneOn(antilogarithm, Integer.Zero, Real.PositiveInfinity, Compare(b, Integer.One) > 0, a => MathS.Log(@base, a), isExact)
                : null;

        /// <summary>
        /// <c>I ^ n</c> for a whole or a unit-fraction exponent: an odd power is increasing
        /// everywhere, an even one on either side of zero and folded across it, a negative one
        /// the reciprocal of the positive one away from zero, and an even root increasing from
        /// zero on.
        /// </summary>
        internal static Entity? Power(Interval @base, Entity exponent, bool isExact)
        {
            if (Ends(@base) is not (var from, var to))
                return null;
            switch (exponent)
            {
                case Integer n when n.EInteger.Sign > 0:
                    if (!n.EInteger.IsEven)
                        return Monotone(@base, increasing: true, b => b.Pow(exponent), isExact);
                    if (Compare(from, Integer.Zero) >= 0)
                        return Monotone(@base, increasing: true, b => b.Pow(exponent), isExact);
                    if (Compare(to, Integer.Zero) <= 0)
                        return Monotone(@base, increasing: false, b => b.Pow(exponent), isExact);
                    // Zero is inside and attained; the far end is whichever is further from it.
                    var (farEnd, farClosed) = Compare(from.Abs(), to.Abs()) > 0
                        ? (@base.Left, @base.LeftClosed)
                        : Compare(from.Abs(), to.Abs()) < 0
                            ? (@base.Right, @base.RightClosed)
                            : (@base.Right, @base.LeftClosed || @base.RightClosed);
                    var far = farEnd.Pow(exponent).InnerSimplified(isExact);
                    return far.Evaled is Real farValue && !farValue.IsNaN
                        ? new Interval(Integer.Zero, true, far, farClosed && farValue.IsFinite)
                        : null;
                case Integer n when n.EInteger.Sign < 0:
                    return Reciprocal(@base, isExact) is Interval reciprocal
                        ? Power(reciprocal, Integer.Create(n.EInteger.Negate()), isExact)
                        : null;
                // A root is increasing from zero on, and only there: the principal cube root of
                // a negative number is not real, so an odd root is no different.
                case Rational q and not Integer when q.ERational.Numerator.Equals(EInteger.One):
                    return MonotoneOn(@base, Integer.Zero, Real.PositiveInfinity, increasing: true, b => b.Pow(exponent), isExact);
                default:
                    return null;
            }
        }

        /// <summary>
        /// <c>1 / I</c> where zero is not inside: decreasing on either side of it. An open end
        /// at zero goes to the infinite end of the interval's sign, since <c>1 / 0</c> is not a
        /// value to map, and an infinite end goes to an open zero.
        /// </summary>
        internal static Entity? Reciprocal(Interval interval, bool isExact)
        {
            if (Ends(interval) is not (var from, var to) || Compare(from, to) > 0)
                return null;
            var positive = Compare(from, Integer.Zero) > 0 || Compare(from, Integer.Zero) == 0 && !interval.LeftClosed;
            var negative = Compare(to, Integer.Zero) < 0 || Compare(to, Integer.Zero) == 0 && !interval.RightClosed;
            if (!positive && !negative)
                return null;
            var (lower, lowerClosed) = Reciprocated(interval.Right, to, interval.RightClosed, Real.NegativeInfinity, isExact);
            var (upper, upperClosed) = Reciprocated(interval.Left, from, interval.LeftClosed, Real.PositiveInfinity, isExact);
            return lower.Evaled is Real low && !low.IsNaN && upper.Evaled is Real high && !high.IsNaN
                ? new Interval(lower, lowerClosed, upper, upperClosed)
                : null;

            static (Entity end, bool closed) Reciprocated(Entity end, Real value, bool closed, Real atZero, bool isExact)
                => value.IsZero ? (atZero, false)
                 : !value.IsFinite ? (Integer.Zero, false)
                 : ((1 / end).InnerSimplified(isExact), closed);
        }

        /// <summary>The product of two intervals with finite numeric ends.</summary>
        internal static Entity? Product(Interval left, Interval right, bool isExact)
        {
            if (Ends(left) is not (var a, var b) || Ends(right) is not (var c, var d))
                return null;
            if (!a.IsFinite || !b.IsFinite || !c.IsFinite || !d.IsFinite || Compare(a, b) > 0 || Compare(c, d) > 0)
                return null;
            var candidates = new (Entity end, bool closed)[]
            {
                (left.Left * right.Left, left.LeftClosed && right.LeftClosed),
                (left.Left * right.Right, left.LeftClosed && right.RightClosed),
                (left.Right * right.Left, left.RightClosed && right.LeftClosed),
                (left.Right * right.Right, left.RightClosed && right.RightClosed),
            };
            (Entity end, Real value, bool closed)? least = null, greatest = null;
            foreach (var (end, closed) in candidates)
            {
                var simplified = end.InnerSimplified(isExact);
                if (simplified.Evaled is not Real value || value.IsNaN)
                    return null;
                if (least is null || Compare(value, least.Value.value) < 0)
                    least = (simplified, value, closed);
                else if (Compare(value, least.Value.value) == 0)
                    least = (least.Value.end, value, least.Value.closed || closed);
                if (greatest is null || Compare(value, greatest.Value.value) > 0)
                    greatest = (simplified, value, closed);
                else if (Compare(value, greatest.Value.value) == 0)
                    greatest = (greatest.Value.end, value, greatest.Value.closed || closed);
            }
            return least is var (lowEnd, _, lowClosed) && greatest is var (highEnd, _, highClosed)
                ? new Interval(lowEnd, lowClosed, highEnd, highClosed)
                : null;
        }

        /// <summary>The quotient of two intervals: a product by the reciprocal, where there is one.</summary>
        internal static Entity? Quotient(Interval dividend, Interval divisor, bool isExact)
            => Reciprocal(divisor, isExact) is Interval reciprocal ? Product(dividend, reciprocal, isExact) : null;

        /// <summary>
        /// <c>abs(I)</c>: the interval itself from zero on, its reflection up to zero, and
        /// folded across zero otherwise.
        /// </summary>
        internal static Entity? Absolute(Interval interval, bool isExact)
        {
            if (Ends(interval) is not (var from, var to) || Compare(from, to) > 0)
                return null;
            if (Compare(from, Integer.Zero) >= 0)
                return interval;
            if (Compare(to, Integer.Zero) <= 0)
                return Monotone(interval, increasing: false, e => -e, isExact);
            var (farEnd, farClosed) = Compare(from.Abs(), to.Abs()) > 0
                ? (-interval.Left, interval.LeftClosed)
                : Compare(from.Abs(), to.Abs()) < 0
                    ? (interval.Right, interval.RightClosed)
                    : (interval.Right, interval.LeftClosed || interval.RightClosed);
            var far = farEnd.InnerSimplified(isExact);
            return far.Evaled is Real farValue && !farValue.IsNaN
                ? new Interval(Integer.Zero, true, far, farClosed && farValue.IsFinite)
                : null;
        }
    }
}
