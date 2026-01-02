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
