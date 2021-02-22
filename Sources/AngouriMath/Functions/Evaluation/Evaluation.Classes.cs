/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Functions;
using System;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public partial record Variable
        {
            // TODO: When target-typed conditional expression lands, remove the explicit conversion
            /// <inheritdoc/>
            protected override Entity InnerEval() => ConstantList.TryGetValue(this, out var value) ? (Entity)value : this;

            /// <inheritdoc/>
            protected override Entity InnerSimplify() => this;
        }
        public partial record Tensor
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() => Elementwise(e => e.Evaled);

            /// <inheritdoc/>
            protected override Entity InnerSimplify() => Elementwise(e => e.InnerSimplified);
        }

        /// <summary>
        /// For two-argument nodes
        /// Used in InnerSimplify and InnerEval
        /// Allows to avoid looking over all the combinations with piecewise, tensor, finiteset
        /// </summary>
        /// <param name="left">
        /// Left argument
        /// </param>
        /// <param name="right">
        /// Right argument
        /// </param>
        /// <param name="operation">
        /// That is the main switch for the types. It must return null if no suitable couple of types is found,
        /// so that the method could move on to the matrix choice
        /// </param>
        /// <param name="defaultCtor">
        /// If no suitable case in switch found, it should return the default node, for example, for sum it would be
        /// <code>(a, b) => a + b</code>
        /// </param>
        private Entity ExpandOnTwoArguments(Entity left, Entity right, Func<Entity, Entity, Entity?> operation, Func<Entity, Entity, Entity> defaultCtor, bool checkIfExactEvaled = false)
        {
            left = left.Unpack1();
            right = right.Unpack1();
            if (operation(left, right) is { } preRes)
                return preRes;

            if (checkIfExactEvaled && this is Number { IsExact: true } n)
                return n;

            Entity ops(Entity a, Entity b)
            {
                if (operation(a, b) is { } res)
                    return res;
                if (checkIfExactEvaled && this is Number { IsExact: true } n)
                    return n;
                return defaultCtor(a, b);
            }

            return (left, right) switch
            {
                (Tensor a, Tensor b) => a.Elementwise(b, ops),
                (Tensor a, var b) => a.Elementwise(a => ops(a, b)),
                (var a, Tensor b) => b.Elementwise(b => ops(a, b)),
                (FiniteSet a, var b) => a.Apply(a => ops(a, b)),
                (var a, FiniteSet b) => b.Apply(b => ops(a, b)),
                _ => ReferenceEquals(left, this.DirectChildren[0]) &&
                     ReferenceEquals(right, this.DirectChildren[1]) ?
                     this : defaultCtor(left, right)
            };
        }

        private Entity ExpandOnOneArgument(Entity expr, Func<Entity, Entity?> operation, Func<Entity, Entity> defaultCtor, bool checkIfExactEvaled = false)
        {
            expr = expr.Unpack1();
            if (operation(expr) is { } notNull)
                return notNull;

            if (checkIfExactEvaled && this is Number { IsExact: true } n)
                return n;

            Entity ops(Entity a)
            {
                if (operation(a) is { } res)
                    return res;
                if (checkIfExactEvaled && this is Number { IsExact: true } n)
                    return n;
                return defaultCtor(a);
            }

            return expr switch
            {
                Tensor t => t.Elementwise(ops),
                FiniteSet s => s.Apply(ops),
                _ => ReferenceEquals(expr, this.DirectChildren[0]) ? this : defaultCtor(expr)
            };
        }

        private Entity ExpandOnTwoAndTArguments<T>(Entity left, Entity right, T third, Func<Entity, Entity, T, Entity?> operation, Func<Entity, Entity, T, Entity> defaultCtor, bool checkIfExactEvaled = false)
        {
            left = left.Unpack1();
            right = right.Unpack1();
            if (operation(left, right, third) is { } preRes)
                return preRes;

            if (checkIfExactEvaled && this is Number { IsExact: true } n)
                return n;

            Entity ops(Entity a, Entity b)
            {
                if (operation(a, b, third) is { } res)
                    return res;
                if (checkIfExactEvaled && this is Number { IsExact: true } n)
                    return n;
                return defaultCtor(left, right, third);
            }

            return (left, right, third) switch
            {
                (Tensor a, Tensor b, _) => a.Elementwise(b, ops),
                (Tensor a, var b, _) => a.Elementwise(a => ops(a, b)),
                (var a, Tensor b, _) => b.Elementwise(b => ops(a, b)),
                (FiniteSet a, var b, _) => a.Apply(a => ops(a, b)),
                (var a, FiniteSet b, _) => b.Apply(b => ops(a, b)),
                _ => ReferenceEquals(left, this.DirectChildren[0]) &&
                     ReferenceEquals(right, this.DirectChildren[1]) ?
                     this : defaultCtor(left, right, third)
            };
        }
    }
}