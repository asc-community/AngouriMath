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
        /// <summary>
        /// A node of arcsine
        /// </summary>
        public sealed partial record Arcsinf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arcsinf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccosine
        /// </summary>
        public sealed partial record Arccosf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arccosf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arctangent
        /// </summary>
        public sealed partial record Arctanf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arctanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccotangent
        /// </summary>
        public sealed partial record Arccotanf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            Arccotanf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arcsecant
        /// </summary>
        public sealed partial record Arcsecantf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arcsecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of arccosecant
        /// </summary>
        public sealed partial record Arccosecantf(Entity Argument) : TrigonometricFunction, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Arccosecantf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
