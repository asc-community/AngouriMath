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
        /// The polygamma function <c>psi^(n)(z)</c>, the <c>(n + 1)</c>-th derivative of <c>ln Gamma(z)</c>, for a whole
        /// <c>n >= 0</c>; <c>psi^(0)</c> is the digamma function, <c>Gamma'(z)/Gamma(z)</c>. Its poles are
        /// <c>0, -1, -2, ...</c>.
        /// </summary>
        /// <remarks>https://github.com/asc-community/AngouriMath/issues/171</remarks>
        public sealed partial record Polygammaf(Entity Order, Entity Argument) : Function, IBinaryNode
        {
            private Polygammaf New(Entity order, Entity argument) =>
                ReferenceEquals(Order, order) && ReferenceEquals(Argument, argument) ? this : new(order, argument) { Codomain = Codomain };

            /// <inheritdoc/>
            public Entity NodeFirstChild => Order;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Argument;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Order.Replace(func), Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Order, Argument };
        }
    }
}
