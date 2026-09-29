//
// Copyright (c) 2019-2026 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Collections.Generic;
using PeterO.Numbers;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;

namespace AngouriMath.Functions
{
    /// <summary>
    /// A sum of an Iverson bracket over a range of whole numbers is a count, written in closed
    /// form: how many whole numbers of the range satisfy the statement. A product of brackets is
    /// the bracket of the conjunction.
    /// <c>sum(iverson(2 divides k or 3 divides k), k, 1, 1000)</c> is <c>667</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The statement is read as conditions on the index. An equation linear in it fixes it at a
    /// point <c>p</c>, and then the sum is a bracket again, of <c>p</c> being whole, in the range,
    /// and satisfying the other conditions: in a nested sum that is what the next sum out counts.
    /// A comparison linear in the index, with a numeric slope, bounds it from one side, and so
    /// does a membership in an interval. A divisibility of a linear function of the index with
    /// whole coefficients fixes its residue, and several residues combine into one class or
    /// none, by the Chinese remainder theorem. A conjunct that does not mention the index is a
    /// factor, and so is a linear function of it being whole, which depends only on its constant
    /// term. What is left is the number of whole numbers from <c>L</c> to <c>U</c> in the class
    /// <c>r</c> modulo <c>m</c>, <c>floor((U - r)/m) - floor((L - 1 - r)/m)</c>, or
    /// <c>U - L + 1</c> with no class, and never below zero, which is also the count of an empty
    /// range. A bound that is not a number is read as real, as a bound of the range that is not a
    /// number is read as whole.
    /// </para>
    /// <para>
    /// A negation and a disjunction are counted by inclusion and exclusion: <c>not Q</c> counts
    /// the range less <c>Q</c>, and <c>Q or R</c> counts <c>Q</c> and <c>R</c> less both. So
    /// <c>sum(iverson(not 2 divides k and not 5 divides k), k, 1, 99)</c> is <c>40</c>, Sullivan and
    /// Mackey's §8.7.4 Try 1.
    /// </para>
    /// https://github.com/asc-community/AngouriMath/issues/1409
    /// https://github.com/asc-community/AngouriMath/issues/1478
    /// </remarks>
    internal static class IversonSum
    {
        // Each negation and disjunction doubles the counts taken; past this many the sum is declined.
        private const int MaxCounts = 64;

        // Residue classes are combined by stepping through one modulus, so a larger one is declined.
        private const int MaxModulus = 1 << 16;

        // A perfect power's exponent past this is declined; the least common multiple grows fast.
        private const int MaxPowerExponent = 1 << 10;

        /// <summary>
        /// <c>sum(c iverson(P), index, from, to)</c> as a count, for a <c>c</c> free of the index,
        /// or <see langword="null"/>.
        /// </summary>
        internal static Entity? ClosedForm(Entity expression, Entity var, Entity from, Entity to)
        {
            if (var is not Variable index)
                return null;
            if (!PolynomialSummation.IsWholeOrSymbolic(from) || !PolynomialSummation.IsWholeOrSymbolic(to)
                || from.ContainsNode(index) || to.ContainsNode(index))
                return null;
            if (Read(expression, index, from, to) is { } count)
                return count;
            // A sum of a bracket inside is counted first, in terms of this index, and where that
            // count is a bracket again it is counted here. Only a sum with a bracket in it is
            // simplified for this: any other would be written out with this index a symbol.
            if (expression is Summationf && ContainsABracket(expression)
                && expression.InnerSimplified is var inner && !ReferenceEquals(inner, expression))
                return Read(inner, index, from, to);
            return null;
        }

        private static bool ContainsABracket(Entity expression)
        {
            foreach (var node in expression.Nodes)
                if (node is Iversonf)
                    return true;
            return false;
        }

        private static Entity? Read(Entity expression, Variable index, Entity from, Entity to)
        {
            // A product of brackets is the bracket of the conjunction.
            var statements = new List<Entity>();
            Entity coefficient = Integer.One;
            foreach (var factor in Mulf.LinearChildren(expression))
            {
                if (factor is Iversonf bracket && bracket.Argument.ContainsNode(index))
                    statements.Add(bracket.Argument);
                else if (factor.ContainsNode(index))
                    return null;
                else
                    coefficient *= factor;
            }
            if (statements.Count == 0)
                return null;
            var budget = MaxCounts;
            if (Count(statements, index, from, to, ref budget) is not { } count)
                return null;
            return (coefficient * count).InnerSimplified;
        }

