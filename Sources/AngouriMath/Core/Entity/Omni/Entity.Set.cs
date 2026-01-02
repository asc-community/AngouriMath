//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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

            /// <summary>
            /// Returns an empty set
            /// You can use it to compare sets to it
            /// or to avoid allocations
            /// </summary>
            [ConstantField] public readonly static FiniteSet Empty = new FiniteSet();

            /// <summary>
            /// Checks that a set is finite
            /// </summary>
            public abstract bool IsSetFinite { get; }

            /// <summary>
            /// Checks that a set does not contain any elements
            /// </summary>
            public abstract bool IsSetEmpty { get; }

            /// <summary>
            /// Adds a constraint to every element of a set.
            /// 1) For a finite set, it will add Provided for every element
            /// 2) For an interval and special set, it will wrap it with a cset (e. g. [a; b].Filter(x > 0, x) -> { x : x in [a; b] and x > 0 })
            /// 3) For a cset, it will add a predicate (e. g. { y : y2 = 3 }.Filter(x > 0, x) -> { y : y2 = 3 and y > 0 })
            /// </summary>
            public abstract Set Filter(Entity predicate, Variable over);
        }

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

#pragma warning disable CS1591
        public static implicit operator Entity(Domain domain) => Set.SpecialSet.Create(domain);
        public static implicit operator Entity((Entity left, Entity right) interval) => new Interval(interval.left, true, interval.right, true);
        public static implicit operator Entity(Entity[] elements) => new FiniteSet(elements);
        public static implicit operator Entity(List<Entity> elements) => new FiniteSet((IEnumerable<Entity>)elements);
#pragma warning restore CS1591

    }
}
