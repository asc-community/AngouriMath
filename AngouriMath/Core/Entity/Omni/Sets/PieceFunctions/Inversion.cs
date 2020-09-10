
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
        public static IEnumerable<SetPiece> Invert(SetPiece A)
        {
            /*
                 -∞+∞𝑖 ┈┈┈┈┈┈┈┈┈┈┈ +∞𝑖 ┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈ +∞+∞𝑖
                   ┊      pieceLeft ┃      ╭────────╮         pieceUp        ┊
                   ┊                ┃      │upClosed│                        ┊
                   ┊                ┣━━━━━━┷━━━━━━━━┷━━━━━━┳━━━━━━━━━━━━━━━ +∞
                   ┊     ╭──────────┨🡼leftUp     rightUp🡽┠───────────╮     ┊
                   ┊     │leftClosed┃                      ┃rightClosed│     ┊
                   ┊     ╰──────────┨🡿leftDown rightDown🡾┠───────────╯     ┊
                  −∞ ━━━━━━━━━━━━━━━┻━━━━━┯━━━━━━━━━━┯━━━━━┫                 ┊
                   ┊                      │downClosed│     ┃                 ┊
                   ┊      pieceDown       ╰──────────╯     ┃  pieceRight     ┊
                 -∞-∞𝑖 ┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈┈ −∞𝑖 ┈┈┈┈┈┈┈┈┈┈┈┈┈ +∞−∞𝑖

            If a side not included, we yield return an interval occupying the side.
            For each of 4 Pieces we close only one side or zero (so there's no intersection between Pieces)
              */
            var ((leftDown, leftClosed, downClosed), (rightUp, rightClosed, upClosed)) = SortEdges(A);
            var leftUp = Complex.Create(leftDown.RealPart, rightUp.ImaginaryPart);
            var rightDown = Complex.Create(rightUp.RealPart, leftDown.ImaginaryPart);
            yield return Interval.OrElement(rightDown, Complex.NegNegInfinity, true, false, false, false); // pieceDown
            yield return Interval.OrElement(leftDown, Complex.NegPosInfinity, false, true, false, false); // pieceLeft
            yield return Interval.OrElement(leftUp, Complex.PosPosInfinity, true, false, false, false); // pieceUp
            yield return Interval.OrElement(rightUp, Complex.PosNegInfinity, false, true, false, false); // pieceRight
            if (!downClosed) yield return Interval.OrElement(leftDown, rightDown);
            if (!leftClosed) yield return Interval.OrElement(leftDown, leftUp);
            if (!upClosed) yield return Interval.OrElement(leftUp, rightUp);
            if (!rightClosed) yield return Interval.OrElement(rightDown, rightUp);
        }
    }
}
