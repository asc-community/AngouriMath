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
        /// A node of round: the nearest integer, with a tie going to the even one.
        /// </summary>
        /// <remarks>
        /// Half to even, which is what Python, SymPy, Mathematica and IEEE 754 all do — and
        /// what .NET's <c>Math.Round</c> does by default, though not what most people expect
        /// of it. So <c>round(1/2)</c> is <c>0</c> and <c>round(3/2)</c> is <c>2</c>, and it
        /// is <b>not</b> <c>floor(x + 1/2)</c>, which differs at every tie.
        /// </remarks>
        public sealed partial record Roundf(Entity Argument) : Function, IUnaryNode
        {
            /// <inheritdoc/>
            public Entity NodeChild => Argument;

            private Roundf New(Entity arg) =>
                ReferenceEquals(Argument, arg) ? this : new(arg) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        /// <summary>
        /// A node of min: the lesser of its two arguments.
        /// </summary>
        /// <remarks>
        /// A node rather than sugar over <c>(a + b - abs(a - b)) / 2</c>. The closed form is
        /// what this simplifies <i>to</i> where that helps; it is not what it should print
        /// as, and the round-trip contract would make that permanent. SymPy keeps
        /// <c>Min</c> as a node for the same reason.
        /// Only ordered arguments compare, so a complex one is left alone rather than
        /// guessed at.
        /// </remarks>
        public sealed partial record Minf(Entity Left, Entity Right) : Function, IBinaryNode
        {
            /// <inheritdoc/>
            public Entity NodeFirstChild => Left;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Right;

            private Minf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// A node of max: the greater of its two arguments.
        /// </summary>
        /// <remarks>See <see cref="Minf"/> for why this is a node.</remarks>
        public sealed partial record Maxf(Entity Left, Entity Right) : Function, IBinaryNode
        {
            /// <inheritdoc/>
            public Entity NodeFirstChild => Left;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Right;

            private Maxf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// A node of gcd: the greatest common divisor of its two arguments.
        /// </summary>
        /// <remarks>
        /// Not integers only. SymPy's <c>gcd</c> is the polynomial gcd with the integer case
        /// as a special case, and this library already computes one — see
        /// <see cref="AngouriMath.Functions.PolynomialGcd"/>, which the
        /// <c>PolynomialGcdCancellation</c> rule set uses. What cannot be settled is left as
        /// this node rather than guessed at.
        /// </remarks>
        public sealed partial record Gcdf(Entity Left, Entity Right) : Function, IBinaryNode
        {
            /// <inheritdoc/>
            public Entity NodeFirstChild => Left;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Right;

            private Gcdf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }
    }
}
