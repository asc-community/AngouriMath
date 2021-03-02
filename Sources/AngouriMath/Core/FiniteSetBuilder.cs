/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.Collections.Generic;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this class for solvers and other places when a set needs to be built
    /// and you want to avoid using lists
    /// </summary>
    internal sealed class FiniteSetBuilder
    {
        private readonly HashSet<Entity> raw = new();

        public bool IsEmpty => raw.Count == 0;
        public FiniteSetBuilder() { }
        public FiniteSetBuilder(IEnumerable<Entity> elements)
        { 
            raw = new(elements);
        }

        public void Add(Entity element)
            => raw.Add(element);

        public void Remove(Entity element)
            => raw.Remove(element);

        public FiniteSet ToFiniteSet()
            => raw.Count == 0 ? Empty : new FiniteSet(raw);
    }
}
