
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
using System.Text;

namespace AngouriMath
{
    using static Entity.Number;
    // First bool is whether the edge is closed for Re(Entity)
    // Second bool is whether the edge is closed for Im(Entity)
    using Edge = ValueTuple<Entity, bool, bool>;

    partial record Entity
    {

        public abstract class SetPiece
        {
            public abstract override int GetHashCode();
            public abstract override bool Equals(object obj);
            public static bool operator ==(SetPiece? a, SetPiece? b) =>
                (a, b) switch
                {
                    (null, null) => true,
                    (null, _) => false,
                    (_, null) => false,
                    (OneElementPiece aa, OneElementPiece bb) => aa == bb,
                    (Interval aa, Interval bb) => aa == bb,
                    _ => false
                };
            public static bool operator !=(SetPiece a, SetPiece b) => !(a == b);

            /// <summary>See Edge definition above in this file</summary>
            public abstract Edge UpperBound();

            /// <summary>See Edge definition above in this file</summary>
            public abstract Edge LowerBound();

            /// <summary>So that any numerical operations could be performed</summary>
            internal bool Evaluable
                => LowerBound().Item1.EvaluableNumerical && UpperBound().Item1.EvaluableNumerical;

            /// <summary>Determines whether interval or element of piece is in this</summary>
            public bool Contains(SetPiece piece)
            {
                /// <summary><see langword="true"/> if num is in between A, B</summary>
                /// <param name="a">one bound</param>
                /// <param name="b">another bound (if A > B, they swap)</param>
                /// <param name="closedA">whether A inclusive</param>
                /// <param name="closedB">whether B inclusive</param>
                /// <param name="closedNum">if false, then (2 is in (2; 3)</param>
                static bool InBetween(Real a, Real b, bool closedA, bool closedB, Real num, bool closedNum)
                {
                    if (num == a && (closedA || !closedNum))
                        return true;
                    if (num == b && (closedB || !closedNum))
                        return true;
                    if (a > b)
                        (a, b) = (b, a);
                    return num > a && num < b;
                }
                /// <summary>Performs InBetween on both Re and Im parts of the number</summary>
                static bool ComplexInBetween(Complex a, Complex b, bool closedARe, bool closedAIm,
                    bool closedBRe,
                    bool closedBIm, Complex num, bool closedRe, bool closedIm)
                    => InBetween(a.RealPart, b.RealPart, closedARe, closedBRe, num.RealPart, closedRe) &&
                       InBetween(a.ImaginaryPart, b.ImaginaryPart, closedAIm, closedBIm, num.ImaginaryPart, closedIm);

                // Gather all information
                var up = UpperBound();
                var low = LowerBound();
                var up_l = piece.LowerBound();
                var low_l = piece.UpperBound();

                if (!(up.Item1.Evaled is Complex num1 && low.Item1.Evaled is Complex num2
                    && up_l.Item1.Evaled is Complex num_up && low_l.Item1.Evaled is Complex num_low))
                    return false;
                return ComplexInBetween(num1, num2, up.Item2, up.Item3,
                           low.Item2, low.Item3, num_low, low_l.Item2,
                           low_l.Item3) &&
                       ComplexInBetween(num1, num2, up.Item2, up.Item3,
                           low.Item2, low.Item3, num_up, up_l.Item2,
                           up_l.Item3);
            }

            internal static bool EdgeEqual(Edge A, Edge B)
                => A.Item1.Evaled == B.Item1.Evaled && A.Item2 == B.Item2 && A.Item3 == B.Item3;

            /// <summary>Creates an instance of an element. See more in <see cref="MathS.Sets.Element(Entity)"/>.</summary>
            internal static OneElementPiece Element(Entity a) => new OneElementPiece(a);
            public static implicit operator SetPiece((Entity left, Entity right) tup)
                => Interval.OrElement(tup.left, tup.right);
            public static implicit operator SetPiece((Entity left, Entity right, bool leftClosed, bool rightClosed) tup)
                => new Interval(tup.left, tup.right).SetLeftClosed(tup.leftClosed).SetRightClosed(tup.rightClosed);
            public static implicit operator SetPiece((Entity left, Entity right, bool leftReClosed, bool leftImClosed, bool rightReClosed, bool rightImClosed) tup)
                => new Interval(tup.left, tup.right).SetLeftClosed(tup.leftReClosed, tup.leftImClosed).SetRightClosed(tup.rightReClosed, tup.rightImClosed);
            public static implicit operator SetPiece(Entity element)
                => new OneElementPiece(element);
            public static implicit operator SetPiece(float element)
                => new OneElementPiece(element);
            public static implicit operator SetPiece(double element)
                => new OneElementPiece(element);
            public static implicit operator SetPiece(int element)
                => new OneElementPiece(element);
            public static implicit operator SetPiece(Complex element)
                => new OneElementPiece(element);
            public static implicit operator SetPiece(System.Numerics.Complex element)
                => new OneElementPiece((Complex)element);
            public static explicit operator Entity(SetPiece piece)
                => ((OneElementPiece)piece).entity;
        }

