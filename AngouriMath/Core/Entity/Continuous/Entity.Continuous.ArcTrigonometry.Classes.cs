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
#pragma warning disable CS1591  // only while records' parameters cannot be documented
        /// <summary>
        /// A node of arcsine
        /// </summary>
        public partial record Arcsinf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arcsinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccosine
        /// </summary>
        public partial record Arccosf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arccosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arctangent
        /// </summary>
        public partial record Arctanf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arctanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccotangent
        /// </summary>
        public partial record Arccotanf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Arccotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arcsecant
        /// </summary>
        public partial record Arcsecantf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arcsecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccosecant
        /// </summary>
        public partial record Arccosecantf(Entity Argument) : TrigonometricFunction
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arccosecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
