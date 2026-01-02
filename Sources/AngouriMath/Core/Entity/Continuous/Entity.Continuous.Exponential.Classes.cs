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
        /// A node of exponential (power)
        /// </summary>
        public sealed partial record Powf(Entity Base, Entity Exponent) : Function, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Powf New(Entity @base, Entity exponent) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Exponent, exponent) ? this : new(@base, exponent);
            internal override Priority Priority => Priority.Pow;

            public Entity NodeFirstChild => Base;

            public Entity NodeSecondChild => Exponent;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Exponent.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Base, Exponent };
        }

        /// <summary>
        /// A node of logarithm
        /// </summary>
        public sealed partial record Logf(Entity Base, Entity Antilogarithm) : Function, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Logf New(Entity @base, Entity antilogarithm) =>
                ReferenceEquals(Base, @base) && ReferenceEquals(Antilogarithm, antilogarithm) ? this : new(@base, antilogarithm);

            public Entity NodeFirstChild => Base;

            public Entity NodeSecondChild => Antilogarithm;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Base.Replace(func), Antilogarithm.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Base, Antilogarithm };
        }

#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
