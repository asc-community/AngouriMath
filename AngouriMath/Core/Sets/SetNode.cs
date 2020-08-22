
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests.Core")]
namespace AngouriMath.Core
{
    using static Entity.Number;
    /// <summary>
    /// Class defines true mathematical sets
    /// It can be empty, it can contain numbers, it can contain intervals etc. It also maybe an operator (|, &, -)
    /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
    /// TODO: To make sets work faster
    /// </summary>
    public abstract partial record SetNode
    {
        public abstract override string ToString();
        public abstract bool Contains(Piece piece);
        public bool Contains(Set set) => set.Pieces.All(Contains);
        public bool Contains(Entity entity) => Contains(new OneElementPiece(entity));

        public static SetNode operator &(SetNode A, SetNode B) => new Intersection(A, B).Eval();
        public static SetNode operator |(SetNode A, SetNode B) => new Union(A, B).Eval();
        public static SetNode operator -(SetNode A, SetNode B) => new Complement(A, B).Eval();
        public static SetNode operator !(SetNode A) => new Inversion(A).Eval();
        public abstract SetNode Eval();

        /// <summary>A or B</summary>
        internal partial record Union(SetNode A, SetNode B) : SetNode
        {
            public override bool Contains(Piece piece) => A.Contains(piece) || B.Contains(piece);
            public override string ToString() => $"({A})&({B})";
        }
        /// <summary>A and B</summary>
        internal partial record Intersection(SetNode A, SetNode B) : SetNode
        {
            public override bool Contains(Piece piece) => A.Contains(piece) && B.Contains(piece);
            public override string ToString() => $"({A})|({B})";
        }
        /// <summary>A and not B</summary>
        internal partial record Complement(SetNode A, SetNode B) : SetNode
        {
            public override bool Contains(Piece piece) => A.Contains(piece) && !B.Contains(piece);
            public override string ToString() => $@"({A})\({B})";
        }
        /// <summary>not A</summary>
        internal partial record Inversion(SetNode A) : SetNode
        {
            public override bool Contains(Piece piece) => !A.Contains(piece);
            public override string ToString() => $"!({A})";
        }
        static (List<Piece> Good, List<Piece> Bad) GatherEvaluablePieces(Set A)
        {
            var goodPieces = new List<Piece>(); // Those we can eval, e. g. [3; 4]
            var badPieces = new List<Piece>();  // Those we cannot, e. g. [x + 3; -3]
            foreach (var piece in A)
                (piece.IsNumeric() ? goodPieces : badPieces).Add(piece);
            return (goodPieces, badPieces);
        }
    }

    public partial record Set : SetNode, ICollection<Piece>
    {
        public override SetNode Eval() => this;

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<Piece> GetEnumerator() => Pieces.GetEnumerator();

        // TODO: exception
        public FiniteSet FiniteSet()
            => new FiniteSet(Pieces.ToArray());

        internal List<Piece> Pieces = new List<Piece>();

        internal void AddPiece(Piece piece)
        {
            if (FastAddingMode)
            {
                Pieces.Add(piece);
                return;
            }

            if (!piece.IsNumeric())
            {
                if (Pieces.All(p => p != piece))
                    Pieces.Add(piece);
                return;
            }
            else if (piece is OneElementPiece oneelem && AsFiniteSet() is { } finiteSet)
            {
                if (finiteSet.All(finite => finite.Evaled != oneelem.entity.Item1.Evaled))
                    Pieces.Add(piece);
                return;
            }
            var remainders = new List<Piece> { piece };
            foreach (var p in Pieces)
            {
                if (!p.IsNumeric())
                    continue;
                var newRemainders = new List<Piece>();
                foreach (var rem in remainders)
                    newRemainders.AddRange(PieceFunctions.Subtract(rem, p));
                remainders = newRemainders;
            }

            Pieces.AddRange(remainders);
        }

        internal void AddRange(Set set)
        {
            foreach (var piece in set)
                AddPiece(piece);
        }

        public override bool Contains(Piece piece)
        {
            // we will subtract each this.piece from piece and if piece finally becomes 0 then
            // there is no point outside this set
            var remainders = new List<Piece> { piece };
            foreach (var p in this.Pieces)
            {
                var newRemainders = new List<Piece>();
                foreach (var rem in remainders)
                    newRemainders.AddRange(PieceFunctions.Subtract(rem, p));
                remainders = newRemainders;
            }
            return remainders.Count == 0;
        }

        public Set(params Piece[] elements)
        {
            foreach (var el in elements)
                AddPiece(el);
        }

        public void Add(Piece piece) => AddPiece(piece);

        /// <summary>
        /// Adds a setNode of numbers to the setNode
        /// </summary>
        /// <param name="elements"></param>
        public void AddElements(params Entity[] elements)
        {
            foreach (var el in elements)
                AddPiece(Piece.Element(el));
        }

        /// <summary>
        /// Creates an interval, for example
        /// <code>AddInterval(MathS.Sets.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true, true)</code>
        /// </summary>
        public void AddInterval(IntervalPiece interval) => AddPiece(interval);

        public override string ToString() => IsEmpty() ? "{}" : string.Join("|", Pieces);

        public bool IsEmpty() => Pieces.Count == 0;

        public void Clear() => Pieces.Clear();

        public IEnumerable<Entity>? AsFiniteSet() =>
            Pieces.All(piece => piece is OneElementPiece)
            ? Pieces.Select(piece => ((OneElementPiece)piece).entity.Item1)
            : null;

        public int Count
        {
            get
            {
                int count = 0;
                foreach (var piece in Pieces)
                    if (piece is OneElementPiece)
                        count++;
                    else
                        return -1;
                return count;
            }
        }

        /// <summary>
        /// Adding to this <see cref="Set"/> will not check whether <see cref="Entity"/> is already added
        /// </summary>
        public bool FastAddingMode { get; set; } = false;

        public bool IsReadOnly => false;

        /// <summary>Returns a set of all <see cref="Complex"/>es</summary>
        internal static Set C()
            => new Set
            {
                Pieces =
                {
                    Piece.Interval(
                        Complex.NegNegInfinity,
                        Complex.PosPosInfinity,
                        false, false, false, false)
                }
            };

        /// <summary>Returns a set of all <see cref="Real"/>s</summary>
        internal static Set R()
            => new Set
            {
                Pieces =
                {
                    Piece.Interval(Real.NegativeInfinity, Real.PositiveInfinity)
                    .SetLeftClosed(false).SetRightClosed(false)
                }
            };

        /// <summary>Creates a finite set</summary>
        internal static Set Finite(params Entity[] entities)
        {
            var res = new Set();
            foreach (var entity in entities)
                res.AddElements(entity);
            return res;
        }

        public void CopyTo(Piece[] array, int arrayIndex) => Pieces.CopyTo(array, arrayIndex);

        public bool Remove(Piece item) => Pieces.Remove(item);

        IEnumerator<Piece> IEnumerable<Piece>.GetEnumerator() => ((IEnumerable<Piece>)Pieces).GetEnumerator();
    }

    public class FiniteSet : IReadOnlyList<Entity>
    {
        private readonly Entity[] entities;
        public int Count => ((IReadOnlyCollection<Entity>)entities).Count;
        public Entity this[int index] => ((IReadOnlyList<Entity>)entities)[index];
        internal FiniteSet(IEnumerable<Piece> pieces) =>
            entities = pieces.OfType<OneElementPiece>().Select(p => p.entity.Item1).ToArray();
        public List<Entity> ToList() => entities.ToList();
        public IEnumerator<Entity> GetEnumerator() => ((IEnumerable<Entity>)entities).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}