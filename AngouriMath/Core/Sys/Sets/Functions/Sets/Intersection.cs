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
                return A & B;
            var (goodAPieces, badAPieces) = GatherEvaluablePieces(A as Set);
            var (goodBPieces, badBPieces) = GatherEvaluablePieces(B as Set);
            if (goodAPieces.Count * goodBPieces.Count == 0)
                return A & B;
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
                return badA & badB;
            var united = new Set{ Pieces = union };
            return united & A & B;
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
