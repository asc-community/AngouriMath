
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

/*
 *
 * INVERSTION
 *
 */

namespace AngouriMath.Core.Sets
{
    public static partial class PieceFunctions
    {
        public static List<Piece> Invert(Piece A)
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
             */
           /*
            *
            * for each of 4 pieces we close only one side or zero (so there's no intersection between pieces)
            *
            */
           var numLeftDown = new Number(edgeLowerNum.Re, edgeLowerNum.Im);
           var numLeftUp = new Number(edgeLowerNum.Re, edgeUpperNum.Im);
           var numRightUp = new Number(edgeUpperNum.Re, edgeUpperNum.Im);
           var numRightDown = new Number(edgeUpperNum.Re, edgeLowerNum.Im);
           var pieceDown = new IntervalPiece(
               numRightDown, new Number(double.NegativeInfinity, double.NegativeInfinity), 
               edgeLower.Item2, false, false, false);
           var pieceLeft = new IntervalPiece(
               numLeftDown, new Number(double.NegativeInfinity, double.PositiveInfinity),
               false, edgeLower.Item3, false, false);
           var pieceUp = new IntervalPiece(
               numLeftUp, new Number(double.PositiveInfinity, double.PositiveInfinity),
               edgeUpper.Item2 && (numRightDown.Im != numLeftUp.Im), false, false, false);
           var pieceRight = new IntervalPiece(
               numRightUp, new Number(double.PositiveInfinity, double.NegativeInfinity),
               false, edgeUpper.Item3 && (numLeftDown.Re != numRightUp.Re), false, false);
           var res = new List<Piece> { pieceDown, pieceLeft, pieceUp, pieceRight };

           bool IsPieceCorrect(Piece piece)
           {
               var lower = piece.LowerBound();
               var upper = piece.UpperBound();
               var num1 = lower.Item1.Eval();
               var num2 = upper.Item1.Eval();
               return (num1.Re != num2.Re || lower.Item2 && upper.Item2) &&
                      (num1.Im != num2.Im || lower.Item3 && upper.Item3);
           }

           return res.Where(IsPieceCorrect).ToList();
        }
    }
}
