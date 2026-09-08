//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Linq;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions.Algebra
{
    /// <summary>
    /// A definite integral whose integrand mentions <c>floor(x)</c> or <c>ceil(x)</c>, split at
    /// the jumps: on <c>[n, n + 1)</c> the floor is <c>n</c> and the ceiling <c>n + 1</c>
    /// exactly, <c>x</c> is <c>n + t</c> with <c>t</c> over <c>[0, 1)</c>, and the integral
    /// over whole bounds is a sum over <c>n</c> of an integral over <c>t</c> that mentions no
    /// step at all. A numeric bound that is not whole contributes the piece from it to the
    /// nearest whole number, on which the step is one known constant.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Exact and unconditional: the step is constant on each piece, a single point has no
    /// measure, and the pieces are exactly the ones the bounds cover. The fractional part
    /// <c>x - floor(x)</c> becomes <c>t</c> by the same substitution, taken first as a unit
    /// because written out it arrives as <c>n + t - n</c>, and a term subtracted from itself
    /// simplifies to a conditional zero rather than to nothing
    /// (<a href="https://github.com/asc-community/AngouriMath/issues/1174">#1174</a>). The
    /// upper bound may be <c>+oo</c>, in which case the sum runs to <c>+oo</c> and is answered
    /// where <see cref="ExponentialSeries"/> and its neighbours answer it.
    /// </para>
    /// <para>
    /// Offered only where every piece then resolves -- the unit-interval integral to something
    /// that is not an integral, the sum to something that is not a sum -- since an integral
    /// rewritten as an unevaluated sum of unevaluated integrals is not an answer. A symbolic
    /// bound is declined: an integral's bound is not a whole number by convention as a
    /// summation's index is, and the pieces depend on where the jumps fall. Question I.2 of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1212">#1212</a>:
    /// <c>integral((x - floor(x)) / floor(x)!, x, 1, +oo)</c> is <c>(e - 1) / 2</c>.
    /// </para>
    /// </remarks>
    internal static class UnitIntervalIntegration
    {
        /// <summary>Whether the integrand mentions <c>floor(x)</c> or <c>ceil(x)</c> of the variable itself.</summary>
        internal static bool HasAStep(Entity expr, Variable x)
            => expr.Nodes.Any(node => node is Floorf(var f) && f == x || node is Ceilf(var c) && c == x);

        internal static Entity? Split(Entity expr, Variable x, Entity from, Entity to)
        {
            if (!HasAStep(expr, x))
                return null;
            if (from.Evaled is not Real lower || !lower.IsFinite)
                return null;
            var unbounded = to.Evaled is Real { IsFinite: false, IsNaN: false, IsNegative: false };
            if (!unbounded && (to.Evaled is not Real || !((Real)to.Evaled).IsFinite))
                return null;
            var upper = unbounded ? null : (Real)to.Evaled;
            if (upper is not null && upper < lower)
                return null;

            // The step is one constant from the lower bound up to the next whole number, and
            // one constant from the last whole number up to the upper bound.
            var firstWhole = Ceiling(lower);
            Entity total = Integer.Zero;
            if (upper is not null && Floor(upper) == Floor(lower) && !IsWhole(lower))
                // Both bounds inside one unit interval: a single piece with the step known.
                return OnAPiece(expr, x, from, to, Floor(lower)) is { } single ? single.InnerSimplified : null;
            if (!IsWhole(lower))
            {
                if (OnAPiece(expr, x, from, Integer.Create(firstWhole), Floor(lower)) is not { } head)
                    return null;
                total += head;
            }
            if (upper is not null && !IsWhole(upper))
            {
                var lastWhole = Floor(upper);
                if (OnAPiece(expr, x, Integer.Create(lastWhole), to, lastWhole) is not { } tail)
                    return null;
                total += tail;
            }

            var taken = expr.Vars.Append(x).ToList();
            var n = Variable.CreateTemp(taken);
            taken.Add(n);
            var t = Variable.CreateTemp(taken);
            var piece = expr
                .Substitute(x - new Floorf(x), t)
                .Substitute(new Floorf(x), n)
                .Substitute(new Ceilf(x), n + Integer.One)
                .Substitute(x, n + t);
            var overUnitInterval = new Integralf(piece, t, (Integer.Zero, Integer.One)).InnerSimplified;
            if (overUnitInterval is Integralf)
                return null;
            Entity lastInterval = unbounded ? to : Integer.Create(Floor(upper!).Subtract(EInteger.One));
            var sum = new Summationf(overUnitInterval, n, Integer.Create(firstWhole), lastInterval).InnerSimplified;
            if (sum is Summationf)
                return null;
            return (total + sum).InnerSimplified;
        }

        /// <summary>
        /// The integral from <paramref name="from"/> to <paramref name="to"/> on a piece where
        /// the floor is <paramref name="floorValue"/> throughout (and the ceiling one more,
        /// the ends aside), or <see langword="null"/> where that integral is not found.
        /// </summary>
        private static Entity? OnAPiece(Entity expr, Variable x, Entity from, Entity to, EInteger floorValue)
        {
            var known = expr
                .Substitute(new Floorf(x), Integer.Create(floorValue))
                .Substitute(new Ceilf(x), Integer.Create(floorValue.Add(EInteger.One)));
            var result = known.Integrate(x, from, to);
            return result is Integralf ? null : result;
        }

        private static bool IsWhole(Real value) => value.EDecimal.IsInteger();
        private static EInteger Floor(Real value) => value.EDecimal.RoundToIntegerNoRoundedFlag(EContext.CliDecimal.WithRounding(ERounding.Floor)).ToEInteger();
        private static EInteger Ceiling(Real value) => value.EDecimal.RoundToIntegerNoRoundedFlag(EContext.CliDecimal.WithRounding(ERounding.Ceiling)).ToEInteger();
    }
}