        /// <summary>
        /// The number of whole <paramref name="k"/> from <paramref name="from"/> to
        /// <paramref name="to"/> that satisfy every one of <paramref name="conjuncts"/>, or
        /// <see langword="null"/>.
        /// </summary>
        private static Entity? Count(List<Entity> conjuncts, Variable k, Entity from, Entity to, ref int budget)
        {
            if (--budget < 0)
                return null;
            var atoms = new List<Entity>();
            foreach (var conjunct in conjuncts)
                Flatten(conjunct, atoms);

            // A negation or a disjunction is split first, by inclusion and exclusion over the rest.
            for (var i = 0; i < atoms.Count; i++)
            {
                if (!atoms[i].ContainsNode(k))
                    continue;
                if (atoms[i] is Notf(var negated))
                {
                    var rest = Without(atoms, i);
                    if (Count(rest, k, from, to, ref budget) is not { } all)
                        return null;
                    rest.Add(negated);
                    if (Count(rest, k, from, to, ref budget) is not { } negatedCount)
                        return null;
                    return all - negatedCount;
                }
                if (atoms[i] is Orf(var either, var other))
                {
                    var withLeft = Without(atoms, i);
                    var withRight = Without(atoms, i);
                    var withBoth = Without(atoms, i);
                    withLeft.Add(either);
                    withRight.Add(other);
                    withBoth.Add(either);
                    withBoth.Add(other);
                    if (Count(withLeft, k, from, to, ref budget) is not { } left
                        || Count(withRight, k, from, to, ref budget) is not { } right
                        || Count(withBoth, k, from, to, ref budget) is not { } both)
                        return null;
                    return left + right - both;
                }
            }

            // An equation fixes the index: the sum of [k = p and Q(k)] is [p whole, in the range,
            // and Q(p)], a bracket again, which is what lets a sum outside count it in turn.
            for (var i = 0; i < atoms.Count; i++)
            {
                if (atoms[i] is not Equalsf(var equationLeft, var equationRight) || !atoms[i].ContainsNode(k)
                    || Root(equationLeft - equationRight, k) is not { } point)
                    continue;
                Entity condition = (from <= point) & (point <= to);
                if (point.Evaled is not Integer)
                    condition = point.In(MathS.Sets.Z) & condition;
                for (var j = 0; j < atoms.Count; j++)
                    if (j != i)
                        condition &= atoms[j].Substitute(k, point);
                return MathS.Iverson(condition);
            }

            var lower = new List<Entity> { from };
            var upper = new List<Entity> { to };
            var factors = new List<Entity>();
            var residue = EInteger.Zero;
            var modulus = EInteger.One;
            // k = m^e for some whole m, where every such condition's e divides this one.
            var powerExponent = 0;
            foreach (var atom in atoms)
            {
                if (!atom.ContainsNode(k))
                {
                    factors.Add(atom);
                    continue;
                }
                switch (atom)
                {
                    case Dividesf(var divisor, var dividend):
                        if (Residue(divisor, dividend, k) is not { } residueClass)
                            return null;
                        if (residueClass.Modulus.IsZero)
                            return Integer.Zero;
                        if (Combined(residue, modulus, residueClass.Residue, residueClass.Modulus) is not { } combinedClass)
                            return null;
                        if (combinedClass.Modulus.IsZero)
                            return Integer.Zero;
                        (residue, modulus) = (combinedClass.Residue, combinedClass.Modulus);
                        break;
                    // k is a perfect e-th power: counted by the root of each end, over a range of
                    // positive whole numbers, and two such conditions are the power of the least
                    // common multiple -- a square that is a cube is a sixth power.
                    case Existsf(var root, var over, var body) when PowerExponent(root, over, body, k) is { } exponent:
                        powerExponent = powerExponent == 0 ? exponent : Lcm(powerExponent, exponent);
                        if (powerExponent > MaxPowerExponent)
                            return null;
                        break;
                    // s k + c is whole for a whole k exactly where c is, s being whole.
                    case Set.Inf(var element, Set.SpecialSet.Integers):
                        if (!TreeAnalyzer.TryGetPolyLinear(element.InnerSimplified, k, out var memberSlope, out var memberConstant)
                            || memberConstant.ContainsNode(k) || memberSlope.Evaled is not Integer)
                            return null;
                        factors.Add(memberConstant.In(MathS.Sets.Z));
                        break;
                    case Greaterf or GreaterOrEqualf or Lessf or LessOrEqualf:
                        if (atom is not IBinaryNode { NodeFirstChild: var lhs, NodeSecondChild: var rhs })
                            return null;
                        if (!TreeAnalyzer.TryGetPolyLinear((lhs - rhs).InnerSimplified, k, out var slopeEntity, out var intercept)
                            || intercept.ContainsNode(k) || slopeEntity.Evaled is not Real slope || slope.IsZero)
                            return null;
                        // a k + b compared with 0: the point where it is 0, and which side of it holds.
                        var bound = (-intercept / slopeEntity).InnerSimplified;
                        // Off the real line the comparison has no truth value, and neither has the sum.
                        if (bound.Evaled is Complex boundValue && !boundValue.ImaginaryPart.IsZero)
                            return null;
                        var rising = slope.IsPositive;
                        switch (atom)
                        {
                            case Greaterf:
                                if (rising) lower.Add(MathS.Floor(bound) + 1); else upper.Add(MathS.Ceil(bound) - 1);
                                break;
                            case GreaterOrEqualf:
                                if (rising) lower.Add(MathS.Ceil(bound)); else upper.Add(MathS.Floor(bound));
                                break;
                            case Lessf:
                                if (rising) upper.Add(MathS.Ceil(bound) - 1); else lower.Add(MathS.Floor(bound) + 1);
                                break;
                            default:
                                if (rising) upper.Add(MathS.Floor(bound)); else lower.Add(MathS.Ceil(bound));
                                break;
                        }
                        break;
                    default:
                        return null;
                }
            }

            Entity least = lower[0], greatest = upper[0];
            for (var i = 1; i < lower.Count; i++)
                least = MathS.Max(least, lower[i]);
            for (var i = 1; i < upper.Count; i++)
                greatest = MathS.Min(greatest, upper[i]);
            Entity count;
            if (powerExponent > 0)
            {
                // The powers of the positive whole numbers up to U are floor(U^(1/e)) many; below
                // 1 the powers of zero and of negative numbers would enter, so the range starts
                // there, and a residue class on top of a power is not counted here.
                if (!modulus.Equals(EInteger.One) || from.Evaled is not Integer { EInteger.Sign: > 0 })
                    return null;
                count = RootFloor(greatest, powerExponent) - RootFloor(least - 1, powerExponent);
            }
            else
                count = modulus.Equals(EInteger.One)
                    ? greatest - least + 1
                    : MathS.Floor((greatest - Integer.Create(residue)) / Integer.Create(modulus))
                        - MathS.Floor((least - 1 - Integer.Create(residue)) / Integer.Create(modulus));
            count = MathS.Max(0, count);
            if (factors.Count > 0)
            {
                var condition = factors[0];
                for (var i = 1; i < factors.Count; i++)
                    condition &= factors[i];
                count = MathS.Iverson(condition) * count;
            }
            return count;
        }