        public class OneElementPiece : SetPiece
        {
            internal Entity entity;
            internal OneElementPiece(Entity element) => entity = element;
            public override Edge UpperBound() => (entity, true, true);
            public override Edge LowerBound() => (entity, true, true);
            public override string ToString() => $"{{{entity}}}";
            public static bool operator ==(OneElementPiece A, OneElementPiece B) => A.entity.Evaled == B.entity.Evaled;
            public static bool operator !=(OneElementPiece A, OneElementPiece B) => !(A == B);
            public override bool Equals(object obj) => obj is OneElementPiece p && entity.Equals(p.entity);
            public override int GetHashCode() => entity.GetHashCode();
        }

        public class Interval : SetPiece
        {
            private Edge leftEdge;
            private Edge rightEdge;
            internal static Interval CreateUniverse() => new(Complex.NegNegInfinity, Complex.PosPosInfinity, false, false, false, false);
            /// <summary>
            /// Creates an instance of A closed interval (use SetNode-functions to change it,
            /// see more in <see cref="MathS.Sets.Interval(Entity, Entity)"/>)
            /// </summary>
            internal static SetPiece OrElement(Entity left, Entity right) =>
                left == right ? new OneElementPiece(left) : (SetPiece)new Interval(left, right);
            internal static SetPiece OrElement(Entity left, Entity right, bool leftReClosed, bool rightReClosed) =>
                left == right ? new OneElementPiece(left) : (SetPiece)new Interval(left, right, leftReClosed, rightReClosed);
            internal static SetPiece OrElement(Entity left, Entity right, bool leftReClosed, bool leftImClosed, bool rightReClosed, bool rightImClosed) =>
                left == right ? new OneElementPiece(left) : (SetPiece)new Interval(left, right, leftReClosed, leftImClosed, rightReClosed, rightImClosed);
            internal Interval(Entity left, Entity right) : this(left, right, true, true, true, true) { }
            internal Interval(Entity left, Entity right, bool leftReClosed, bool rightReClosed) : this(left, right, leftReClosed, true, rightReClosed, true) { }
            internal Interval(Entity left, Entity right, bool leftReClosed, bool leftImClosed, bool rightReClosed, bool rightImClosed)
            {
                if (left == right)
                    throw new ArgumentException($"{nameof(left)} and {nameof(right)} are equal. Create a {nameof(OneElementPiece)} instead.");
                leftEdge = new Edge(left, leftReClosed, leftImClosed);
                rightEdge = new Edge(right, rightReClosed, rightImClosed);
            }

            public override Edge LowerBound() => leftEdge;
            public override Edge UpperBound() => rightEdge;

            /// <summary>
            /// Used for real intervals only.
            /// <see langword="true"/>: [
            /// <see langword="false"/>: (
            /// </summary>
            public Interval SetLeftClosed(bool isClosed) => SetLeftClosed(isClosed, true);

            /// <summary>
            /// Used for real intervals only.
            /// <see langword="true"/>: ]
            /// <see langword="false"/>: )
            /// </summary>
            public Interval SetRightClosed(bool isClosed) => SetRightClosed(isClosed, true);

            /// <summary>
            /// Used for any type of interval
            /// sets [ or ( for real and [ or ( for imaginary part of the number
            /// </summary>
            public Interval SetLeftClosed(bool Re, bool Im)
            {
                leftEdge = new Edge(leftEdge.Item1, Re, Im);
                return this;
            }

            /// <summary>
            /// Used for any type of interval
            /// sets ] or ) for real and ] or ) for imaginary part of the number
            /// </summary>
            public Interval SetRightClosed(bool Re, bool Im)
            {
                rightEdge = new Edge(rightEdge.Item1, Re, Im);
                return this;
            }

            public override string ToString() =>
                new StringBuilder(leftEdge.Item3 ? "[" : "(")
                .Append(leftEdge.Item2 ? "[" : "(")
                .Append(leftEdge.Item1)
                .Append("; ")
                .Append(rightEdge.Item1)
                .Append(rightEdge.Item2 ? "]" : ")")
                .Append(rightEdge.Item3 ? "]" : ")")
                .ToString();
            public static bool operator ==(Interval A, Interval B)
                => EdgeEqual(A.leftEdge, B.leftEdge) && EdgeEqual(A.rightEdge, B.rightEdge);
            public static bool operator !=(Interval A, Interval B) => !(A == B);
            public override bool Equals(object obj) => obj is Interval p && leftEdge.Equals(p.leftEdge) && rightEdge.Equals(p.rightEdge);
            public override int GetHashCode() => (leftEdge, rightEdge).GetHashCode();
        }
    }
}