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

namespace AngouriMath.Core.Sets
{
    /// <summary>
    /// Sizes of sets, Sullivan and Mackey's §7.6: <c>card</c> of an infinite set is an aleph or a
    /// power of <c>2</c> of one, sums and products of sizes are the larger, and sizes compare.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <c>PP</c>, <c>ZZ+</c>, <c>ZZ*</c>, <c>ZZ</c> and <c>QQ</c> are <c>aleph(0)</c> in size.
    /// <c>RR</c>, <c>CC</c> and every interval with two distinct ends are <c>2^aleph(0)</c>, the size
    /// of the power set of <c>ZZ+</c>. That is an aleph too, but which one is the continuum
    /// hypothesis, which ZFC does not decide, so it is not written as <c>aleph(1)</c>. A power set
    /// is <c>2^card(S)</c>; a union with an infinite side is its larger side; and an infinite
    /// <c>A</c> less a smaller <c>B</c> keeps the size of <c>A</c>, so the irrationals are the size
    /// of <c>RR</c>.
    /// </para>
    /// <para>
    /// A size read here is a finite count, or a tower <c>2^(...^(2^aleph(k)))</c> of height
    /// <c>t</c>, <c>t = 0</c> being <c>aleph(k)</c> itself. Only what ZFC proves is used:
    /// <c>aleph(j) &lt; aleph(k)</c> for <c>j &lt; k</c>; <c>s &lt; 2^s</c> for every size, Cantor's
    /// theorem; <c>2^s &gt;= aleph(k + 1)</c> for <c>s &gt;= aleph(k)</c>, so a tower of height
    /// <c>t</c> over <c>aleph(k)</c> is at least <c>aleph(k + t)</c>; and <c>2^s &lt;= 2^u</c> for
    /// <c>s &lt;= u</c>. What these leave open stays written: <c>2^aleph(0) = aleph(1)</c> is
    /// consistent with ZFC and so is its negation. A subset is no larger than its superset, which
    /// settles one side of a comparison between two sizes that are not read.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class Cardinality
    {
        // A finite power set is counted as 2^n up to this n; past it, only that it is finite.
        private const int MaxCountedExponent = 4096;

        [ConstantField] private static readonly EInteger Two = EInteger.FromInt32(2);

        /// <summary>
        /// A size: finite, with <see cref="Count"/> where it is known, or infinite, a tower of
        /// height <see cref="Tower"/> over <c>aleph(<see cref="Aleph"/>)</c>.
        /// </summary>
        private readonly record struct Size(bool Infinite, EInteger? Count, int Aleph, int Tower)
        {
            internal static Size Finite(EInteger? count) => new(false, count, 0, 0);
            internal static Size Of(int aleph, int tower) => new(true, null, aleph, tower);
        }

        /// <summary>
        /// The size of the set a <c>card</c> is taken of, as an aleph or a power of one, where the
        /// set is infinite and its size read, or <see langword="null"/>.
        /// </summary>
        internal static Entity? Of(Entity set, bool isExact)
        {
            switch (set)
            {
                case SpecialSet special:
                    return special.ToDomain() switch
                    {
                        Domain.Boolean => Integer.Create(2),
                        Domain.Prime or Domain.PositiveInteger or Domain.NonNegativeInteger or Domain.Integer or Domain.Rational => MathS.Aleph(0),
                        Domain.Real or Domain.Complex => MathS.Pow(2, MathS.Aleph(0)),
                        _ => null,
                    };
                case Interval { Left: Real left, Right: Real right } when left.EDecimal.CompareTo(right.EDecimal) < 0:
                    return MathS.Pow(2, MathS.Aleph(0));
                // |P(S)| = 2^|S|, whatever S is.
                case Powersetf(var of):
                    return MathS.Pow(2, MathS.Sets.Card(of).InnerSimplified(isExact)).InnerSimplified(isExact);
                // |A u B| = max(|A|, |B|) where one side is infinite.
                case Unionf(var either, var other):
                    return Absorbed(MathS.Sets.Card(either).InnerSimplified(isExact), MathS.Sets.Card(other).InnerSimplified(isExact));
                // An infinite A less a smaller B keeps A's size: were A \ B smaller too, A, their
                // union with A /\ B, would be smaller than itself.
                case SetMinusf(var whole, var removed):
                    var wholeSize = MathS.Sets.Card(whole).InnerSimplified(isExact);
                    return SizeOf(wholeSize, false) is { Infinite: true } wholeRead
                        && SizeOf(MathS.Sets.Card(removed).InnerSimplified(isExact), false) is { } removedRead
                        && Signs(removedRead, wholeRead) == (true, false, false)
                        ? wholeSize
                        : null;
                // RR \ QQ arrives as { x in RR : not x in QQ }, and is read as the difference.
                case ConditionalSet builder when builder.DeclaredMembership is (var declared, var rest) && builder.Var is Variable member
                    && rest is Notf(Inf(var element, var excluded)) && element == member && !excluded.ContainsNode(member):
                    return Of(new SetMinusf(declared, excluded), isExact);
                default:
                    return null;
            }
        }

        /// <summary>
        /// The sum or, neither being zero, the product of two sizes one of which is infinite: the
        /// larger, or <c>NaN</c> where the other is a number that is not a size -- a whole number
        /// from zero -- since a size less one, or a half of one, has no value. <see langword="null"/>
        /// where no side is an infinite size, or the two do not compare.
        /// </summary>
        /// <remarks>
        /// No value is <c>NaN</c>, as for <c>log(0)</c>, which is how Happypig375 asked for it on
        /// https://github.com/asc-community/AngouriMath/issues/1409.
        /// </remarks>
        internal static Entity? Absorbed(Entity a, Entity b)
        {
            if (!IsInfinite(a) && !IsInfinite(b))
                return null;
            if (a is Number { IsNaN: false } && SizeOf(a, false) is null || b is Number { IsNaN: false } && SizeOf(b, false) is null)
                return MathS.NaN;
            if (SizeOf(a, false) is not { } sizeA || SizeOf(b, false) is not { } sizeB)
                return null;
            return Signs(sizeA, sizeB) switch
            {
                (true, false, false) => b,
                (false, true, false) or (false, false, true) => a,
                _ => null,
            };
        }

        /// <summary>
        /// Whether a difference or a quotient with these two sides has no value: one of them is an
        /// infinite size, and sizes are not subtracted or divided -- taking the even numbers from
        /// the whole numbers leaves <c>aleph(0)</c> of them and taking all of them none.
        /// </summary>
        internal static bool HasNoValue(Entity a, Entity b)
            => IsInfinite(a) || IsInfinite(b);

        /// <summary>
        /// Whether an expression stands for a size: a <c>card</c>, an aleph, or a power of
        /// <c>2</c> of a size.
        /// </summary>
        internal static bool IsSize(Entity expression)
            => expression is Cardf or Alephf
                || expression is Powf(Integer { EInteger: var two }, var exponent) && two.Equals(Two) && IsSize(exponent);

        private static bool IsInfinite(Entity expression)
            => expression is Alephf
                || expression is Powf(Integer { EInteger: var two }, var exponent) && two.Equals(Two) && IsInfinite(exponent);

        /// <summary>
        /// The comparison of two sizes, <paramref name="left"/> against <paramref name="right"/>,
        /// as a truth value where every sign the two can have agrees on it, or
        /// <see langword="null"/>: <paramref name="holds"/> says whether the statement holds for
        /// the sign of <c>left - right</c>.
        /// </summary>
        internal static Entity? Compare(Entity left, Entity right, Func<int, bool> holds, bool isExact)
        {
            var (less, equal, greater) = PossibleSigns(left, right, isExact);
            if (!less && !equal && !greater)
                return null;
            var any = (less && holds(-1)) || (equal && holds(0)) || (greater && holds(1));
            var all = (!less || holds(-1)) && (!equal || holds(0)) && (!greater || holds(1));
            return all ? Entity.Boolean.True : !any ? Entity.Boolean.False : null;
        }

        /// <summary>
        /// Which signs <c>left - right</c> can have; none of the three where nothing is known.
        /// </summary>
        private static (bool Less, bool Equal, bool Greater) PossibleSigns(Entity left, Entity right, bool isExact)
        {
            if (left == right)
                return (false, true, false);
            // Cantor's theorem: s < 2^s, whatever the size s is.
            if (IsSize(left) && right is Powf(Integer { EInteger: var twoRight }, var rightExponent) && twoRight.Equals(Two) && rightExponent == left)
                return (true, false, false);
            if (IsSize(right) && left is Powf(Integer { EInteger: var twoLeft }, var leftExponent) && twoLeft.Equals(Two) && leftExponent == right)
                return (false, false, true);
            if (SizeOf(left, true) is { } leftSize && SizeOf(right, true) is { } rightSize)
                return Signs(leftSize, rightSize);
            // A subset is no larger than its superset.
            if (left is Cardf(var subCandidate) && right is Cardf(var superCandidate))
            {
                if (SetOperators.Subset(subCandidate, superCandidate, isExact) == Entity.Boolean.True)
                    return (true, true, false);
                if (SetOperators.Subset(superCandidate, subCandidate, isExact) == Entity.Boolean.True)
                    return (false, true, true);
            }
            return (false, false, false);
        }

        /// <summary>
        /// Which signs <c>left - right</c> can have for two sizes read, by what ZFC proves of
        /// alephs and powers of two; all three where it proves nothing.
        /// </summary>
        private static (bool Less, bool Equal, bool Greater) Signs(Size left, Size right)
        {
            const bool y = true, n = false;
            switch (left.Infinite, right.Infinite)
            {
                case (false, false):
                    if (left.Count is { } a && right.Count is { } b)
                        return a.CompareTo(b) switch { < 0 => (y, n, n), 0 => (n, y, n), _ => (n, n, y) };
                    return (y, y, y);
                case (false, true):
                    return (y, n, n);
                case (true, false):
                    return (n, n, y);
            }
            if (left.Tower == 0 && right.Tower == 0)
                return left.Aleph.CompareTo(right.Aleph) switch { < 0 => (y, n, n), 0 => (n, y, n), _ => (n, n, y) };
            if (left.Tower == 0)
                return AlephAgainstTower(left.Aleph, right.Aleph, right.Tower);
            if (right.Tower == 0)
            {
                var (less, equal, greater) = AlephAgainstTower(right.Aleph, left.Aleph, left.Tower);
                return (greater, equal, less);
            }
            // Two towers: over one aleph, the higher is larger, by Cantor's theorem each step;
            // over a smaller aleph and no higher, it is no larger, since 2^s is monotone in s.
            if (left.Aleph == right.Aleph)
                return left.Tower.CompareTo(right.Tower) switch { < 0 => (y, n, n), 0 => (n, y, n), _ => (n, n, y) };
            if (left.Aleph < right.Aleph && left.Tower <= right.Tower)
                return left.Tower < right.Tower ? (y, n, n) : (y, y, n);
            if (left.Aleph > right.Aleph && left.Tower >= right.Tower)
                return left.Tower > right.Tower ? (n, n, y) : (n, y, y);
            return (y, y, y);
        }

        /// <summary>
        /// <c>aleph(j)</c> against a tower of height <paramref name="tower"/> over
        /// <c>aleph(k)</c>, which is at least <c>aleph(k + tower)</c>: smaller below that,
        /// no larger at it, and anything above it.
        /// </summary>
        private static (bool Less, bool Equal, bool Greater) AlephAgainstTower(int j, int k, int tower)
            => ((long)j).CompareTo((long)k + tower) switch
            {
                < 0 => (true, false, false),
                0 => (true, true, false),
                _ => (true, true, true),
            };

        /// <summary>
        /// The size an expression stands for, or <see langword="null"/>: a whole number from zero,
        /// any finite real where <paramref name="numbers"/> allows it, an aleph, a power of two
        /// of a size, or <c>card</c> of a listed set.
        /// </summary>
        private static Size? SizeOf(Entity expression, bool numbers)
        {
            switch (expression)
            {
                case Integer { EInteger: var count } when count.Sign >= 0:
                    return Size.Finite(count);
                case Real { IsFinite: true, IsNaN: false } when numbers:
                    return Size.Finite(null);
                case Alephf(Integer { EInteger: var index }) when index.Sign >= 0 && index.CanFitInInt32():
                    return Size.Of(index.ToInt32Checked(), 0);
                case Powf(Integer { EInteger: var two }, var exponent) when two.Equals(Two):
                    if (SizeOf(exponent, false) is not { } power)
                        return null;
                    if (power.Infinite)
                        return Size.Of(power.Aleph, power.Tower + 1);
                    return Size.Finite(power.Count is { } counted && counted.CanFitInInt32() && counted.ToInt32Checked() <= MaxCountedExponent
                        ? EInteger.One.ShiftLeft(counted.ToInt32Checked())
                        : null);
                case Cardf(FiniteSet listed):
                    return Size.Finite(MathS.Sets.Card(listed).InnerSimplified is Integer { EInteger: var listedCount } ? listedCount : null);
                default:
                    return null;
            }
        }
    }
}
