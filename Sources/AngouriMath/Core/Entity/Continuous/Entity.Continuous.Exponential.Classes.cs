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
        /// <summary>
        /// A node of exponential (power)
        /// </summary>
        public sealed partial record Powf(Entity Base, Entity Exponent) : Entity
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Powf New(Entity @base, Entity exponent) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Exponent, exponent) ? this : new(@base, exponent);
            internal override Priority Priority => Priority.Pow;
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Exponent.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Base, Exponent };
        }

        /// <summary>
        /// A node of logarithm
        /// </summary>
        public sealed partial record Logf(Entity Base, Entity Antilogarithm) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Logf New(Entity @base, Entity antilogarithm) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Antilogarithm, antilogarithm) ? this : new(@base, antilogarithm);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Antilogarithm.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
        }

#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
