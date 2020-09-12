
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


namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// This class describes every node whose value is boolean, that is, true or false
        /// </summary>
        public abstract partial record BooleanNode : Entity
        {
            
        }

        public static implicit operator Entity(bool v) => Boolean.Create(v);
        public static Entity operator !(Entity a) => new Notf(a);
        public static Entity operator &(Entity a, Entity b) => new Andf(a, b);
        public static Entity operator |(Entity a, Entity b) => new Orf(a, b);

        // TODO: do we need this operator? Won't it be confused with power?
        //public static Entity operator ^(Entity a, Entity b) => new Xorf(a, b);
        public Entity Xor(Entity another) => new Xorf(this, another);
        public Entity Implies(Entity conclusion) => new Impliesf(this, conclusion);
    }
}
