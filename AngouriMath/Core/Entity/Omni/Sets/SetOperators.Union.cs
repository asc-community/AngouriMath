
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
        internal static Set UniteFiniteSetAndSet(FiniteSet finite, Set set)
        {
            if (set is FiniteSet another)
                return FiniteSet.Unite(finite, another);
            var sb = new FiniteSetBuilder();
            foreach (var el in finite)
                if (!set.TryContains(el, out var contains) || !contains)
                    sb.Add(el);
            return sb.IsEmpty ? set : sb.ToFiniteSet().Unite(set);
        }

        // TODO: it requires cleaning
        internal static Set UniteIntervalAndInterval(Interval A, Interval B)
        {
            if (A.Left == B.Right && (A.LeftClosed || B.RightClosed))
                return new Interval(B.Left, B.LeftClosed, A.Right, B.RightClosed);
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
