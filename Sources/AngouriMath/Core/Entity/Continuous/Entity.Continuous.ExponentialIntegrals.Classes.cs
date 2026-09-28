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
        /// The exponential integral, <c>Ei(z) = gamma + (ln z - ln(1/z))/2 + sum_(k >= 1) z^k/(k k!)</c>,
        /// which on the real line is the principal value of <c>int_-oo^x e^t/t dt</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Eif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Eif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The logarithmic integral, <c>li(z) = Ei(ln z)</c>, which on <c>x > 0</c> is the principal
        /// value of <c>int_0^x dt/ln t</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Lif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Lif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
