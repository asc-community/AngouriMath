/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using System;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591  // only while records' parameters cannot be documented
        // Iterations should be refactored? to be int instead of Entity
        /// <summary>
        /// A node of derivative
        /// </summary>
        public partial record Derivativef(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Derivativef New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        /// <summary>
        /// A node of integral
        /// </summary>
        public partial record Integralf(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Integralf New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        /// <summary>
        /// A node of limit
        /// </summary>
        public partial record Limitf(Entity Expression, Entity Var, Entity Destination, ApproachFrom ApproachFrom) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Limitf New(Entity expression, Entity var, Entity destination, ApproachFrom approachFrom) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Destination, destination)
                && ApproachFrom == approachFrom ? this : new(expression, var, destination, approachFrom);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func), Destination.Replace(func), ApproachFrom));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Destination };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
