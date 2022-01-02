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
        /// A node of factorial
        /// </summary>
        public sealed partial record Factorialf(Entity Argument) : Function, IUnaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Factorialf New(Entity argument) => ReferenceEquals(Argument, argument) ? this : new(argument);
            // This is still a function for pattern replacement
            internal override Priority Priority => Priority.Factorial;

            public Entity NodeChild => Argument;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
