/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using System.Linq;
using static AngouriMath.Entity;
using System.Collections.Generic;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        private static Entity SortAndGroup(IEnumerable<Entity> children, TreeAnalyzer.SortLevel level, Func<Entity, Entity, Entity> ctor)
        {
            var groups = new Dictionary<string, List<Entity>>();
            foreach (var child in children)
            {
                var hash = child.SortHash(level);
                if (!groups.ContainsKey(hash))
                    groups[hash] = new();
                groups[hash].Add(child);
            }
            return groups.OrderBy(pair => pair.Key).Select(pair => pair.Value.Aggregate(ctor)).Aggregate(ctor);
        }

        /// <summary>Actual sorting with <see cref="Entity.SortHash(TreeAnalyzer.SortLevel)"/></summary>
        internal static Func<Entity, Entity> SortRules(TreeAnalyzer.SortLevel level) => x => x switch
        {
            Sumf or Minusf =>
                SortAndGroup(Sumf.LinearChildren(x), level, (a, b) => a + b),
            Mulf or Divf =>
                SortAndGroup(Mulf.LinearChildren(x), level, (a, b) => a * b),
            Andf =>
                SortAndGroup(Andf.LinearChildren(x), level, (a, b) => a & b),
            Orf =>
                SortAndGroup(Orf.LinearChildren(x), level, (a, b) => a | b),
            Unionf =>
                SortAndGroup(Unionf.LinearChildren(x), level, (a, b) => a.Unite(b)),
            Intersectionf =>
                SortAndGroup(Intersectionf.LinearChildren(x), level, (a, b) => a.Intersect(b)),
            _ => x,
        };
        internal static Entity PolynomialLongDivision(Entity x) =>
            x is Divf(var num, var denom)
            && TreeAnalyzer.PolynomialLongDivision(num, denom) is var (divided, remainder)
            ? divided + remainder
            : x;
    }
}