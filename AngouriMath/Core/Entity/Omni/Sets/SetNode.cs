
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

using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("UnitTests.Core")]
namespace AngouriMath
{
    using static Entity.Number;

    partial record Entity
    {

        /// <summary>
        /// Class defines true mathematical sets
        /// It can be empty, it can contain numbers, it can contain intervals etc. It also maybe an operator (|, &, -)
        /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
        /// TODO: To make sets work faster
        /// </summary>
        public abstract partial record SetNode
        {
            public abstract override string ToString();
            public abstract bool Contains(SetPiece piece);
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
                public override bool Contains(SetPiece piece) => A.Contains(piece) || B.Contains(piece);
                public override string ToString() => $@"({A}) \/ ({B})";
                public override bool IsFinite => false; //A.IsFinite && B.IsFinite; we can't iterate over this :(
                public override bool IsEmpty => false; //A.IsEmpty && B.IsEmpty; we can't iterate over this :(
            }
            /// <summary>A and B</summary>
            internal partial record Intersection(SetNode A, SetNode B) : SetNode
            {
                public override bool Contains(SetPiece piece) => A.Contains(piece) && B.Contains(piece);
                public override string ToString() => $@"({A}) /\ ({B})";
                public override bool IsFinite => false; // A.IsFinite || B.IsFinite; we can't iterate over this :(
                public override bool IsEmpty => false; // A.IsEmpty || B.IsEmpty; we can't iterate over this :(
            }
            /// <summary>A and not B</summary>
            internal partial record Complement(SetNode A, SetNode B) : SetNode
            {
                public override bool Contains(SetPiece piece) => A.Contains(piece) && !B.Contains(piece);
                public override string ToString() => $@"({A}) \ ({B})";
                public override bool IsFinite => false; // A.IsFinite; we can't iterate over this :(
                public override bool IsEmpty => false; // A.IsEmpty; we can't iterate over this :(
            }
            /// <summary>not A</summary>
            internal partial record Inversion(SetNode A) : SetNode
            {
                public override bool Contains(SetPiece piece) => !A.Contains(piece);
                public override string ToString() => $"!({A})";
                public override bool IsFinite => false;
                public override bool IsEmpty => false;
            }
            static (List<SetPiece> Good, List<SetPiece> Bad) GatherEvaluablePieces(Set A)
            {
                var goodPieces = new List<SetPiece>(); // Those we can eval, e. g. [3; 4]
                var badPieces = new List<SetPiece>();  // Those we cannot, e. g. [x + 3; -3]
                foreach (var piece in A)
                    (piece.Evaluable ? goodPieces : badPieces).Add(piece);
                return (goodPieces, badPieces);
            }
            /// <summary>
            /// Performs <paramref name="func"/> on all of <paramref name="left"/> and <paramref name="right"/>.
            /// Skips over any <see langword="null"/>s of <paramref name="left"/> and <paramref name="right"/>.
            /// </summary>
            /// <param name="left">If <see langword="null"/>, initializes to the first item of <paramref name="right"/>.</param>
            protected static IEnumerable<SetPiece>? RepeatApply
                (IEnumerable<SetPiece>? left, IEnumerable<SetPiece?> right,
                 Func<SetPiece, SetPiece, IEnumerable<SetPiece>> func)
            {
                using var enumerator = right.GetEnumerator();
                var remainders = left;
                if (remainders is null)
                {
                    do if (!enumerator.MoveNext()) return null;
                    while (enumerator.Current is null);
                    remainders = new[] { enumerator.Current };
                }
                while (enumerator.MoveNext())
                    if (enumerator.Current is { } current)
                        remainders = remainders.SelectMany(rem => func(rem, current));
                return remainders;
            }

            public abstract bool IsFinite { get; }
            public abstract bool IsEmpty { get; }

            // TODO: Should've made it a virtual method instead?
            public bool IsFiniteSet(out IEnumerable<Entity> res)
            {
                if (!IsFinite)
                {
                    res = Enumerable.Empty<Entity>();
                    return false;
                }
                if (this is not Set set)
                    throw new AngouriBugException($"If a set is finite, it should be of the {nameof(Set)} type");
                var resQ = set.AsFiniteSetInternal();
                if (resQ is null)
                    throw new AngouriBugException($"If a set is finite, it should not be null");
                res = resQ;
                return true;
            }