        /// <summary>
        /// The conjuncts of a statement, with a membership in an interval written as the two
        /// comparisons it is.
        /// </summary>
        private static void Flatten(Entity statement, List<Entity> into)
        {
            switch (statement)
            {
                case Andf(var left, var right):
                    Flatten(left, into);
                    Flatten(right, into);
                    break;
                case Set.Inf(var element, Set.Interval interval):
                    if (interval.Left is not Real { IsFinite: false })
                        into.Add(interval.LeftClosed ? interval.Left <= element : interval.Left < element);
                    if (interval.Right is not Real { IsFinite: false })
                        into.Add(interval.RightClosed ? element <= interval.Right : element < interval.Right);
                    break;
                default:
                    into.Add(statement);
                    break;
            }
        }

        private static List<Entity> Without(List<Entity> atoms, int at)
        {
            var rest = new List<Entity>(atoms.Count);
            for (var i = 0; i < atoms.Count; i++)
                if (i != at)
                    rest.Add(atoms[i]);
            return rest;
        }

        /// <summary>
        /// Where a function linear in <paramref name="k"/> with a real numeric slope is zero, or
        /// <see langword="null"/>: the point an equation fixes the index at.
        /// </summary>
        private static Entity? Root(Entity difference, Variable k)
        {
            if (!TreeAnalyzer.TryGetPolyLinear(difference.InnerSimplified, k, out var slope, out var constant)
                || constant.ContainsNode(k) || slope.Evaled is not Real { IsZero: false })
                return null;
            var root = (-constant / slope).InnerSimplified;
            return root.Evaled is Complex rootValue && !rootValue.ImaginaryPart.IsZero ? null : root;
        }

