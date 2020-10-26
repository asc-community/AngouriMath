/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// A node of sine
        /// </summary>
        public partial record Sinf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of cosine
        /// </summary>
        public partial record Cosf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of tangent
        /// </summary>
        public partial record Tanf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Tanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of cotangent
        /// </summary>
        public partial record Cotanf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of secant
        /// </summary>
        public partial record Secantf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Secantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of secant
        /// </summary>
        public partial record Cosecantf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cosecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
