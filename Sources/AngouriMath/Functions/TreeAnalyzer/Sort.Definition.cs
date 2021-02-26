/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity.Number;

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