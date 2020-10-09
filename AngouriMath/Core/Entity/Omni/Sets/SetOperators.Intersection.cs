
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Number;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core.Sets
{
    internal static partial class SetOperators
    {
        internal static Set IntersectFiniteSetAndSet(FiniteSet finite, Set set)
        {
            if (set is FiniteSet another)
                return FiniteSet.Intersect(finite, another);
            var fsb = new FiniteSetBuilder(finite);
            foreach (var elem in finite)
                if (set.TryContains(elem, out var contains) && !contains)
                    fsb.Remove(elem);
            return fsb.IsEmpty ? Empty : fsb.ToFiniteSet().Intersect(set);
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
