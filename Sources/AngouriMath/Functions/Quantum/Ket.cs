//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Linq;
using AngouriMath.Functions.Algebra.MonoidAlgebra;

namespace AngouriMath.Functions.Quantum
{
    /// <summary>
    /// A computational basis state: a fixed-width word over {0, 1}, with
    /// <see cref="Free"/> marking a position that has been factored out.
    /// </summary>
    /// <remarks>
    /// Compared by value, which <see cref="SparseTerms{TBasis}"/> requires -- keyed on a
    /// reference-equal basis it would silently stop collecting like terms.
    /// </remarks>
    internal readonly struct Ket : IEquatable<Ket>
    {
        /// <summary>A position no longer fixed, because it was divided out.</summary>
        internal const int Free = -1;

        internal int[] Cells { get; }

        internal Ket(params int[] cells) => Cells = cells;

        internal int Width => Cells.Length;

        internal bool IsAllFree => Cells.All(cell => cell == Free);

        public bool Equals(Ket other) => Cells.SequenceEqual(other.Cells);

        public override bool Equals(object? obj) => obj is Ket other && Equals(other);

        public override int GetHashCode() => Cells.Aggregate(17, (hash, cell) => hash * 31 + cell);

        public override string ToString()
            => string.Concat(Cells.Select(cell => cell == Free ? "-" : cell.ToString()));
    }

    /// <summary>
    /// The monoid and lattice structure on kets: the tensor product joins them, and the meet
    /// keeps a position only where both agree.
    /// </summary>
    /// <remarks>
    /// The alphabet is unordered, so this is the meet in a product of *flat* lattices -- which
    /// is the only place it differs from a polynomial's exponent vector, where the meet is a
    /// componentwise minimum in a product of chains. Everything else about factoring is shared.
    /// </remarks>
    internal sealed class KetOps : IBasisOps<Ket>
    {
        private readonly int width;

        internal KetOps(int width) => this.width = width;

        public Ket Identity => new(Enumerable.Repeat(Ket.Free, width).ToArray());

        /// <summary>
        /// The tensor product, for kets that occupy disjoint positions: each takes the value
        /// the other leaves free.
        /// </summary>
        public Ket Combine(Ket left, Ket right)
            => new(left.Cells.Zip(right.Cells,
                    (l, r) => l == Ket.Free ? r : l).ToArray());

        public Ket Meet(Ket left, Ket right)
            => new(left.Cells.Zip(right.Cells,
                    (l, r) => l == r ? l : Ket.Free).ToArray());

        public bool TryDivide(Ket whole, Ket part, out Ket quotient)
        {
            quotient = new Ket(whole.Cells.Zip(part.Cells,
                    (w, p) => p == Ket.Free ? w : Ket.Free).ToArray());
            return whole.Cells.Zip(part.Cells, (w, p) => p == Ket.Free || w == p).All(ok => ok);
        }
    }
}
