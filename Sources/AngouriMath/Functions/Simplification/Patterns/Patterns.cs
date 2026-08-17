//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions
{
    internal static partial class Patterns
    {
        private static Entity SortAndGroup(Entity original, IEnumerable<Entity> children, TreeAnalyzer.SortLevel level, Func<Entity, Entity, Entity> ctor)
        {
            var groups = new Dictionary<string, List<Entity>>();
            foreach (var child in children)
            {
                var hash = child.SortHash(level);
                if (!groups.ContainsKey(hash))
                    groups[hash] = new();
                groups[hash].Add(child);
            }
            var sorted = groups.OrderBy(pair => pair.Key).Select(pair => pair.Value.Aggregate(ctor)).Aggregate(ctor);
            // Hand the original back where the sort changed nothing. Aggregate builds a fresh
            // node whether or not the order moved, and an Entity memoises InnerSimplified, Evaled
            // and its complexity on the instance -- so rebuilding an already-ordered chain threw
            // all of that away and made the next pass re-derive it from cold.
            // https://github.com/asc-community/AngouriMath/issues/975
            return sorted == original ? original : sorted;
        }

        /// <summary>Actual sorting with <see cref="Entity.SortHash(TreeAnalyzer.SortLevel)"/></summary>
        [AddressableRules]
        internal static Func<Entity, Entity> SortRules(TreeAnalyzer.SortLevel level) => x => x switch
        {
            Sumf or Minusf =>
                SortAndGroup(x, Sumf.LinearChildren(x), level, (a, b) => a + b),
            Mulf or Divf =>
                SortAndGroup(x, Mulf.LinearChildren(x), level, (a, b) => a * b),
            Andf =>
                SortAndGroup(x, Andf.LinearChildren(x), level, (a, b) => a & b),
            Orf =>
                SortAndGroup(x, Orf.LinearChildren(x), level, (a, b) => a | b),
            Unionf =>
                SortAndGroup(x, Unionf.LinearChildren(x), level, (a, b) => a.Unite(b)),
            Intersectionf =>
                SortAndGroup(x, Intersectionf.LinearChildren(x), level, (a, b) => a.Intersect(b)),
            Xorf => 
                SortAndGroup(x, Xorf.LinearChildren(x), level, (a, b) => a ^ b),
            _ => x,
        };
        [AddressableRules]
        internal static Entity PolynomialLongDivision(Entity x) =>
            x is Divf(var num, var denom)
            && TreeAnalyzer.PolynomialLongDivision(num, denom) is var (divided, remainder)
            ? divided + remainder
            : x;

        /// <summary>
        /// A quotient of polynomials put into lowest terms:
        /// <c>(x^2 + 2xy + y^2) / (x^2 - y^2)</c> becomes <c>(x + y) / (x - y)</c>.
        /// See <see cref="PolynomialGcd"/> for what is and is not attempted, and
        /// <a href="https://github.com/asc-community/AngouriMath/issues/55">#55</a>.
        /// </summary>
        [AddressableRules]
        internal static Entity PolynomialGcdCancellation(Entity x) =>
            x is Divf(var num, var denom)
            && PolynomialGcd.TryCancel(num, denom, out var cancelled)
            ? cancelled
            : x;
    }
}