
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining A copy of this software and associated documentation
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
using System.Numerics;
using System.Text;
using AngouriMath.Core.Numerix;

namespace AngouriMath.Core
{
    // First bool is whether the edge is closed for Re(Entity)
    // Second bool is whether the edge is closed for Im(Entity)
    using Edge = Tuple<Entity, bool, bool>;

    public abstract class Piece
    {
        public enum PieceType
        {
            ENTITY,
            INTERVAL,
        }

        public bool In(Set set)
            => set.Contains(this);

        public PieceType Type { get; }

        public static bool operator ==(Piece a, Piece b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            if (a.Type != b.Type)
                return false;
            if (a.Type == PieceType.ENTITY)
                return a as OneElementPiece == b as OneElementPiece;
            else
                return a as IntervalPiece == b as IntervalPiece;
        }

        public static bool operator !=(Piece a, Piece b)
            => !(a == b);

        /// <summary>
        /// See Edge definition above in this file
        /// </summary>
        /// <returns></returns>
        public abstract Edge UpperBound();

        /// <summary>
        /// See Edge definition above in this file
        /// </summary>
        /// <returns></returns>
        public abstract Edge LowerBound();

        /// <summary>
        /// True if num is in between A, B
        /// </summary>
        /// <param name="a">
        /// one bound
        /// </param>
        /// <param name="b">
        /// another bound (if A > B, they swap)
        /// </param>
        /// <param name="closedA">
        /// whether A inclusive
        /// </param>
        /// <param name="closedB">
        /// whether B inclusive
        /// </param>
        /// <param name="closedNum">
        /// if false, then
        /// (2 is in (2; 3)
        /// </param>
        /// <param name="num"></param>
        /// <returns></returns>
        private static bool InBetween(RealNumber a, RealNumber b, bool closedA, bool closedB, RealNumber num,
            bool closedNum)
        {
            if (num == a && (closedA || !closedNum))
                return true;
            if (num == b && (closedB || !closedNum))
                return true;
            if (a > b)
                Const.Swap(ref a, ref b);
            return num > a && num < b;
        }

        /// <summary>
        /// Performs InBetween on both Re and Im parts of the number
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="closedARe"></param>
        /// <param name="closedAIm"></param>
        /// <param name="closedBRe"></param>
        /// <param name="closedBIm"></param>
        /// <param name="num"></param>
        /// <param name="closedRe"></param>
        /// <param name="closedIm"></param>
        /// <returns></returns>
        private static bool ComplexInBetween(ComplexNumber a, ComplexNumber b, bool closedARe, bool closedAIm,
            bool closedBRe,
            bool closedBIm, ComplexNumber num, bool closedRe, bool closedIm)
            => InBetween(a.Real, b.Real, closedARe, closedBRe, num.Real, closedRe) &&
               InBetween(a.Imaginary, b.Imaginary, closedAIm, closedBIm, num.Imaginary, closedIm);

        /// <summary>
        /// So that any numberical operations could be performed
        /// </summary>
        /// <returns></returns>
        internal bool IsNumeric()
            => (MathS.CanBeEvaluated(LowerBound().Item1) && MathS.CanBeEvaluated(UpperBound().Item1));

        /// <summary>
        /// Determines whether interval or element of piece is in this
        /// </summary>
        /// <param name="piece"></param>
        /// <returns></returns>
        public bool Contains(Piece piece)
        {
            if (!piece.IsNumeric() || !this.IsNumeric())
                return false;

            // Gather all information
            var up = UpperBound();
            var low = LowerBound();
            var up_l = piece.LowerBound();
            var low_l = piece.UpperBound();
            // If still running the function, all entities are numbers now

            // Evaluate them
            var num1 = up.Item1.Eval();
            var num2 = low.Item1.Eval();
            var num_up = up_l.Item1.Eval();
            var num_low = low_l.Item1.Eval();
            // // //

            return ComplexInBetween(num1, num2, up.Item2, up.Item3,
                       low.Item2, low.Item3, num_low, low_l.Item2,
                       low_l.Item3) &&
                   ComplexInBetween(num1, num2, up.Item2, up.Item3,
                       low.Item2, low.Item3, num_up, up_l.Item2,
                       up_l.Item3);
        }

        protected Piece(PieceType type)
        {
            Type = type;
        }

        internal static Edge CopyEdge(Edge edge)
            => new Edge(edge.Item1.DeepCopy(), edge.Item2, edge.Item3);

        internal static bool EdgeEqual(Edge A, Edge B)
            => Const.EvalIfCan(A.Item1) == Const.EvalIfCan(B.Item1) && A.Item2 == B.Item2 && A.Item3 == B.Item3;

            internal static Piece Interval(Entity a, Entity b, bool closedARe, bool closedAIm, bool closedBRe, bool closedBIm)
        {
            if (a == b)
                return new OneElementPiece(a);
            var interval = new IntervalPiece(a, b, closedARe, closedAIm, closedBRe, closedBIm);
            return interval;
        }

        /// <summary>
        /// Creates an instance of A closed interval (use SetNode-functions to change it,
        /// see more in MathS.Sets.Interval() )
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static Piece Interval(Entity a, Entity b)
            => Interval(a, b, true, true, true, true);

