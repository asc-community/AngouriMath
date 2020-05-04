using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sets
{
    public static partial class SetFunctions
    {
        internal static SetNode Subtract(SetNode A, SetNode B)
        {
            if (A.Type == SetNode.NodeType.OPERATOR || B.Type == SetNode.NodeType.OPERATOR)
                return A - B;
            var (goodAPieces, badAPieces) = GatherEvaluablePieces(A as Set);
            var (goodBPieces, badBPieces) = GatherEvaluablePieces(B as Set);
            var newGoodPieces = new List<Piece>();
            newGoodPieces.AddRange(goodAPieces);
            foreach (var goodB in goodBPieces)
            {
                var newNewGoodPieces = new List<Piece>();
                foreach (var newGoodPiece in newGoodPieces)
                    newNewGoodPieces.AddRange(PieceFunctions.Subtract(newGoodPiece, goodB));
                newGoodPieces = newNewGoodPieces;
            }

            newGoodPieces.AddRange(badAPieces);
            var newSet = new Set{ Pieces = newGoodPieces };
            if (badBPieces.Count == 0)
                return newSet;
            else
                return new OperatorSet(OperatorSet.OperatorType.COMPLEMENT, newSet, new Set { Pieces = badBPieces });
        }
    }
}
