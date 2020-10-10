using System;
using System.Collections.Generic;
using System.Text;
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
    }
}
