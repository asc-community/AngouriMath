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

        /// <summary>
        /// <paramref name="predicate"/> with every conjunct that <paramref name="expression"/>
        /// implies on its own taken off: a condition <c>not e = 0</c> says nothing where the
        /// expression has no value at the zeros of <c>e</c> anyway. <see cref="Entity.Boolean.True"/>
        /// where nothing is left.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <c>(1 - ln(x))/x^2 provided not x = 0</c> is <c>(1 - ln(x))/x^2</c>: the quotient is
        /// undefined at zero, so the condition excludes a point the expression does not reach,
        /// and a reader is told twice what the expression says once. The same for
        /// <c>(1 + ln(x)) x^x provided not x = 0</c>, where <c>ln(0)</c> and <c>0^0</c> are both
        /// undefined. https://github.com/asc-community/AngouriMath/issues/1394
        /// </para>
        /// <para>
        /// <b>Sound whichever way "undefined" is read.</b> A <c>provided</c> says the expression
        /// has no value where its condition fails; dropping it changes nothing where the
        /// expression already has no value there, whether that absence is written <c>NaN</c> or
        /// a complex infinity (https://github.com/asc-community/AngouriMath/issues/217). What has
        /// to be proven is that the expression <em>is</em> undefined at every zero of <c>e</c>,
        /// and it is proven only structurally: a quotient by <c>d</c>, a negative power of
        /// <c>d</c>, a logarithm of <c>d</c> or to the base <c>d</c>, or <c>d^f</c> with <c>f</c>
        /// vanishing too, where <c>d</c> is <c>e</c> itself, a positive power of it, a product
        /// holding it, or a polynomial in the variable <c>e</c> is with no constant term. Every
        /// such node is undefined at the zeros of <c>e</c>, and an undefined operand makes the
        /// whole expression undefined, since no operation here absorbs one. Anything not proven
        /// keeps its condition: a false negative is a redundant clause, a false positive would
        /// claim a value at a point.
        /// </para>
        /// <para>
        /// Decided on the expression as it stands, which matters: <c>x/x provided not x = 0</c>
        /// simplifies to <c>1</c>, and <c>1</c> says nothing about zero, so the condition on the
        /// simplified body is not redundant and stays.
        /// </para>
        /// </remarks>
        internal static Entity WithoutWhatTheExpressionImplies(Entity expression, Entity predicate)
        {
            // Asked from the simplification of a `provided`, and the reading below simplifies
            // polynomials, which can make a `provided` of their own: one level, and no more.
            if (deciding)
                return predicate;
            deciding = true;
            try
            {
                return Without(expression, predicate);
            }
            finally
            {
                deciding = false;
            }
        }

        [System.ThreadStatic] private static bool deciding;

        private static Entity Without(Entity expression, Entity predicate)
            => predicate switch
            {
                Andf(var left, var right) =>
                    (Without(expression, left), Without(expression, right)) switch
                    {
                        (Entity.Boolean { Value: true }, var rest) => rest,
                        (var rest, Entity.Boolean { Value: true }) => rest,
                        (var l, var r) => l == left && r == right ? predicate : new Andf(l, r),
                    },
                Notf(Equalsf(var e, var zero)) when zero.Evaled is Complex { IsZero: true } && IsUndefinedAtTheZerosOf(expression, e) => Entity.Boolean.True,
                Notf(Equalsf(var zero, var e)) when zero.Evaled is Complex { IsZero: true } && IsUndefinedAtTheZerosOf(expression, e) => Entity.Boolean.True,
                _ => predicate,
            };

        /// <summary>
        /// Whether <paramref name="expression"/> has a node that is undefined wherever
        /// <paramref name="e"/> is zero.
        /// </summary>
        private static bool IsUndefinedAtTheZerosOf(Entity expression, Entity e)
            => expression.Nodes.Any(node => node switch
            {
                Divf(_, var d) => VanishesWith(d, e),
                Powf(var d, var power) => VanishesWith(d, e) && (power.Evaled is Real { IsNegative: true } || VanishesWith(power, e)),
                Logf(var @base, var antilogarithm) => VanishesWith(antilogarithm, e) || VanishesWith(@base, e),
                _ => false,
            });

        /// <summary>
        /// Whether <paramref name="d"/> is zero wherever <paramref name="e"/> is: <c>d</c> is
        /// <c>e</c>, a positive power of it, a product with such a factor, or -- for a variable
        /// <c>e</c> -- a polynomial in it without a constant term.
        /// </summary>
        private static bool VanishesWith(Entity d, Entity e)
        {
            if (d == e)
                return true;
            if (d is Powf(var @base, var power) && power.Evaled is Real { IsPositive: true })
                return VanishesWith(@base, e);
            if (d is Mulf)
                return Mulf.LinearChildren(d).Any(factor => VanishesWith(factor, e));
            if (e is Variable variable && d.ContainsNode(variable)
                && TreeAnalyzer.TryGetPolynomial(d, variable, out var polynomial) && polynomial.Count > 0)
                return !polynomial.ContainsKey(PeterO.Numbers.EInteger.Zero)
                    && polynomial.Keys.All(degree => degree.Sign > 0)
                    && polynomial.Values.All(coefficient => !coefficient.ContainsNode(variable));
            // A linear factor with a numeric root: `x + 1` vanishes with `x^2 + x`, since the
            // latter is zero at -1. Read by substituting the root, which is cheap; a polynomial
            // division here was asked on every quotient of every simplification inside the
            // integrator and made a decline of `x/sin(x)` take a minute.
            if (e is Sumf or Minusf && e.Vars.Count() == 1 && e.Vars.First() is var only && d.ContainsNode(only)
                && TreeAnalyzer.TryGetPolynomial(e, only, out var line) && line.Count == 2
                && line.TryGetValue(PeterO.Numbers.EInteger.One, out var slope) && line.TryGetValue(PeterO.Numbers.EInteger.Zero, out var intercept)
                && slope.Evaled is Complex { IsFinite: true } b && !b.IsZero && intercept.Evaled is Complex { IsFinite: true } a
                && TreeAnalyzer.TryGetPolynomial(d, only, out var polynomialInIt) && polynomialInIt.Values.All(coefficient => coefficient.Evaled is Complex))
                return d.Substitute(only, (-a / b).Evaled).Evaled is Complex { IsZero: true };
            return false;
        }
    }
}