        /// <summary>
        /// <c>e</c> where <c>exists m in S : m^e = k</c> says that <paramref name="k"/> is a
        /// perfect <c>e</c>-th power, for <c>S</c> the positive, the non-negative or all whole
        /// numbers, or <see langword="null"/>. Over a range of positive whole numbers the three
        /// agree: zero is out of it, and so is every odd power of a negative number.
        /// </summary>
        private static int? PowerExponent(Entity root, Entity over, Entity body, Variable k)
        {
            if (root is not Variable m || m == k
                || over is not (Set.SpecialSet.PositiveIntegers or Set.SpecialSet.NonNegativeIntegers or Set.SpecialSet.Integers))
                return null;
            var power = body switch
            {
                Equalsf(Powf(var powerBase, var powerExponent), var target) when powerBase == m && target == k => powerExponent,
                Equalsf(var target, Powf(var powerBase, var powerExponent)) when powerBase == m && target == k => powerExponent,
                _ => null,
            };
            return power?.Evaled is Integer { EInteger: var e } && e.CompareTo(EInteger.FromInt32(2)) >= 0 && e.CanFitInInt32()
                ? e.ToInt32Checked()
                : null;
        }

        private static int Lcm(int a, int b)
        {
            int x = a, y = b;
            while (y != 0)
                (x, y) = (y, x % y);
            return a / x * b;
        }

        /// <summary>
        /// The number of positive whole <c>m</c> with <c>m^e</c> at most <paramref name="bound"/>:
        /// computed exactly where the bound is a number, and <c>floor(bound^(1/e))</c> where not.
        /// </summary>
        private static Entity RootFloor(Entity bound, int e)
        {
            if (bound.Evaled is not Integer { EInteger: var value })
                return MathS.Floor(MathS.Pow(bound, Rational.Create(1, e)));
            if (value.Sign <= 0)
                return Integer.Zero;
            // The largest m with m^e <= value, by bisection between 0 and 2^(bits/e + 1).
            var low = EInteger.Zero;
            var high = EInteger.One.ShiftLeft((int)(value.GetUnsignedBitLengthAsInt64() / e) + 1);
            while (low.CompareTo(high) < 0)
            {
                var middle = low.Add(high).Add(EInteger.One).ShiftRight(1);
                if (middle.Pow(e).CompareTo(value) <= 0)
                    low = middle;
                else
                    high = middle.Subtract(EInteger.One);
            }
            return Integer.Create(low);
        }

        /// <summary>
        /// The class of whole <paramref name="k"/> for which a whole <paramref name="divisor"/>
        /// divides <c>s k + e</c>, with <c>s</c> and <c>e</c> whole: a residue and a modulus, the
        /// modulus zero where no <c>k</c> does, or <see langword="null"/>.
        /// </summary>
        private static (EInteger Residue, EInteger Modulus)? Residue(Entity divisor, Entity dividend, Variable k)
        {
            if (divisor.ContainsNode(k) || divisor.Evaled is not Integer { IsZero: false } d)
                return null;
            if (!TreeAnalyzer.TryGetPolyLinear(dividend.InnerSimplified, k, out var slopeEntity, out var interceptEntity)
                || slopeEntity.Evaled is not Integer s || interceptEntity.Evaled is not Integer e)
                return null;
            var m = d.EInteger.Abs();
            if (m.CompareTo(EInteger.FromInt32(MaxModulus)) > 0)
                return null;
            // s k + e = 0 mod m: solvable where g = gcd(s, m) divides e, and then k is one class
            // modulo m / g, found by trying its members.
            var g = s.EInteger.Gcd(m);
            if (!e.EInteger.Remainder(g).IsZero)
                return (EInteger.Zero, EInteger.Zero);
            var classModulus = m.Divide(g);
            for (var r = EInteger.Zero; r.CompareTo(classModulus) < 0; r = r.Add(EInteger.One))
                if (s.EInteger.Multiply(r).Add(e.EInteger).Mod(m).IsZero)
                    return (r, classModulus);
            return (EInteger.Zero, EInteger.Zero);
        }

        /// <summary>
        /// The class that is <paramref name="r1"/> modulo <paramref name="m1"/> and
        /// <paramref name="r2"/> modulo <paramref name="m2"/> at once, the modulus zero where the two
        /// are disjoint, or <see langword="null"/> where the combined modulus is too large to step
        /// through.
        /// </summary>
        private static (EInteger Residue, EInteger Modulus)? Combined(EInteger r1, EInteger m1, EInteger r2, EInteger m2)
        {
            var combined = m1.Divide(m1.Gcd(m2)).Multiply(m2);
            if (combined.CompareTo(EInteger.FromInt32(MaxModulus)) > 0)
                return null;
            for (var t = r1; t.CompareTo(combined) < 0; t = t.Add(m1))
                if (t.Subtract(r2).Mod(m2).IsZero)
                    return (t, combined);
            return (EInteger.Zero, EInteger.Zero);
        }
    }
}
