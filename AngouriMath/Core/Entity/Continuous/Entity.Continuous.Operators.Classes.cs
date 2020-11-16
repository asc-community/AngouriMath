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
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591  // only while records' parameters cannot be documented
        /// <summary>
        /// A node of sum
        /// </summary>
        public sealed partial record Sumf(Entity Augend, Entity Addend) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sumf New(Entity augend, Entity addend) =>
                ReferenceEquals(Augend, augend) && ReferenceEquals(Addend, addend) ? this : new(augend, addend);
            internal override Priority Priority => Priority.Sum;
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Augend.Replace(func), Addend.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Augend, Addend };
            /// <summary>
            /// Gathers linear children of a sum, e.g.
            /// <code>1 + (x - a/2) + b - 4</code>
            /// would return
            /// <code>{ 1, x, (-1) * a / 2, b, (-1) * 4 }</code>
            /// </summary>
            internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
            {
                Sumf(var augend, var addend) => LinearChildren(augend).Concat(LinearChildren(addend)),
                Minusf(var subtrahend, var minuend) =>
                    LinearChildren(subtrahend).Concat(LinearChildren(minuend).Select(entity => -1 * entity)),
                _ => new[] { tree }
            };
        }

        /// <summary>
        /// A node of difference
        /// </summary>
        public sealed partial record Minusf(Entity Subtrahend, Entity Minuend) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Minusf New(Entity subtrahend, Entity minuend) =>
                ReferenceEquals(Subtrahend, subtrahend) && ReferenceEquals(Minuend, minuend) ? this : new(subtrahend, minuend);
            internal override Priority Priority => Priority.Minus;
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Subtrahend.Replace(func), Minuend.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Subtrahend, Minuend };
        }

        /// <summary>
        /// A node of product
        /// </summary>
        public sealed partial record Mulf(Entity Multiplier, Entity Multiplicand) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Mulf New(Entity multiplier, Entity multiplicand) =>
                ReferenceEquals(Multiplier, multiplier) && ReferenceEquals(Multiplicand, multiplicand) ? this : new(multiplier, multiplicand);
            internal override Priority Priority => Priority.Mul;
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Multiplier.Replace(func), Multiplicand.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Multiplier, Multiplicand };
            /// <summary>
            /// Gathers linear children of a product, e.g.
            /// <code>1 * (x / a^2) * b / 4</code>
            /// would return
            /// <code>{ 1, x, (a^2)^(-1), b, 4^(-1) }</code>
            /// </summary>
            internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
            {
                Mulf(var multiplier, var multiplicand) => LinearChildren(multiplier).Concat(LinearChildren(multiplicand)),
                Divf(var dividend, var divisor) =>
                    LinearChildren(dividend).Concat(LinearChildren(divisor).Select(entity => new Powf(entity, -1))),
                _ => new[] { tree }
            };
        }

        /// <summary>
        /// A node of division
        /// </summary>
        public sealed partial record Divf(Entity Dividend, Entity Divisor) : NumericNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Divf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
            internal override Priority Priority => Priority.Div;
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
