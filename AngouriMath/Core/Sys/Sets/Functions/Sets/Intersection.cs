using System;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Core.Sets
{
    public static partial class SetFunctions
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