            public IEnumerable<Entity> AsFiniteSet()
                // Should be a more appropriate exception
                => IsFiniteSet(out var res) ? res : throw new InvalidOperationException("The given set is non-finite");
        }

        public partial record Set : SetNode, ICollection<SetPiece>
        {
            public override SetNode Eval() => this;

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            public IEnumerator<SetPiece> GetEnumerator() => Pieces.GetEnumerator();

            // TODO: exception
            public FiniteSet FiniteSet()
                => new FiniteSet(Pieces.ToArray());

            internal List<SetPiece> Pieces = new List<SetPiece>();

            internal void AddPiece(SetPiece piece)
            {
                if (FastAddingMode)
                    Pieces.Add(piece);
                else if (!piece.Evaluable)
                {
                    if (Pieces.All(p => (p.LowerBound(), p.UpperBound()) != (piece.LowerBound(), piece.UpperBound())))
                        Pieces.Add(piece);
                }
                else if (piece is OneElementPiece oneelem && IsFiniteSet(out var finiteSet))
                {
                    if (finiteSet.All(finite => finite.Evaled != oneelem.entity.Evaled))
                        Pieces.Add(piece);
                }
                else
                    Pieces.AddRange(RepeatApply(new[] { piece }, Pieces.Where(p => p.Evaluable), PieceFunctions.Subtract));
            }

            internal void AddRange(Set set)
            {
                foreach (var piece in set)
                    AddPiece(piece);
            }

            public override bool Contains(SetPiece piece)
            {
                // we will subtract each this.piece from piece and if piece finally becomes 0 then
                // there is no point outside this set
                var remainders = new List<SetPiece> { piece };
                foreach (var p in this.Pieces)
                {
                    var newRemainders = new List<SetPiece>();
                    foreach (var rem in remainders)
                        newRemainders.AddRange(PieceFunctions.Subtract(rem, p));
                    remainders = newRemainders;
                }
                return remainders.Count == 0;
            }

            public Set(params SetPiece[] elements)
            {
                foreach (var el in elements)
                    AddPiece(el);
            }

            public void Add(SetPiece piece) => AddPiece(piece);

            /// <summary>
            /// Adds a setNode of numbers to the setNode
            /// </summary>
            /// <param name="elements"></param>
            public void AddElements(params Entity[] elements)
            {
                foreach (var el in elements)
                    AddPiece(SetPiece.Element(el));
            }

            /// <summary>
            /// Creates an interval, for example
            /// <code>AddInterval(MathS.Sets.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true, true)</code>
            /// </summary>
            public void AddInterval(Interval interval) => AddPiece(interval);

            public override string ToString() => IsEmpty ? "{}" : string.Join("|", Pieces);

            public override bool IsEmpty => Pieces.Count == 0;
            public override bool IsFinite => Count != -1;

            public void Clear() => Pieces.Clear();

            internal IEnumerable<Entity>? AsFiniteSetInternal() =>
                Pieces.All(piece => piece is OneElementPiece)
                ? Pieces.Select(piece => ((OneElementPiece)piece).entity)
                : null;

            public int Count => (Pieces.Any(p => p is Interval) ? -1 : Pieces.Count);

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
                    new Interval(
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
                    new Interval(Real.NegativeInfinity, Real.PositiveInfinity)
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

            public void CopyTo(SetPiece[] array, int arrayIndex) => Pieces.CopyTo(array, arrayIndex);

            public bool Remove(SetPiece item) => Pieces.Remove(item);

            IEnumerator<SetPiece> IEnumerable<SetPiece>.GetEnumerator() => ((IEnumerable<SetPiece>)Pieces).GetEnumerator();
        }

        /// <summary>
        /// A collection whose length is finite so that you can
        /// perform arithmetic operations on it
        /// </summary>
        public class FiniteSet : IReadOnlyList<Entity>
        {
            private readonly Entity[] entities;
            public int Count => ((IReadOnlyCollection<Entity>)entities).Count;
            public Entity this[int index] => ((IReadOnlyList<Entity>)entities)[index];
            internal FiniteSet(IEnumerable<SetPiece> pieces) =>
                entities = pieces.OfType<OneElementPiece>().Select(p => p.entity).ToArray();
            public List<Entity> ToList() => entities.ToList();
            public IEnumerator<Entity> GetEnumerator() => ((IEnumerable<Entity>)entities).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}