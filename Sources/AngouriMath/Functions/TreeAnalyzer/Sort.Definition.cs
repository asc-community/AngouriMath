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
        internal string SortHash(SortLevel level) =>
            SortHashName(level) + string.Join("_", DirectChildren.Select(child => child.SortHash(level)).Where(x => x is not ""));
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