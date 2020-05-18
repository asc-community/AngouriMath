
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
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Sets;

[assembly: InternalsVisibleTo("UnitTests.Core")]
namespace AngouriMath.Core
{
    /// <summary>
    /// Class defines true mathematical sets
    /// It can be empty, it can contain numbers, it can contain intervals etc. It also maybe an operator (|, &, -)
    /// It supports intersection (with & operator), union (with | operator), subtracting (with - operator)
    /// TODO: To make sets work faster
    /// </summary>
    public abstract partial class SetNode
    {
        public enum NodeType
        {
            SET,
            OPERATOR
        }

        public NodeType Type { get; }

        protected SetNode(NodeType type)
            => Type = type;

        public abstract bool Contains(Piece piece);
        public abstract bool Contains(Set piece);
        public abstract bool Contains(Entity piece);

        public static SetNode operator &(SetNode A, SetNode B)
            => OperatorSet.And(A, B);

        public static SetNode operator |(SetNode A, SetNode B)
            => OperatorSet.Or(A, B);

        public static SetNode operator -(SetNode A, SetNode B)
            => OperatorSet.Minus(A, B);

        public static SetNode operator !(SetNode A)
            => OperatorSet.Inverse(A);

        public SetNode Eval()
        {
            switch (Type)
            {
                case NodeType.SET:
                    return this;
                case NodeType.OPERATOR:
                    var op = this as OperatorSet;
                    return op.ConnectionType switch
                    {
                        OperatorSet.OperatorType.UNION =>
                            SetFunctions.Unite(op.Children[0], op.Children[1]),
                        OperatorSet.OperatorType.INTERSECTION =>
                            SetFunctions.Intersect(op.Children[0], op.Children[1]),
                        OperatorSet.OperatorType.COMPLEMENT =>
                            SetFunctions.Subtract(op.Children[0], op.Children[1]),
                        OperatorSet.OperatorType.INVERSION =>
                            SetFunctions.Invert(op.Children[0])
                    };
            }

            throw new SysException("Unknown error");
        }
    }

    public class OperatorSet : SetNode
    {
        public enum OperatorType
        {
            UNION,           // OR
            INTERSECTION,    // AND
            COMPLEMENT,      // SUBTRACT
            INVERSION,       // NOT
        }
        internal SetNode[] Children { get; }
        public readonly OperatorType ConnectionType;
        internal OperatorSet(OperatorType type, params SetNode[] children) : base(NodeType.OPERATOR)
        {
            Children = children;
            ConnectionType = type;
        }

        public override string ToString()
            => SetToString.OperatorToString(this);

        public override bool Contains(Piece piece)
            => ConnectionType switch
            {
                OperatorType.UNION => Children[0].Contains(piece) || Children[1].Contains(piece),
                OperatorType.INTERSECTION => Children[0].Contains(piece) && Children[1].Contains(piece),
                OperatorType.COMPLEMENT => Children[0].Contains(piece) && !Children[1].Contains(piece),
                OperatorType.INVERSION => !Children[0].Contains(piece)
            };

        public override bool Contains(Set set)
            => set.Pieces.All(Contains);

        public override bool Contains(Entity entity)
            => Contains(new OneElementPiece(entity));

        internal static SetNode And(SetNode A, SetNode B)
        => new OperatorSet(OperatorSet.OperatorType.INTERSECTION, A, B);

        internal static SetNode Or(SetNode A, SetNode B)
            => new OperatorSet(OperatorSet.OperatorType.UNION, A, B);

        internal static SetNode Minus(SetNode A, SetNode B)
        => new OperatorSet(OperatorSet.OperatorType.COMPLEMENT, A, B);

        internal static SetNode Inverse(SetNode A)
        => new OperatorSet(OperatorSet.OperatorType.INVERSION, A);
    }

