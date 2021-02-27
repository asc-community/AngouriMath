/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// This class describes every node whose value is boolean, that is, true or false
        /// </summary>
        public abstract partial record Statement : Entity
        {
            
        }

        /// <summary>
        /// =, &lt;, &gt;, &gt;=, &lt;=
        /// </summary>
        public abstract partial record ComparisonSign : Statement
        {

        }

        /// <returns>A node</returns>
        public static implicit operator Entity(bool v) => Boolean.Create(v);

        /// <returns>A node</returns>
        public static Entity operator !(Entity a) => new Notf(a);

        /// <returns>A node</returns>
        public static Entity operator &(Entity a, Entity b) => new Andf(a, b);

        /// <returns>A node</returns>
        public static Entity operator |(Entity a, Entity b) => new Orf(a, b);

        /// <summary>
        /// This is an exclusive OR operator. Shouldn't be confused with power!
        /// </summary>
        /// <returns>A node</returns>
        public static Entity operator ^(Entity a, Entity b) => new Xorf(a, b);

        /// <returns>A node</returns>
        public Entity Xor(Entity another) => new Xorf(this, another);

        /// <returns>A node</returns>
        public Entity Implies(Entity conclusion) => new Impliesf(this, conclusion);

        /// <returns>A node</returns>
        public Entity Equalizes(Entity another) => HangOperator(this, another, (a, b) => new Equalsf(this, another));

        /// <returns>A node</returns>
        public static Entity operator >(Entity a, Entity b) => HangOperator(a, b, (a, b) => new Greaterf(a, b));

        /// <returns>A node</returns>
        public static Entity operator <(Entity a, Entity b) => HangOperator(a, b, (a, b) => new Lessf(a, b));

        /// <returns>A node</returns>
        public static Entity operator >=(Entity a, Entity b) => HangOperator(a, b, (a, b) => new GreaterOrEqualf(a, b));

        /// <returns>A node</returns>
        public static Entity operator <=(Entity a, Entity b) => HangOperator(a, b, (a, b) => new LessOrEqualf(a, b));

        internal static Entity HangOperator(Entity a, Entity b, Func<Entity, Entity, Entity> ctor)
           => a switch
           {
               Greaterf(var left, var right) => new Greaterf(left, right) & ctor(right, b),
               GreaterOrEqualf(var left, var right) => new GreaterOrEqualf(left, right) & ctor(right, b),
               Lessf(var left, var right) => new Lessf(left, right) & ctor(right, b),
               LessOrEqualf(var left, var right) => new LessOrEqualf(left, right) & ctor(right, b),

               _ => ctor(a, b)
           };
    }
}