        internal static Piece Interval(Edge A, Edge B)
            => Interval(A.Item1, B.Item1, A.Item2, A.Item3, B.Item2, B.Item3);

        /// <summary>
        /// Creates an instance of an element
        /// See more in MathS.Sets.Element()
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        internal static OneElementPiece Element(Entity a)
        => new OneElementPiece(a);

        internal static IntervalPiece CreateUniverse()
            => Piece.Interval(ComplexNumber.NegNegInfinity(),
                ComplexNumber.PosPosInfinity(),
                false, false, false, false).AsInterval();

        internal IntervalPiece AsInterval() => this as IntervalPiece;

        public static implicit operator Piece((Entity left, Entity right) tup)
            => Interval(tup.left, tup.right);
        public static implicit operator Piece((Entity left, Entity right, bool leftClosed, bool rightClosed) tup)
            => Interval(tup.left, tup.right).AsInterval().SetLeftClosed(tup.leftClosed).SetRightClosed(tup.rightClosed);
        public static implicit operator Piece((Entity left, Entity right, bool leftReClosed, bool leftImClosed, bool rightReClosed, bool rightImClosed) tup)
            => Interval(tup.left, tup.right).AsInterval().SetLeftClosed(tup.leftReClosed, tup.leftImClosed).SetRightClosed(tup.rightReClosed, tup.rightImClosed);
        public static implicit operator Piece(Entity element)
            => new OneElementPiece(element);
        public static implicit operator Piece(float element)
            => new OneElementPiece(element);
        public static implicit operator Piece(int element)
            => new OneElementPiece(element);
        public static implicit operator Piece(ComplexNumber element)
            => new OneElementPiece(element);
        public static implicit operator Piece(Complex element)
            => new OneElementPiece(Number.Create(element));
        public static explicit operator Entity(Piece piece)
            => (piece as OneElementPiece).entity.Item1;
    }

    public class OneElementPiece : Piece
    {
        internal Edge entity;

        internal OneElementPiece(Entity element) : base(PieceType.ENTITY)
        {
            entity = new Edge(element, true, true);
        }

        public override Edge UpperBound()
            => CopyEdge(entity);

        public override Edge LowerBound()
            => CopyEdge(entity);

        public override string ToString()
            => "{" + entity.Item1.ToString() + "}";

        public static bool operator ==(OneElementPiece A, OneElementPiece B)
            => EdgeEqual(A.entity, B.entity);

        public static bool operator !=(OneElementPiece A, OneElementPiece B)
            => !(A == B);
    }

    public class IntervalPiece : Piece
    {
        private Edge leftEdge;
        private Edge rightEdge;

        internal IntervalPiece(Entity left, Entity right, bool closedARe, bool closedAIm, bool closedBRe, bool closedBIm) : base(PieceType.INTERVAL)
        {
            leftEdge = new Edge(left, closedARe, closedAIm);
            rightEdge = new Edge(right, closedBRe, closedBIm);
        }

        internal IntervalPiece(Edge left, Edge right) : base(PieceType.INTERVAL)
        {
            leftEdge = left;
            rightEdge = right;
        }

        public override Edge LowerBound()
            => CopyEdge(leftEdge);

        public override Edge UpperBound()
            => CopyEdge(rightEdge);

        /// <summary>
        /// Used for real intervals only.
        /// true: [
        /// false: (
        /// </summary>
        /// <param name="isClosed"></param>
        /// <returns></returns>
        public IntervalPiece SetLeftClosed(bool isClosed)
            => SetLeftClosed(isClosed, true);

        /// <summary>
        /// Used for real intervals only.
        /// true: ]
        /// false: )
        /// </summary>
        /// <param name="isClosed"></param>
        /// <returns></returns>
        public IntervalPiece SetRightClosed(bool isClosed)
            => SetRightClosed(isClosed, true);

        /// <summary>
        /// Used for any type of interval
        /// sets [ or ( for real and [ or ( for imaginary part of the number
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        /// <returns></returns>
        public IntervalPiece SetLeftClosed(bool Re, bool Im)
        {
            leftEdge = new Edge(leftEdge.Item1, Re, Im);
            return this;
        }

        /// <summary>
        /// Used for any type of interval
        /// sets ] or ) for real and ] or ) for imaginary part of the number
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        /// <returns></returns>
        public IntervalPiece SetRightClosed(bool Re, bool Im)
        {
            rightEdge = new Edge(rightEdge.Item1, Re, Im);
            return this;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (leftEdge.Item3)
                sb.Append("[");
            else
                sb.Append("(");
            if (leftEdge.Item2)
                sb.Append("[");
            else
                sb.Append("(");
            sb.Append(leftEdge.Item1.ToString());
            sb.Append("; ");
            sb.Append(rightEdge.Item1.ToString());
            if (rightEdge.Item2)
                sb.Append("]");
            else
                sb.Append(")");
            if (rightEdge.Item3)
                sb.Append("]");
            else
                sb.Append(")");
            return sb.ToString();
        }

        public static bool operator ==(IntervalPiece A, IntervalPiece B)
            => EdgeEqual(A.leftEdge, B.leftEdge) && EdgeEqual(A.rightEdge, B.rightEdge);

        public static bool operator !=(IntervalPiece A, IntervalPiece B)
            => !(A == B);
    }
}