    public class Set : SetNode, IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public SetEnumerator GetEnumerator()
            => new SetEnumerator(Pieces.ToArray());

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
            var remainders = new List<Piece>{ piece };
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
            var remainders = new List<Piece>{ piece };
            foreach (var p in this.Pieces)
            {
                var newRemainders = new List<Piece>();
                foreach (var rem in remainders)
                    newRemainders.AddRange(PieceFunctions.Subtract(rem, p));
                remainders = newRemainders;
            }
            return remainders.Count == 0;
        }

        public override bool Contains(Set set)
            => set.Pieces.All(Contains);

        public override bool Contains(Entity entity)
            => Contains(new OneElementPiece(entity));

        public Set(params Piece[] elements) : base(NodeType.SET)
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
            foreach(var el in elements)
                AddPiece(Piece.Element(el));
        }


        /// <summary>
        /// Creates an interval, for example
        /// AddInterval(MathS.Sets.Interval(3, 4).SetLeftClosed(true).SetRightClosed(true, true)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public void AddInterval(IntervalPiece interval) 
            => AddPiece(interval);

        public override string ToString()
            => SetToString.LinearSetToString(this);

        public bool IsEmpty() => Pieces.Count == 0;

        public enum PowerLevel
        {
            EMPTY,
            FINITE,
            INFINITE
        }

        public PowerLevel Power
        {
            get
            {
                if (Pieces.Count == 0)
                    return 0;
                if (Pieces.Any(p => p.Type == Piece.PieceType.INTERVAL))
                    return PowerLevel.INFINITE;
                else
                    return PowerLevel.FINITE;
            }
        }

        public int Count
        {
            get => Power == PowerLevel.INFINITE ? -1 : Pieces.Count;
        }

        public IEnumerable<T> Select<T>(Func<Piece, T> selector)
            => Pieces.Select(selector);

        public Set Where(Func<Piece, bool> selector)
            => new Set { Pieces = Pieces.Where(selector).ToList() };

        public Set FiniteWhere(Func<Entity, bool> selector)
            => new Set
            {
                Pieces = Pieces.Where(p => p.Type == Piece.PieceType.ENTITY && selector((Entity)p)).ToList()
            };

        public void Apply(Func<Piece, Piece> processor)
        {
            for (int i = 0; i < Pieces.Count; i++)
                Pieces[i] = processor(Pieces[i]);
        }

        public void FiniteApply(Func<Entity, Entity> processor)
        {
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].Type == Piece.PieceType.INTERVAL)
                    continue;
                var oneelem = Pieces[i] as OneElementPiece;
                oneelem.entity = new Tuple<Entity, bool, bool>(
                    processor(oneelem.entity.Item1),
                    oneelem.entity.Item2,
                    oneelem.entity.Item3);
            }
        }

        /// <summary>
        /// Without check whether entity is already in the set
        /// </summary>
        public bool FastAddingMode { get; set; } = false;

        /// <summary>
        /// Returns a set of all complex numbers
        /// </summary>
        /// <returns></returns>
        internal static Set C()
            => new Set
            {
                Pieces = new List<Piece>
                {
                    Piece.Interval(
                        new Number(double.NegativeInfinity, double.NegativeInfinity),
                        new Number(double.PositiveInfinity, double.PositiveInfinity)
                    ).AsInterval().SetLeftClosed(false, false).SetRightClosed(false, false)
                }
            };

        /// <summary>
        /// Returns a set of all real numbers
        /// </summary>
        /// <returns></returns>
        internal static Set R()
            => new Set
            {
                Pieces = new List<Piece>
                {
                    Piece.Interval(
                        new Number(double.NegativeInfinity),
                        new Number(double.PositiveInfinity)
                    ).AsInterval().SetLeftClosed(false).SetRightClosed(false)
                }
            };

        /// <summary>
        /// Creates a finite set
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        internal static Set Finite(params Entity[] entities)
        {
            var res = new Set();
            foreach (var entity in entities)
                res.AddElements(entity);
            return res;
        }
    }

    public class FiniteSet : IEnumerable
    {
        private Piece[] pieces;
        internal FiniteSet(Piece[] pieces)
            => this.pieces = pieces;
        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)GetEnumerator();
        }

        public SetFiniteEnumerator GetEnumerator()
            => new SetFiniteEnumerator(
                pieces.Where(p => p.Type == Piece.PieceType.ENTITY)
                    .Select(p => (Entity)p).ToArray()
            );

        public List<Entity> ToList()
        => pieces.Select(c => (c as OneElementPiece).entity.Item1).ToList();
    }

    public class SetFiniteEnumerator : IEnumerator
    {
        private Entity[] entities;
        private int position = -1;
        public SetFiniteEnumerator(Entity[] entities)
            => this.entities = entities;

        public bool MoveNext()
        {
            position++;
            return position < entities.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
            // do nothing if disposed
        }

        object IEnumerator.Current
        {
            get => Current;
        }

        public Entity Current
        {
            get => entities[position];
        }
    }

    public class SetEnumerator : IEnumerator
    {
        private Piece[] pieces;
        private int position = -1;

        public SetEnumerator(Piece[] pieces)
            => this.pieces = pieces;

        public bool MoveNext()
        {
            position++;
            return position < pieces.Length;
        }

        public void Reset()
        {
            position = -1;
        }

        public void Dispose()
        {
            // do nothing if disposed
        }

        object IEnumerator.Current
        {
            get => Current;
        }

        public Piece Current
        {
            get => pieces[position];
        }
    }
}
