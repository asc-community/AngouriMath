
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

/*
 *
 * SUBTRACTION
 *
 */

namespace AngouriMath.Core
{
    static partial class PieceFunctions
    {
        /// <summary>Subtracts B from A: A \ B = A & !B</summary>
        public static IEnumerable<Piece> Subtract(Piece A, Piece B)
        {
            if (Intersect(A, B) == null) // if A & B is none, then A \ B = A
                return new[] { A };

            if (A == B || B.Contains(A))
                return Enumerable.Empty<Piece>();

            return Invert(B).SelectMany(piece =>
                Intersect(A, piece) is { } conj && IsPieceCorrect(conj)
                ? new[] { conj }
                : Enumerable.Empty<Piece>());
        }
        internal static bool IsPieceCorrect(Piece piece)
        {
            var lower = piece.LowerBound();
            var upper = piece.UpperBound();
            var num1 = lower.Item1.Eval();
            var num2 = upper.Item1.Eval();
            return (num1.RealPart != num2.RealPart || lower.Item2 && upper.Item2) &&
                   (num1.ImaginaryPart != num2.ImaginaryPart || lower.Item3 && upper.Item3);
        }
    }
}