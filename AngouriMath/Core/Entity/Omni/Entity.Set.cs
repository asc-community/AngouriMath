
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
using System.Collections.Generic;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Set_(mathematics)"/>
        /// A set might be a set operator, conditinal set, finite set, or interval
        /// </summary>
        public abstract partial record Set
        {
            /// <summary>
            /// Checks whether the given element is in the set
            /// </summary>
            /// <param name="entity">The element to find in the set</param>
            /// <param name="contains">The result whether it is in the set or not</param>
            /// <returns>Whether this is possible to determine that it contains the given element</returns>
            public abstract bool TryContains(Entity entity, out bool contains);

            /// <summary>
            /// Checks that an element is in the set
            /// Unless you are confident about the set,
            /// it is recommended to use <see cref="TryContains"/> instead
            /// </summary>
            /// <exception cref="ElementInSetAmbiguousException">Thrown when </exception>
            public bool Contains(Entity entity)
                => TryContains(entity, out var res) ? res : throw new ElementInSetAmbiguousException("Cannot determine whether the element is in the set");

            public readonly static FiniteSet Empty = new FiniteSet();

            public abstract bool IsSetFinite { get; }
            public abstract bool IsSetEmpty { get; }
        }

        public static implicit operator Entity(Domain domain) => Set.SpecialSet.Create(domain);

        /// <summary>
        /// Creates a node of union of two nodes (sets)
        /// </summary>
        /// <returns>A new node</returns>
        public Set Unite(Entity anotherSet) => new Unionf(this, anotherSet);

        /// <summary>
        /// Creates a node of intersection of two nodes (sets)
        /// </summary>
        /// <returns>A new node</returns>
        public Set Intersect(Entity anotherSet) => new Intersectionf(this, anotherSet);

        /// <summary>
        /// Creates a new node of set difference of two nodes (sets)
        /// </summary>
        /// <returns>A new node</returns>
        public Set SetSubtract(Entity anotherSet) => new SetMinusf(this, anotherSet);

        public static implicit operator Entity((Entity left, Entity right) interval) => new Interval(interval.left, true, interval.right, true);

        public static implicit operator Entity(Entity[] elements) => new FiniteSet(elements);
        public static implicit operator Entity(List<Entity> elements) => new FiniteSet((IEnumerable<Entity>)elements);
    }
}
