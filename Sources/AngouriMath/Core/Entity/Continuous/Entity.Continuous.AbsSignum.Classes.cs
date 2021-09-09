using System;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591  // only while records' parameters cannot be documented
        /// <summary>
        /// A node of signum
        /// </summary>
        public sealed partial record Signumf(Entity Argument) : Function, IUnaryNode
        {
            public Entity NodeChild => Argument;

            private Signumf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of abs
        /// </summary>
        public sealed partial record Absf(Entity Argument) : Function, IUnaryNode
        {
            public Entity NodeChild => Argument;

            private Absf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
