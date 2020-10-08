
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
        internal static Set SetSubtractSetAndFiniteSet(Set set, FiniteSet finite)
        {
            var fsb = new FiniteSetBuilder(finite);
            foreach (var el in finite)
                if (set.TryContains(el, out var contains) && !contains)
                    fsb.Remove(el);
            return fsb.IsEmpty ? set : set.SetSubtract(fsb.ToFiniteSet());
        }

        private static bool Implies(this bool assumption, bool conclusion)
            => !assumption || conclusion;

        internal static Set SetSubtractIntervalAndInterval(Interval A, Interval B)
        {
            if (A == B)
                return Empty;
            if (A.Left == B.Left && A.Right == B.Right)
                return new Interval(A.Left, A.LeftClosed && !B.LeftClosed, A.Right, A.RightClosed && !B.RightClosed);
            if (A.Left is not Real aLeft ||
                A.Right is not Real aRight ||
                B.Left is not Real bLeft ||
                B.Right is not Real bRight)
                return A.SetSubtract(B);
             if (aLeft > aRight)
                return Empty;
             if (bLeft > bRight)
                return A;
             if (aLeft == bRight)
                return B.RightClosed ? A.New(A.Left, false, A.Right, A.RightClosed) : A;
            if (aRight == bLeft)
                return B.LeftClosed ? A.New(A.Left, A.LeftClosed, A.Right, false) : A;
            if (aRight < bLeft || aLeft > bRight)
                return A;
            if (
                (bLeft < aLeft || bLeft == aLeft && A.LeftClosed.Implies(B.LeftClosed))
                &&
                (bRight > aLeft || bRight == aRight && A.RightClosed.Implies(B.RightClosed))
                )
                return Empty;

        }
    }
}
