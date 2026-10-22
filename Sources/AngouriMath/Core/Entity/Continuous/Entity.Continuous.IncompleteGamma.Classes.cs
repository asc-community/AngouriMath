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
        /// The upper incomplete gamma function, <c>Gamma(a, z) = int_z^oo t^(a - 1) e^(-t) dt</c>, with the principal branch
        /// of <c>z^a</c>: the cut is the negative real axis.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/2075</remarks>
        public sealed partial record IncompleteGammaf(Entity Parameter, Entity LowerLimit) : Function, IBinaryNode
        {
            private IncompleteGammaf New(Entity parameter, Entity lowerLimit) =>
                ReferenceEquals(Parameter, parameter) && ReferenceEquals(LowerLimit, lowerLimit) ? this : new(parameter, lowerLimit) { Codomain = Codomain };

            /// <inheritdoc/>
            public Entity NodeFirstChild => Parameter;

            /// <inheritdoc/>
            public Entity NodeSecondChild => LowerLimit;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Parameter.Replace(func), LowerLimit.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Parameter, LowerLimit };
        }
    }
}
