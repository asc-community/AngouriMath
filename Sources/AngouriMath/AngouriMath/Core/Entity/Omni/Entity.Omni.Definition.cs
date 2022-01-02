//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Creates a node of whether the given element is an element of the given set
        /// </summary>
        /// <param name="supSet">
        /// The assumed super-set of the given expression.
        /// </param>
        /// <returns>A node</returns>
        public Entity In(Entity supSet)
            => new Inf(this, supSet);

        /// <summary>
        /// Creates a node of a expression assuming some condition
        /// </summary>
        /// <param name="that">
        /// A condition under which a given expression (this) is valid.
        /// </param>
        /// <returns>A node</returns>
        public Entity Provided(Entity that)
            => new Providedf(this, that);
    }
}
