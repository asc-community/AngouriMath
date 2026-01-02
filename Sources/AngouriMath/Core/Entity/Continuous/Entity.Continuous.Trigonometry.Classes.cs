//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591  // only while records' parameters cannot be documented
        /// <summary>
        /// A node of sine
        /// </summary>
        public sealed partial record Sinf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

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
        public sealed partial record Cosf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

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
        public sealed partial record Tanf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

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
        public sealed partial record Cotanf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

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
        public sealed partial record Secantf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

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
        public sealed partial record Cosecantf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Cosecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
