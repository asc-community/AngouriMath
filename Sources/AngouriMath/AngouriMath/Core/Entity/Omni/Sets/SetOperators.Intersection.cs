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
        internal static Set IntersectFiniteSetAndSet(FiniteSet finite, Set set)
        {
            if (set is FiniteSet another)
                return FiniteSet.Intersect(finite, another);
            var fsb = new FiniteSetBuilder();
            var amb = new FiniteSetBuilder();
            foreach (var elem in finite)
            {
                if (!set.TryContains(elem, out var contains))
                    amb.Add(elem);
                else if (contains)
                    fsb.Add(elem);
            }
            return amb.IsEmpty ? fsb.ToFiniteSet() : amb.ToFiniteSet().Unite(fsb.ToFiniteSet());
        }

        internal static Set IntersectIntervalAndInterval(Interval A, Interval B)
        {
            if (A.Left == B.Left && A.Right == B.Right)
                return new Interval(A.Left, A.LeftClosed && B.LeftClosed, A.Right, A.RightClosed && B.RightClosed);
            if (A.Left is not Real aLeft ||
                A.Right is not Real aRight ||
                B.Left is not Real bLeft ||
                B.Right is not Real bRight)
                return A.Intersect(B);
            if (aLeft == bRight)
                return A.LeftClosed && B.RightClosed ? new FiniteSet(aLeft) : Empty;
            if (bLeft == aRight)
                return A.RightClosed && B.LeftClosed ? new FiniteSet(bLeft) : Empty;
            if (aLeft >= aRight)
                return B;
            if (bLeft >= bRight)
                return A;
            if (aLeft > bRight)
                return Empty;
            if (bLeft > aRight)
                return Empty;
            var (left, leftClosed) =
               aLeft == bLeft ?
               (aLeft, A.LeftClosed && B.LeftClosed) :
               (bLeft < aLeft ? (aLeft, A.LeftClosed) : (bLeft, B.LeftClosed));
            var (right, rightClosed) =
                aRight == bRight ?
                (aRight, A.RightClosed && B.RightClosed) :
                (bRight > aRight ? (aRight, A.RightClosed) : (bRight, B.RightClosed));
            return new Interval(left, leftClosed, right, rightClosed);
        }

        internal static Set IntersectCSetAndCSet(ConditionalSet intLeft, ConditionalSet intRight)
        {
            (intLeft, intRight) = MergeToOneVariable(intLeft, intRight);
            return new ConditionalSet(intLeft.Var, (intLeft.Predicate & intRight.Predicate).InnerSimplified);
        }
    }
}
