
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

using System.Collections.Generic;

namespace AngouriMath.Core.Sets
{
    static partial class SetFunctions
    {
        /// <summary>
        /// Intersects two nodes. If 
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static SetNode Intersect(SetNode A, SetNode B)
        {
            if (A.Type == SetNode.NodeType.OPERATOR || B.Type == SetNode.NodeType.OPERATOR)
                return new OperatorSet(OperatorSet.OperatorType.INTERSECTION, A, B);
            var (goodAPieces, badAPieces) = GatherEvaluablePieces(A as Set);
            var (goodBPieces, badBPieces) = GatherEvaluablePieces(B as Set);
            if (goodAPieces.Count * goodBPieces.Count == 0)
                return new OperatorSet(OperatorSet.OperatorType.INTERSECTION, A, B);
            var newPieces = new List<Piece>();
            foreach (var goodAPiece in goodAPieces)
                foreach (var goodBPiece in goodBPieces)
                {
                    var piece = PieceFunctions.Intersect(goodAPiece, goodBPiece);
                    if (!(piece is null) && PieceFunctions.IsPieceCorrect(piece))
                        newPieces.Add(piece);
                }
            var union = UniteList(newPieces);
            var badA = new Set{ Pieces = badAPieces };
            var badB = new Set{ Pieces = badBPieces };
            if (union.Count == 0)
                return new OperatorSet(OperatorSet.OperatorType.INTERSECTION, badA, badB);
            var united = new Set{ Pieces = union };
            if (badBPieces.Count + badAPieces.Count == 0)
                return united;
            /*
             * A = A1 or A2 (A1 - good, A2 - bad)
             * B = B1 or B2 (B1 - good, B2 - bad)
             * A & B = (A1 & B1) | (A1 & B2) | (A2 & B1) | (A2 & B2)
             */
            var goodA = new Set{ Pieces = goodAPieces };
            var goodB = new Set{ Pieces = goodBPieces };
            return OperatorSet.Or(
                united,                                      // A1 & B1
                OperatorSet.Or(
                    OperatorSet.And(                         // A2 & B2
                        badA,
                        badB
                    ),
                    OperatorSet.Or(
                        OperatorSet.And(badA, goodB),        // A2 & B1
                        OperatorSet.And(badB, goodA)     // A1 & B2
                        )));
        }

        internal static (List<Piece>, List<Piece>) GatherEvaluablePieces(Set A)
        {
            var goodPieces = new List<Piece>(); // Those we can eval, e. g. [3; 4]
            var badPieces = new List<Piece>();  // Those we cannot, e. g. [x + 3; -3]
            foreach (var piece in A)
                if (piece.IsNumeric())
                    goodPieces.Add(piece);
                else
                    badPieces.Add(piece);
            return (goodPieces, badPieces);
        }
    }
}
