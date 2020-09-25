
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
        /// =, <, >, <=, >=
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

        public static Entity HangOperator(Entity a, Entity b, Func<Entity, Entity, Entity> ctor)
           => a switch
           {
               Equalsf(var left, var right) => new Equalsf(left, right) & ctor(right, b),
               Greaterf(var left, var right) => new Greaterf(left, right) & ctor(right, b),
               GreaterOrEqualf(var left, var right) => new GreaterOrEqualf(left, right) & ctor(right, b),
               Lessf(var left, var right) => new Lessf(left, right) & ctor(right, b),
               LessOrEqualf(var left, var right) => new LessOrEqualf(left, right) & ctor(right, b),

               _ => ctor(a, b)
           };
    }
}
