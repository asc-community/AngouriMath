//
// Copyright (c) 2019-2026 Angouri.
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
        /// The sine integral, <c>Si(z) = int_0^z sin(t)/t dt</c>, entire and odd.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Sif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Sif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The cosine integral, <c>Ci(z) = gamma + ln z + int_0^z (cos t - 1)/t dt</c>, with the cut of <c>ln</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Cif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Cif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The hyperbolic sine integral, <c>Shi(z) = int_0^z sinh(t)/t dt</c>, entire and odd.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Shif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Shif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The hyperbolic cosine integral, <c>Chi(z) = gamma + ln z + int_0^z (cosh t - 1)/t dt</c>, with the cut of <c>ln</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Chif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Chif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
