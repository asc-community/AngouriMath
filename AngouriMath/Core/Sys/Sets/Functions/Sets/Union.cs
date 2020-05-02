using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Sets
{
    public static partial class SetFunctions
    {
        public static SetNode Unite(SetNode A, SetNode B)
        {
            if (A.Type == SetNode.NodeType.OPERATOR || B.Type == SetNode.NodeType.OPERATOR)
                return A - B;
            var newSet = new Set();
            foreach (var piece in A as Set)
                newSet.AddPiece(piece);
            foreach (var piece in B as Set)
                newSet.AddPiece(piece);
            return newSet;
        }

        internal static List<Piece> UniteList(List<Piece> pieces)
        {
            if (pieces.Count == 0)
                return new List<Piece>();
            var remainders = new List<Piece> { pieces[0] };
            for (int i = 1; i < pieces.Count; i++)
            {
                var newRemainders = new List<Piece>();
                foreach (var rem in remainders)
                    newRemainders.AddRange(PieceFunctions.Unite(rem, pieces[i]));
                remainders = newRemainders;
            }

            return remainders;
        }
    }
}
