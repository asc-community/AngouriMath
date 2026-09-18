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
    /// The residue classes modulo <c>n</c> as sets, and the arithmetic the reference does on them:
    /// a class is written <c>{ x in ZZ : x = r (mod n) }</c>, with <c>r</c> in <c>[0, n)</c>, and
    /// that is what solving a linear congruence answers, what two congruences intersect to by
    /// the Chinese remainder theorem, and what an interval cuts a finite set out of.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A multiplicative inverse modulo <c>n</c> is a class, never a number <c>1/a</c>: the
    /// representative in <c>[1, n - 1]</c> is what <see cref="Inverse"/> gives, and nothing where
    /// <c>a</c> and <c>n</c> share a factor, since then <c>a x = 1 (mod n)</c> has no solution.
    /// A linear congruence <c>a x = b (mod n)</c> is solved through the gcd: with
    /// <c>g = gcd(a, n)</c> it has solutions exactly when <c>g</c> divides <c>b</c>, and they form
    /// one class modulo <c>n / g</c>. Two classes <c>x = a (mod m)</c>, <c>x = b (mod n)</c> meet
    /// in one class modulo <c>lcm(m, n)</c> when <c>a = b (mod gcd(m, n))</c> and nowhere
    /// otherwise, which is the Chinese remainder theorem with the coprime case as the case
    /// where the condition is empty.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// </remarks>
    internal static class ResidueClasses
    {
        /// <summary>The class of <paramref name="residue"/> modulo <paramref name="modulus"/>, as a set over <paramref name="x"/>.</summary>
        internal static Set Create(Variable x, EInteger residue, EInteger modulus)
        {
            modulus = modulus.Abs();
            if (modulus.IsZero)
                return new FiniteSet(Integer.Create(residue));
            if (modulus.Equals(EInteger.One))
                return MathS.Sets.Z;
            return new ConditionalSet(x, x.In(MathS.Sets.Z) & new Congruentf(x, Integer.Create(residue.Mod(modulus)), Integer.Create(modulus)));
        }

        /// <summary>Reads a set written as one residue class: <c>{ x in ZZ : x = r (mod n) }</c>.</summary>
        internal static (Variable x, EInteger residue, EInteger modulus)? Read(Set set)
        {
            if (set is not ConditionalSet { Var: Variable x } builder
                || builder.DeclaredMembership is not (SpecialSet.Integers, var rest))
                return null;
            return rest switch
            {
                Congruentf(var left, Integer r, Integer n) when left == x && !n.EInteger.IsZero => (x, r.EInteger.Mod(n.EInteger.Abs()), n.EInteger.Abs()),
                Congruentf(Integer r, var right, Integer n) when right == x && !n.EInteger.IsZero => (x, r.EInteger.Mod(n.EInteger.Abs()), n.EInteger.Abs()),
                _ => null,
            };
        }

        /// <summary>
        /// The representative in <c>[1, n - 1]</c> of the class inverse to <paramref name="a"/>
        /// modulo <paramref name="n"/>, or <see langword="null"/> where there is none.
        /// </summary>
        internal static EInteger? Inverse(EInteger a, EInteger n)
        {
            n = n.Abs();
            if (n.CompareTo(EInteger.One) <= 0)
                return null;
            var (g, x, _) = ExtendedGcd(a.Mod(n), n);
            return g.Equals(EInteger.One) ? x.Mod(n) : null;
        }

        /// <summary><c>gcd(a, b)</c> with <c>x, y</c> such that <c>a x + b y = gcd</c>.</summary>
        private static (EInteger gcd, EInteger x, EInteger y) ExtendedGcd(EInteger a, EInteger b)
        {
            EInteger oldR = a, r = b, oldS = EInteger.One, s = EInteger.Zero, oldT = EInteger.Zero, t = EInteger.One;
            while (!r.IsZero)
            {
                var quotient = oldR.Divide(r);
                (oldR, r) = (r, oldR - quotient * r);
                (oldS, s) = (s, oldS - quotient * s);
                (oldT, t) = (t, oldT - quotient * t);
            }
            return oldR.Sign < 0 ? (-oldR, -oldS, -oldT) : (oldR, oldS, oldT);
        }

        /// <summary>
        /// The solutions of <c>a x = b (mod n)</c> as a set over <paramref name="x"/>: one class
        /// modulo <c>n / gcd(a, n)</c>, or nothing.
        /// </summary>
        internal static Set SolveLinear(Variable x, EInteger a, EInteger b, EInteger n)
        {
            n = n.Abs();
            if (n.IsZero)
                return a.IsZero ? (b.IsZero ? MathS.Sets.Z : Set.Empty)
                    : b.Remainder(a).IsZero ? new FiniteSet(Integer.Create(b.Divide(a))) : Set.Empty;
            a = a.Mod(n);
            b = b.Mod(n);
            if (a.IsZero)
                return b.IsZero ? MathS.Sets.Z : Set.Empty;
            var g = a.Gcd(n);
            if (!b.Remainder(g).IsZero)
                return Set.Empty;
            var reduced = n.Divide(g);
            var inverse = Inverse(a.Divide(g), reduced) ?? throw new Core.Exceptions.AngouriBugException("a / g and n / g are coprime by construction");
            return Create(x, b.Divide(g) * inverse, reduced);
        }

        /// <summary>
        /// The members of <c>x = a (mod m)</c> that are also <c>x = b (mod n)</c>: one class
        /// modulo <c>lcm(m, n)</c>, or none.
        /// </summary>
        internal static Set Meet(Variable x, EInteger a, EInteger m, EInteger b, EInteger n)
        {
            var g = m.Gcd(n);
            if (!(a - b).Mod(g).IsZero)
                return Set.Empty;
            // x = a + m t, and m t = b - a (mod n): a linear congruence in t modulo n.
            var lcm = m * n / g;
            var mReduced = m.Divide(g);
            var nReduced = n.Divide(g);
            var t = Inverse(mReduced.Mod(nReduced), nReduced) is { } inverse
                ? (b - a).Divide(g) * inverse
                : EInteger.Zero;
            return Create(x, a + m * t, lcm);
        }

        /// <summary>The members of a class that lie in a numeric interval, where there are few enough to list.</summary>
        internal static Set? Within(Variable x, EInteger residue, EInteger modulus, Interval interval)
        {
            if (interval.Left.Evaled is not Real from || interval.Right.Evaled is not Real to || !from.IsFinite || !to.IsFinite)
                return null;
            var lower = from.EDecimal.RoundToExponent(EInteger.Zero, ERounding.Ceiling).ToEInteger();
            if (!interval.LeftClosed && from.EDecimal.CompareTo(EDecimal.FromEInteger(lower)) == 0)
                lower += 1;
            var upper = to.EDecimal.RoundToExponent(EInteger.Zero, ERounding.Floor).ToEInteger();
            if (!interval.RightClosed && to.EDecimal.CompareTo(EDecimal.FromEInteger(upper)) == 0)
                upper -= 1;
            if (upper.CompareTo(lower) < 0)
                return Set.Empty;
            if ((upper - lower).CompareTo(EInteger.FromInt32(LargestListed) * modulus) > 0)
                return null;
            var members = new List<Entity>();
            var first = lower + (residue - lower).Mod(modulus);
            for (var member = first; member.CompareTo(upper) <= 0; member += modulus)
                members.Add(Integer.Create(member));
            return new FiniteSet(members);
        }

        /// <summary>How many members of a class an interval may cut out before the set is left as written.</summary>
        private const int LargestListed = 4096;
    }
}
