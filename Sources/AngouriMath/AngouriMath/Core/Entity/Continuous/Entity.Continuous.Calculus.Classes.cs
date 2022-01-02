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
        // Iterations should be refactored? to be int instead of Entity
        /// <summary>
        /// A node of derivative
        /// </summary>
        public sealed partial record Derivativef(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Derivativef New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        /// <summary>
        /// A node of integral
        /// </summary>
        public sealed partial record Integralf(Entity Expression, Entity Var, int Iterations) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Integralf New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        /// <summary>
        /// A node of limit
        /// </summary>
        public sealed partial record Limitf(Entity Expression, Entity Var, Entity Destination, ApproachFrom ApproachFrom) : Function
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Limitf New(Entity expression, Entity var, Entity destination, ApproachFrom approachFrom) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Destination, destination)
                && ApproachFrom == approachFrom ? this : new(expression, var, destination, approachFrom);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func), Destination.Replace(func), ApproachFrom));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Destination };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
