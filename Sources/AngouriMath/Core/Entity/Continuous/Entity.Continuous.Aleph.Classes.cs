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
        /// The aleph number <c>aleph(k)</c>, the size of an infinite set: <c>aleph(0)</c> is the
        /// size of the whole numbers, and <c>aleph(k + 1)</c> the least size after
        /// <c>aleph(k)</c>.
        /// </summary>
        /// <remarks>
        /// <c>card</c> of an infinite set is one of these, or a power of <c>2</c> of one:
        /// <c>card(ZZ)</c> is <c>aleph(0)</c> and <c>card(RR)</c> is <c>2^aleph(0)</c>, which is an
        /// aleph but not a named one -- that it is <c>aleph(1)</c> is the continuum hypothesis,
        /// which ZFC does not decide. A sum or a product of sizes is the larger of them, and the
        /// sizes compare.
        /// https://github.com/asc-community/AngouriMath/issues/1409
        /// </remarks>
        public sealed partial record Alephf(Entity Index) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Index;

            private Alephf New(Entity index) =>
                ReferenceEquals(Index, index) ? this : new(index) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Index.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Index };
        }
    }
}
