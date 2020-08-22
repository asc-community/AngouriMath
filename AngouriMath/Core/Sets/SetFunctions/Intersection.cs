
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
using System.Linq;

namespace AngouriMath.Core
{
    partial record SetNode
    {
        partial record Intersection
        {
            public override SetNode Eval()
            {
                if (!(A is Set a && B is Set b))
                    return new Intersection(A, B);
                var (goodAPieces, badAPieces) = GatherEvaluablePieces(a);
                var (goodBPieces, badBPieces) = GatherEvaluablePieces(b);
                if (goodAPieces.Count * goodBPieces.Count == 0)
                    return new Intersection(A, B);
                var newPieces = goodAPieces
                    .SelectMany(_ => goodBPieces, (goodAPiece, goodBPiece) =>
                        PieceFunctions.Intersect(goodAPiece, goodBPiece) is { } piece
                        && PieceFunctions.IsPieceCorrect(piece)
                        ? piece : null);

                static IEnumerable<Piece>? UniteList(IEnumerable<Piece?> pieces)
                {
                    using var enumerator = pieces.GetEnumerator();
                    do if (!enumerator.MoveNext()) return null;
                    while (enumerator.Current is null);
                    IEnumerable<Piece> remainders = new[] { enumerator.Current };
                    while (enumerator.MoveNext())
                        if (enumerator.Current is { } current)
                            remainders = remainders.SelectMany(rem => PieceFunctions.Unite(rem, current));
                    return remainders;
                }
                var union = UniteList(newPieces);
                var badA = new Set { Pieces = badAPieces };
                var badB = new Set { Pieces = badBPieces };
                if (union is null)
                    return badA.IsEmpty() || badB.IsEmpty() ? new Set() : (SetNode)new Intersection(badA, badB);
                var united = new Set { Pieces = union.ToList() };
                if (badBPieces.Count + badAPieces.Count == 0)
                    return united;
                /*
                 * A = A1 or A2 (A1 - good, A2 - bad)
                 * B = B1 or B2 (B1 - good, B2 - bad)
                 * A & B = (A1 & B1) | (A1 & B2) | (A2 & B1) | (A2 & B2)
                 */
                var goodA = new Set { Pieces = goodAPieces };
                var goodB = new Set { Pieces = goodBPieces };
                return new Union(
                    united,                                // A1 & B1
                    new Union(                             
                        new Intersection(badA, badB),      // A2 & B2
                        new Union(
                            new Intersection(badA, goodB), // A2 & B1
                            new Intersection(badB, goodA)  // A1 & B2
                        )
                    )
                );
            }
        }
    }
}