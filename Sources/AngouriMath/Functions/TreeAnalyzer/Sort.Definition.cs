//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;

namespace AngouriMath
{
    using SortLevel = Functions.TreeAnalyzer.SortLevel;
    partial record Entity
    {
        /// <summary>Hash that is convenient to sort with</summary>
        /// <remarks>
        /// Spelt once per instance and level, as <see cref="InnerSimplified"/> and
        /// <see cref="SimplifiedRate"/> are computed once. The key of a node is built out of the
        /// keys of every node below it, and the sort asks for the key of every operand of every
        /// chain it orders, so without the memo one sort of a tree walked and re-spelt each
        /// subtree once per ancestor -- and the simplifier sorts what it registers at every pass.
        /// On <c>SimplifyHard</c> that was 174 MB of one run's 735, some 2.6 MB per sort of a
        /// tree a few hundred nodes wide. A candidate shares most of its subtrees with the
        /// candidates before it, and a shared subtree's key is now spelt once.
        /// <para/>
        /// Three things about the shape of the memo, each measured. It is one reference per node
        /// and an array made on the first sort, not a lazy slot per level: three slots cost
        /// every node some fifty bytes, which the gate saw as +4-6% on every solve and derivative
        /// entry for a saving only the sort ever sees. It is on the instance and not in a
        /// dictionary kept for the run: keyed by reference the dictionary saved the same bytes
        /// and cost a lookup per node per sort, 369 ms against 266 for the same input. And the
        /// array names its owner, because a <c>with</c> copy -- <see cref="WithCodomain"/>, a
        /// re-differentiation -- carries the original's fields while a number's key spells its
        /// codomain: a copy finds an array that is not its own and starts one of its own.
        /// https://github.com/asc-community/AngouriMath/issues/746
        /// </remarks>
        internal string SortHash(SortLevel level)
        {
            var cache = sortHashes.Keys;
            if (cache is null || !ReferenceEquals(cache[Owner], this))
            {
                // Two threads may each make one; both are right, and whichever lands stays.
                cache = new object?[Owner + 1];
                cache[Owner] = this;
                sortHashes.Keys = cache;
            }
            if (cache[(int)level] is string known)
                return known;
            var key = SortHashName(level) + string.Join("_", DirectChildren.Select(child => child.SortHash(level)).Where(x => x is not ""));
            cache[(int)level] = key;
            return key;
        }
        /// <summary>The slot after the three levels, holding the node the array was made for.</summary>
        private const int Owner = 3;
        private SortKeyCache sortHashes;

        /// <summary>
        /// The keys, behind a struct that is equal to every other, for the reason
        /// <c>LazyPropertyA</c> is: an <see cref="Entity"/> is a record and compares every
        /// field, so a bare array here made two equal trees unequal the moment one had been
        /// sorted -- and the simplifier, which recognises a repeated candidate by equality,
        /// then never recognised one. Measured: 30 GB and 57 s on <c>SimplifyHard</c>.
        /// </summary>
        private struct SortKeyCache : IEquatable<SortKeyCache>
        {
            internal object?[]? Keys;
            public bool Equals(SortKeyCache other) => true;
            public override bool Equals(object? obj) => obj is SortKeyCache;
            public override int GetHashCode() => 0;
        }
        private protected abstract string SortHashName(SortLevel level);
    }
}

namespace AngouriMath.Functions
{
    internal static partial class TreeAnalyzer
    {
        internal static IEnumerable<Entity> SortRealsAndNonReals(IEnumerable<Entity> entities)
        {
            var reals = entities.OfType<Real>();
            var nonReals = entities.Where(c => c is not Real);
            var all = reals.OrderBy(c => c).Concat(nonReals);
            return all;
        }

        /// <summary>Binary multi hanging: ((1 + 1) + (1 + 1))</summary>
        internal static Entity MultiHangBinary(IReadOnlyList<Entity> children, Func<Entity, Entity, Entity> op)
        {
            Entity MultiHangBinary(int start, int length) =>
                length switch
                {
                    0 => throw new AngouriBugException("At least 1 child required"),
                    1 => children[start],
                    2 => op(children[start], children[start + 1]),
                    _ => op(MultiHangBinary(start, length / 2),
                            MultiHangBinary(start + length / 2, length - length / 2))
                };
            return MultiHangBinary(0, children.Count);
        }
        internal enum SortLevel
        {
            HIGH_LEVEL, // Variables, functions. Doesn't pay attention to constants or ops
            MIDDLE_LEVEL, // Contants are now countable
            LOW_LEVEL, // De facto full hash
        }
    }
}