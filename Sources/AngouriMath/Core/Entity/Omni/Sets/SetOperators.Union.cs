//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core.Sets
{
    internal static partial class SetOperators
    {
        /// <summary>
        /// A union of numeric intervals and listed real numbers written as its disjoint pieces:
        /// the listed numbers first, in increasing order, then the intervals in increasing order,
        /// a point at an open end closing it and an empty interval dropped. <c>x^2 &gt;= x</c> is
        /// solved to <c>{ 0, 1 } \/ (-oo; 0) \/ (1; +oo)</c>, which is <c>(-oo; 0] \/ [1; +oo)</c>,
        /// and two ways of writing one set of numbers come out the same. <see langword="null"/>
        /// where a piece is anything else, or the union is written so already.
        /// https://github.com/asc-community/AngouriMath/issues/1409
        /// </summary>
        internal static Set? CanonicalUnion(Set set)
        {
            var pieces = new List<(Entity Left, bool LeftClosed, Entity Right, bool RightClosed)>();
            bool Collect(Entity piece)
            {
                switch (piece)
                {
                    case Unionf(var left, var right):
                        return Collect(left) && Collect(right);
                    case Interval(var left, var leftClosed, var right, var rightClosed) when left.Evaled is Number.Real && right.Evaled is Number.Real:
                        var order = At(left).CompareTo(At(right));
                        if (order < 0 || order == 0 && leftClosed && rightClosed)
                            pieces.Add((left, leftClosed, right, rightClosed));
                        return true;
                    case FiniteSet listed when listed.All(static member => member.Evaled is Number.Real):
                        foreach (var member in listed)
                            pieces.Add((member, true, member, true));
                        return true;
                    default:
                        return false;
                }
            }
            if (!Collect(set))
                return null;
            // By the left end, a closed end before an open one at the same point.
            pieces.Sort(static (a, b) => At(a.Left).CompareTo(At(b.Left)) is var byLeft and not 0 ? byLeft : b.LeftClosed.CompareTo(a.LeftClosed));
            var merged = new List<(Entity Left, bool LeftClosed, Entity Right, bool RightClosed)>();
            foreach (var piece in pieces)
            {
                if (merged.Count == 0)
                {
                    merged.Add(piece);
                    continue;
                }
                var last = merged[^1];
                var gap = At(piece.Left).CompareTo(At(last.Right));
                if (gap > 0 || gap == 0 && !last.RightClosed && !piece.LeftClosed)
                {
                    merged.Add(piece);
                    continue;
                }
                var further = At(piece.Right).CompareTo(At(last.Right));
                merged[^1] = further > 0 ? (last.Left, last.LeftClosed, piece.Right, piece.RightClosed)
                    : further == 0 ? (last.Left, last.LeftClosed, last.Right, last.RightClosed || piece.RightClosed)
                    : last;
            }
            var points = merged.Where(static piece => At(piece.Left).CompareTo(At(piece.Right)) == 0).Select(static piece => piece.Left).ToList();
            var intervals = merged.Where(static piece => At(piece.Left).CompareTo(At(piece.Right)) != 0)
                .Select(static piece => (Set)new Interval(piece.Left, piece.LeftClosed, piece.Right, piece.RightClosed)).ToList();
            Set canonical = intervals.Count == 0 ? new FiniteSet(points)
                : (points.Count == 0 ? intervals : intervals.Prepend(new FiniteSet(points)))
                    .Aggregate(static (left, right) => new Unionf(left, right));
            return canonical == set ? null : canonical;
        }

        private static PeterO.Numbers.EDecimal At(Entity end) => ((Number.Real)end.Evaled).EDecimal;

        internal static Set UniteFiniteSetAndSet(FiniteSet finite, Set set)
        {
            if (set is FiniteSet another)
                return FiniteSet.Unite(finite, another);
            var sb = new FiniteSetBuilder();
            if (set is Interval(var left, var leftClosed, var right, var rightClosed) inter)
            {
                var newLeftClosed = leftClosed;
                var newRightClosed = rightClosed;
                foreach (var el in finite)
                    if (!set.TryContains(el, out var contains) || !contains)
                    {
                        if (el == left)
                            newLeftClosed = true;
                        else if (el == right)
                            newRightClosed = true;
                        else
                            sb.Add(el);
                    }
                set = inter.New(left, newLeftClosed, right, newRightClosed);
            }
            else
            {
                foreach (var el in finite)
                    if (!set.TryContains(el, out var contains) || !contains)
                        sb.Add(el);
            }
            return sb.IsEmpty ? set : sb.ToFiniteSet().Unite(set);
        }

        // TODO: it requires cleaning
        internal static Set UniteIntervalAndInterval(Interval A, Interval B)
        {
            if (A.Left == B.Right && (A.LeftClosed || B.RightClosed))
                return new Interval(B.Left, B.LeftClosed, A.Right, A.RightClosed);
            if (A.Right == B.Left && (A.RightClosed || B.LeftClosed))
                return new Interval(A.Left, A.LeftClosed, B.Right, B.RightClosed);
            if (A.Left is not Real aLeft ||
                A.Right is not Real aRight ||
                B.Left is not Real bLeft ||
                B.Right is not Real bRight)
                return A.Unite(B);
            if (aLeft == aRight && A.LeftClosed && A.RightClosed)
                UniteFiniteSetAndSet(new FiniteSet(aLeft), B);
            if (bLeft == bRight && B.LeftClosed && B.RightClosed)
                UniteFiniteSetAndSet(new FiniteSet(bLeft), A);
            if (aLeft >= aRight)
                return B;
            if (bLeft >= bRight)
                return A;
            if (aLeft == bRight && !A.LeftClosed && !B.RightClosed)
                return A.Unite(B);
            if (bLeft == aRight && !B.LeftClosed && !A.RightClosed)
                return A.Unite(B);
            if (aLeft > bRight)
                return A.Unite(B);
            if (bLeft > aRight)
                return A.Unite(B);
            if (aLeft < bLeft && bRight < aRight)
                return A;
            if (bLeft < aLeft && aRight < bRight)
                return B;
            var (left, leftClosed) = 
                aLeft == bLeft ? 
                (aLeft, A.LeftClosed || B.LeftClosed) : 
                (bLeft < aLeft ? (bLeft, B.LeftClosed) : (aLeft, A.LeftClosed));
            var (right, rightClosed) =
                aRight == bRight ?
                (aRight, A.RightClosed || B.RightClosed) :
                (bRight > aRight ? (bRight, B.RightClosed) : (aRight, A.RightClosed));
            return new Interval(left, leftClosed, right, rightClosed);
        }

        internal static Set UniteCSetAndCSet(ConditionalSet intLeft, ConditionalSet intRight)
        {
            (intLeft, intRight) = MergeToOneVariable(intLeft, intRight);
            return new ConditionalSet(intLeft.Var, (intLeft.Predicate | intRight.Predicate).InnerSimplified);
        }
    }
}
