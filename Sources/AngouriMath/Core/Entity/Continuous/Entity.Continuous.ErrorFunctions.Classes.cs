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
        /// The error function, <c>erf(z) = 2/sqrt(pi) int_0^z e^(-t^2) dt</c>, entire and odd.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Erff(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Erff New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The complementary error function, <c>erfc(z) = 1 - erf(z)</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Erfcf(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Erfcf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// The imaginary error function, <c>erfi(z) = -i erf(i z)</c>, which on the real line is
        /// <c>2/sqrt(pi) int_0^x e^(t^2) dt</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/1501</remarks>
        public sealed partial record Erfif(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Erfif New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
