
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
 * INVERSTION
 *
 */

namespace AngouriMath.Core
{
    using System.Collections.Generic;
    using static Entity.Number;
    static partial class PieceFunctions
    {
        public static IEnumerable<Piece> Invert(Piece A)
        {
            var sortedEdges = SortEdges(A.LowerBound(), A.UpperBound());
            var edgeLower = sortedEdges.Item1;
            var edgeUpper = sortedEdges.Item2;
            var edgeLowerNum = edgeLower.Item1.Eval();
            var edgeUpperNum = edgeUpper.Item1.Eval();
            /*

                       +oo Im
                        |
                        |       (upper.Item2)
                        |_______________________________ +oo Re
                        |leftUp      rightUp  |
        (lower.Item3)   |                     |  (upper.Item3)
                        |leftDown    rightDown|
       -oo Re  -------------------------------|
                             (lower.Item2)    |
                                              |
                                             -oo Im

             if a side not included, we add an interval instead

              */
            /*
             *
             * for each of 4 Pieces we close only one side or zero (so there's no intersection between Pieces)
             *
             */
            var numLeftDown = Complex.Create(edgeLowerNum.RealPart, edgeLowerNum.ImaginaryPart);
            var numLeftUp = Complex.Create(edgeLowerNum.RealPart, edgeUpperNum.ImaginaryPart);
            var numRightUp = Complex.Create(edgeUpperNum.RealPart, edgeUpperNum.ImaginaryPart);
            var numRightDown = Complex.Create(edgeUpperNum.RealPart, edgeLowerNum.ImaginaryPart);
            yield return Piece.ElementOrInterval(
                numRightDown, Complex.NegNegInfinity,
                true, false, false, false); // pieceDown
            yield return Piece.ElementOrInterval(
               numLeftDown, Complex.NegPosInfinity,
               false, true, false, false); // pieceLeft
            yield return Piece.ElementOrInterval(
               numLeftUp, Complex.PosPosInfinity,
               true, false, false, false); // pieceUp
            yield return Piece.ElementOrInterval(
               numRightUp, Complex.PosNegInfinity,
               false, true, false, false); // pieceRight

            if (!edgeLower.Item2)
                yield return Piece.ElementOrInterval(
                   Complex.Create(numLeftDown.RealPart, numLeftDown.ImaginaryPart),
                   Complex.Create(numLeftDown.RealPart, numLeftUp.ImaginaryPart), // Re part is the same
                   true, true, true, true
                );

            if (!edgeLower.Item3)
                yield return Piece.ElementOrInterval(
                   Complex.Create(numLeftUp.RealPart, numLeftDown.ImaginaryPart),
                   Complex.Create(numRightUp.RealPart, numLeftDown.ImaginaryPart), // Im part is the same
                   true, true, true, true
                );

            if (!edgeUpper.Item2)
                yield return Piece.ElementOrInterval(
                   Complex.Create(numRightUp.RealPart, numRightDown.ImaginaryPart),
                   Complex.Create(numRightUp.RealPart, numRightUp.ImaginaryPart), // Re part is the same
                   true, true, true, true
                );

            if (!edgeLower.Item3)
                yield return Piece.ElementOrInterval(
                   Complex.Create(numLeftDown.RealPart, numRightDown.ImaginaryPart),
                   Complex.Create(numRightDown.RealPart, numRightDown.ImaginaryPart), // Im part is the same
                   true, true, true, true
                );
        }
    }
}
