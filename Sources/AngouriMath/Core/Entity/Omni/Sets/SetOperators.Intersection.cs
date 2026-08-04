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
            // Compared by what the endpoints are worth, not by whether they are written as
            // bare numbers. (sqrt(33) - 3) / 6 is a Divf and never a Real, so an interval
            // written with one was given up on -- which is
            // https://github.com/asc-community/AngouriMath/issues/415. The bounds of the result
            // are taken from the original expressions, so the answer keeps them exact
            // rather than turning them into a hundred decimal places.
            if (A.Left.Evaled is not Real aLeft ||
                A.Right.Evaled is not Real aRight ||
                B.Left.Evaled is not Real bLeft ||
                B.Right.Evaled is not Real bRight)
                return A.Intersect(B);
            if (aLeft == bRight)
                return A.LeftClosed && B.RightClosed ? new FiniteSet(A.Left) : Empty;
            if (bLeft == aRight)
                return A.RightClosed && B.LeftClosed ? new FiniteSet(B.Left) : Empty;
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
               (A.Left, A.LeftClosed && B.LeftClosed) :
               (bLeft < aLeft ? (A.Left, A.LeftClosed) : (B.Left, B.LeftClosed));
            var (right, rightClosed) =
                aRight == bRight ?
                (A.Right, A.RightClosed && B.RightClosed) :
                (bRight > aRight ? (A.Right, A.RightClosed) : (B.Right, B.RightClosed));
            return new Interval(left, leftClosed, right, rightClosed);
        }

        internal static Set IntersectCSetAndCSet(ConditionalSet intLeft, ConditionalSet intRight)
        {
            (intLeft, intRight) = MergeToOneVariable(intLeft, intRight);
            return new ConditionalSet(intLeft.Var, (intLeft.Predicate & intRight.Predicate).InnerSimplified);
        }
    }
}
