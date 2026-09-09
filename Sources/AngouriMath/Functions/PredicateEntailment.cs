//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// Whether one predicate being true forces another to be true, decided structurally and
    /// only where it can be proven.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Written for <see cref="Entity.Piecewise"/>, which takes its first matching case and can
    /// therefore drop any case whose predicate entails an earlier one: wherever the later
    /// predicate holds the earlier one holds too, so the earlier case is taken and the later is
    /// unreachable. That rule was already applied for predicates that are <em>equal</em>; this is
    /// the same rule with a wider notion of "already covered".
    /// </para>
    /// <para>
    /// <b>One-directional and deliberately incomplete.</b> Entailment between arbitrary predicates
    /// is not decidable, so the only answers here are "proven" and "not proven", and the second is
    /// answered <see langword="false"/>. A false negative costs a case that could have been
    /// dropped, which is a longer answer; a false positive would delete a reachable case, which is
    /// a wrong one. Everything below is therefore a sufficient condition, never a heuristic.
    /// </para>
    /// <para>
    /// <b>Three-valued logic does not complicate it.</b> A case is *taken* only when its predicate
    /// is true, so the only thing that has to hold is that a true antecedent forces a true
    /// consequent. What either predicate does when it is <c>NaN</c> — over a non-real argument, say
    /// — never arises, because a case with a <c>NaN</c> predicate is not taken either way.
    /// </para>
    /// <para>
    /// Part of question I.3 of
    /// <a href="https://github.com/asc-community/AngouriMath/issues/1212">#1212</a>, where
    /// distributing a binder over a piecewise produces one case per subset of the conditions and
    /// most of them are unreachable.
    /// </para>
    /// </remarks>
    internal static class PredicateEntailment
    {
        /// <summary>
        /// Whether <paramref name="stronger"/> being true forces <paramref name="weaker"/> to be
        /// true. <see langword="false"/> wherever that is not proven, including where it is true
        /// but not by one of the shapes below.
        /// </summary>
        internal static bool Entails(Entity stronger, Entity weaker)
        {
            if (stronger == weaker)
                return true;
            return (stronger, weaker) switch
            {
                // To force a conjunction is to force both halves of it.
                (_, Andf(var left, var right)) => Entails(stronger, left) && Entails(stronger, right),
                // A conjunction is at least as strong as either half.
                (Andf(var left, var right), _) => Entails(left, weaker) || Entails(right, weaker),
                // A disjunction forces something only if both halves do.
                (Orf(var left, var right), _) => Entails(left, weaker) && Entails(right, weaker),
                // Forcing either half of a disjunction forces the disjunction.
                (_, Orf(var left, var right)) => Entails(stronger, left) || Entails(stronger, right),
                _ => ComparisonEntails(stronger, weaker)
            };
        }

        /// <summary>
        /// Two comparisons of the same expression against numbers: <c>a &gt; 2</c> forces
        /// <c>a &gt; 1</c>, and <c>a &gt;= 2</c> forces <c>a &gt; 1</c>, but not the other way
        /// about. Both are read into the same shape first, so that <c>1 &lt; a</c> and
        /// <c>a &gt; 1</c> are the one statement they are.
        /// </summary>
        private static bool ComparisonEntails(Entity stronger, Entity weaker)
        {
            if (!TryRead(stronger, out var strongSubject, out var strongAbove, out var strongBound, out var strongStrict)
                || !TryRead(weaker, out var weakSubject, out var weakAbove, out var weakBound, out var weakStrict))
                return false;
            // The same unknown, and both bounding it from the same side. A lower bound says
            // nothing about an upper one.
            if (strongSubject != weakSubject || strongAbove != weakAbove)
                return false;
            var comparison = strongBound.EDecimal.CompareTo(weakBound.EDecimal);
            // Bounding from below: `subject > s` forces `subject > w` when s is at least w, and
            // where the bounds are equal only if the stronger is no looser about the endpoint.
            // Bounding from above the comparison is the other way round.
            var atLeastAsTight = strongAbove ? comparison >= 0 : comparison <= 0;
            if (!atLeastAsTight)
                return false;
            // At the same bound, `>=` does not force `>` -- they differ exactly at the endpoint.
            return comparison != 0 || strongStrict || !weakStrict;
        }

        /// <summary>
        /// A comparison as "this expression is above / below this number", or
        /// <see langword="false"/> where it is not one. <paramref name="above"/> is whether the
        /// subject is bounded from below, <paramref name="strict"/> whether the endpoint itself
        /// is excluded.
        /// </summary>
        private static bool TryRead(Entity predicate, out Entity subject, out bool above, out Real bound, out bool strict)
        {
            subject = predicate;
            above = false;
            bound = Integer.Zero;
            strict = false;
            switch (predicate)
            {
                // subject > bound, subject >= bound
                case Greaterf(var left, var right) when Number(right) is { } b:
                    (subject, above, bound, strict) = (left, true, b, true);
                    return true;
                case GreaterOrEqualf(var left, var right) when Number(right) is { } b:
                    (subject, above, bound, strict) = (left, true, b, false);
                    return true;
                // bound < subject, bound <= subject -- the same two, written the other way.
                case Lessf(var left, var right) when Number(left) is { } b:
                    (subject, above, bound, strict) = (right, true, b, true);
                    return true;
                case LessOrEqualf(var left, var right) when Number(left) is { } b:
                    (subject, above, bound, strict) = (right, true, b, false);
                    return true;
                // subject < bound, subject <= bound
                case Lessf(var left, var right) when Number(right) is { } b:
                    (subject, above, bound, strict) = (left, false, b, true);
                    return true;
                case LessOrEqualf(var left, var right) when Number(right) is { } b:
                    (subject, above, bound, strict) = (left, false, b, false);
                    return true;
                // bound > subject, bound >= subject
                case Greaterf(var left, var right) when Number(left) is { } b:
                    (subject, above, bound, strict) = (right, false, b, true);
                    return true;
                case GreaterOrEqualf(var left, var right) when Number(left) is { } b:
                    (subject, above, bound, strict) = (right, false, b, false);
                    return true;
                default:
                    return false;
            }

            // A finite real literal, which is what a bound has to be for the comparison of two
            // of them to mean anything. `+oo` is excluded deliberately: it is a value here, and
            // reasoning about it as an endpoint is not what this is for.
            static Real? Number(Entity what)
                => what.Evaled is Real { IsFinite: true } real ? real : null;
        }
    }
}
