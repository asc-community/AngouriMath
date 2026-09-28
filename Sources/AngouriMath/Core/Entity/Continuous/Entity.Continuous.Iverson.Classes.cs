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
        /// The Iverson bracket <c>iverson(P)</c>: <c>1</c> where the statement <c>P</c> holds and
        /// <c>0</c> where it does not.
        /// </summary>
        /// <remarks>
        /// It is how a count is written as a sum: <c>sum(iverson(k divides 12), k, 1, 12)</c> is
        /// the number of divisors of <c>12</c>. A named function rather than a bracket, since every
        /// bracket shape the grammar has already means something: a vector, a matrix, an interval.
        /// A statement that is not decided keeps the node, and so does a number, which is not a
        /// statement, as a connective keeps one.
        /// https://github.com/asc-community/AngouriMath/issues/1478
        /// </remarks>
        public sealed partial record Iversonf(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Iversonf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
    }
}
