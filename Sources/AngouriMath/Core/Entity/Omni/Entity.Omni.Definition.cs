/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Creates a node of whether the given element is an element of the given set
        /// </summary>
        /// <returns>A node</returns>
        public Entity In(Entity supSet)
            => new Inf(this, supSet);

        /// <summary>
        /// Creates a node of a expression assuming some condition
        /// </summary>
        /// <param name="that"></param>
        /// <returns></returns>
        public Entity Provided(Entity that)
            => new Providedf(this, that);
    }
}
