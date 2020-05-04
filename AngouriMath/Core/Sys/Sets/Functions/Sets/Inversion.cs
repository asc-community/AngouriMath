using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sets
{
    public static partial class SetFunctions
    {
        public static SetNode Invert(SetNode A)
        {
            if (A.Type == SetNode.NodeType.OPERATOR)
                return new OperatorSet(OperatorSet.OperatorType.INVERSION, A);
            var (goodAPieces, badAPieces) = GatherEvaluablePieces(A as Set);
            var remainders = new List<Piece>{ Piece.CreateUniverse() };
            foreach (var good in goodAPieces)
            {
                var newRemainders = new List<Piece>();
                foreach (var rem in remainders)
                    newRemainders.AddRange(PieceFunctions.Subtract(rem, good));
                remainders = newRemainders;
            }

            var newSet = new Set{ Pieces = remainders };
            if (badAPieces.Count == 0)
                return newSet;
            else
                return new OperatorSet(OperatorSet.OperatorType.COMPLEMENT, newSet, new Set {Pieces = badAPieces});
        }
    }
}
