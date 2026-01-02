//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Set;

namespace AngouriMath.Core
{
    /// <summary>
    /// Use this class for solvers and other places when a set needs to be built
    /// and you want to avoid using lists. It builds an instance of <see cref="FiniteSet"/>.
    /// </summary>
    public sealed class FiniteSetBuilder
    {
        private readonly HashSet<Entity> raw = new();

        /// <summary>
        /// Checks whether the number of elements added to the builder is zero
        /// </summary>
        public bool IsEmpty => raw.Count == 0;

        /// <summary>
        /// Creates an instance of <see cref="FiniteSetBuilder"/> with no elements.
        /// </summary>
        public FiniteSetBuilder() { }

        /// <summary>
        /// Creates an instance of <see cref="FiniteSetBuilder"/> with elements, provided in the argument.
        /// It does not check for uniqueness of elements, however, it will be performed automatically once
        /// you will call the <see cref="ToFiniteSet"/> method.
        /// </summary>
        public FiniteSetBuilder(IEnumerable<Entity> elements)
        { 
            raw = new(elements);
        }

        /// <summary>
        /// It does not check for uniqueness of elements, however, it will be performed automatically once
        /// you will call the <see cref="ToFiniteSet"/> method.
        /// </summary>
        public void Add(Entity element)
            => raw.Add(element);

        /// <summary>
        /// Removes a given element from the builder
        /// </summary>
        /// <param name="element">
        /// The element to remove. If no such element was found, method
        /// silently exits.
        /// </param>
        public void Remove(Entity element)
            => raw.Remove(element);

        /// <summary>
        /// Build itself into a <see cref="FiniteSet"/> entity. This method
        /// can be called multiple times throughout its lifetime.
        /// </summary>
        /// <example>
        /// <code>
        /// var builder = new FiniteSetBuilder();
        /// Console.WriteLine(builder.ToFiniteSet());
        /// builder.Add(3);
        /// Console.WriteLine(builder.ToFiniteSet());
        /// builder.Add(4);
        /// Console.WriteLine(builder.ToFiniteSet());
        /// builder.Add(5);
        /// Console.WriteLine(builder.ToFiniteSet());
        /// </code>
        /// </example>
        /// <returns>
        /// An immutable <see cref="FiniteSet"/> entity.
        /// </returns>
        public FiniteSet ToFiniteSet()
            => raw.Count == 0 ? Empty : new FiniteSet(raw);
    }
}
