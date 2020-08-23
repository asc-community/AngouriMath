
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

/*
 *
 * INTERSECTION
 *
 */
using System;
namespace AngouriMath.Core
{
    using static Entity.Number;
    using Edge1D = ValueTuple<Entity.Number.Real, bool>;
    using Edge = ValueTuple<Entity, bool, bool>;
    static partial class PieceFunctions
    {
        internal static Edge1D IntersectEdge(
            Real num1, bool closed1,
            Real num2, bool closed2,
            Func<Real, Real, bool> comparator
            ) =>
            num1 == num2
            ? (num1, closed1 && closed2) // if at least one is open, result is open
            : comparator(num1, num2)
            ? (num1, closed1)
            : (num2, closed2);

        internal static (Edge1D, Edge1D) IntersectAxis(
            Real min1, bool closedMin1,
            Real max1, bool closedMax1,
            Real min2, bool closedMin2,
            Real max2, bool closedMax2)
            => (IntersectEdge(min1, closedMin1, min2, closedMin2, (min1, min2) => min1 > min2),
                IntersectEdge(max1, closedMax1, max2, closedMax2, (max1, max2) => max1 < max2));

        private static ((Complex, bool, bool) lowerEdge, (Complex, bool, bool) upperEdge) SortEdges(SetPiece piece)
        {
            var A = piece.LowerBound();
            var B = piece.UpperBound();
            var num1 = A.Item1.Eval();
            var num2 = B.Item1.Eval();
            var lowRe = num1.RealPart;
            var upRe = num2.RealPart;
            var lowIm = num1.ImaginaryPart;
            var upIm = num2.ImaginaryPart;
            var lowReClosed = A.Item2;
            var upReClosed = B.Item2;
            var lowImClosed = A.Item3;
            var upImClosed = B.Item3;
            if (lowRe > upRe)
            {
                (lowRe, upRe) = (upRe, lowRe);
                (lowReClosed, upReClosed) = (upReClosed, lowReClosed);
            }

            if (lowIm > upIm)
            {
                (lowIm, upIm) = (upIm, lowIm);
                (lowImClosed, upImClosed) = (upImClosed, lowImClosed);
            }

            return ((Complex.Create(lowRe, lowIm), lowReClosed, lowImClosed),
                    (Complex.Create(upRe, upIm), upReClosed, upImClosed));
        }

        /// <summary>Unsafe!</summary>
        public static SetPiece? Intersect(SetPiece A, SetPiece B)
        {
            if (A == B)
                return A;
            var ((low1Num, low1ReClosed, low1ImClosed), (up1Num, up1ReClosed, up1ImClosed)) = SortEdges(A);
            var ((low2Num, low2ReClosed, low2ImClosed), (up2Num, up2ReClosed, up2ImClosed)) = SortEdges(B);
            if (low1Num.RealPart > up2Num.RealPart || (low1Num.RealPart == up2Num.RealPart && (!low1ReClosed || !up2ReClosed)) ||
                low2Num.RealPart > up1Num.RealPart || (low2Num.RealPart == up1Num.RealPart && (!low2ReClosed || !up1ReClosed)) ||
                low1Num.ImaginaryPart > up2Num.ImaginaryPart || (low1Num.ImaginaryPart == up2Num.ImaginaryPart && (!low1ImClosed || !up2ImClosed)) ||
                low2Num.ImaginaryPart > up1Num.ImaginaryPart || (low2Num.ImaginaryPart == up1Num.ImaginaryPart && (!low2ImClosed || !up1ImClosed)))
                return null; // if they don't intersect
            var realAxis = IntersectAxis(
                low1Num.RealPart, low1ReClosed,
                up1Num.RealPart, up1ReClosed,
                low2Num.RealPart, low2ReClosed,
                up2Num.RealPart, up2ReClosed
            );
            var imaginaryAxis = IntersectAxis(
                low1Num.ImaginaryPart, low1ImClosed,
                up1Num.ImaginaryPart, up1ImClosed,
                low2Num.ImaginaryPart, low2ImClosed,
                up2Num.ImaginaryPart, up2ImClosed
            );
            return Interval.OrElement(
                Complex.Create(realAxis.Item1.Item1, imaginaryAxis.Item1.Item1),
                Complex.Create(realAxis.Item2.Item1, imaginaryAxis.Item2.Item1),
                realAxis.Item1.Item2, imaginaryAxis.Item1.Item2,
                realAxis.Item2.Item2, imaginaryAxis.Item2.Item2);
        }
    }
}