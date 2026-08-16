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
        /// A node of floor: the greatest integer not above the argument.
        /// </summary>
        /// <remarks>
        /// On a complex argument it is taken componentwise, so that
        /// <c>floor(3/2 + 5/2i)</c> is <c>1 + 2i</c>. That is what SymPy and Mathematica
        /// both do, and it is the only reading under which <c>floor</c> of a real number
        /// keeps its meaning when the imaginary part happens to be zero.
        /// </remarks>
        public sealed partial record Floorf(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Floorf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of ceil: the least integer not below the argument.
        /// </summary>
        /// <remarks>
        /// Componentwise on a complex argument, for the same reason as
        /// <see cref="Floorf"/>.
        /// </remarks>
        public sealed partial record Ceilf(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Ceilf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
