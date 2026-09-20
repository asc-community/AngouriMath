//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using System.Collections.Generic;
using HonkSharp.Laziness;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// A node of sum
        /// </summary>
        public sealed partial record Sumf(Entity Augend, Entity Addend) : ContinuousNode, IBinaryNode
        {
            /// <summary>
            /// Sums all the terms.
            /// </summary>
            /// <remarks>
            /// The empty sum is <c>0</c>. This takes a list from the caller and used to hand it
            /// to <c>MultiHangBinary</c> unchecked, whose precondition is genuine and whose
            /// refusal is an <c>AngouriBugException</c> — so an empty list asked the caller to
            /// report a bug against this repository.
            /// <a href="https://github.com/asc-community/AngouriMath/issues/1028">#1028</a>
            /// </remarks>
            public static Entity Sum(IReadOnlyList<Entity> terms)
                => terms.Count > 0
                    ? TreeAnalyzer.MultiHangBinary(terms, (a, b) => a + b)
                    : Number.Integer.Zero;

            /// <summary>
            /// Sums all the terms.
            /// </summary>
            public static Entity Sum(IEnumerable<Entity> terms)
                => Sum(terms.ToList());

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sumf New(Entity augend, Entity addend) =>
                ReferenceEquals(Augend, augend) && ReferenceEquals(Addend, addend) ? this : new(augend, addend) { Codomain = Codomain };
            internal override Priority Priority => Priority.Sum;

            /// <inheritdoc/>
            public Entity NodeFirstChild => Augend;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Addend;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Augend.Replace(func), Addend.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Augend, Addend };
            /// <summary>
            /// Gathers linear children of a sum, e.g.
            /// <code>1 + (x - a/2) + b - 4</code>
            /// would return
            /// <code>{ 1, x, (-1) * a / 2, b, (-1) * 4 }</code>
            /// </summary>
            /// <remarks>
            /// Cached on the node it is asked of, since a node is immutable and the flattening
            /// is asked for many times over: every rule that reads a sum as a list reads it
            /// again for each candidate. A nested sum's list is built from its parts' lists,
            /// which are cached on those parts, so the memory is linear in the tree and no
            /// enumeration is repeated. https://github.com/asc-community/AngouriMath/issues/224
            /// </remarks>
            internal static IReadOnlyList<Entity> LinearChildren(Entity tree) => tree switch
            {
                Sumf sum => sum.Linear,
                Minusf difference => difference.Linear,
                _ => new[] { tree }
            };

            private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Flatten(LinearChildren(@this.Augend), LinearChildren(@this.Addend)), this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;

            /// <summary>The two lists as one array.</summary>
            internal static Entity[] Flatten(IReadOnlyList<Entity> left, IReadOnlyList<Entity> right)
            {
                var flat = new Entity[left.Count + right.Count];
                for (var i = 0; i < left.Count; i++)
                    flat[i] = left[i];
                for (var i = 0; i < right.Count; i++)
                    flat[left.Count + i] = right[i];
                return flat;
            }
        }

        /// <summary>
        /// A node of difference
        /// </summary>
        public sealed partial record Minusf(Entity Minuend, Entity Subtrahend) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Minusf New(Entity minuend, Entity subtrahend) =>
                ReferenceEquals(Minuend, minuend) && ReferenceEquals(Subtrahend, subtrahend) ? this : new(minuend, subtrahend) { Codomain = Codomain };
            internal override Priority Priority => Priority.Minus;

            /// <inheritdoc/>
            public Entity NodeFirstChild => Minuend;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Subtrahend;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Minuend.Replace(func), Subtrahend.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Minuend, Subtrahend };

            /// <summary>The terms as a sum reads them: the subtrahend's terms each times -1. See <see cref="Sumf.LinearChildren"/>.</summary>
            internal IReadOnlyList<Entity> Linear => linear.GetValue(static @this =>
                {
                    var subtracted = Sumf.LinearChildren(@this.Subtrahend);
                    var negated = new Entity[subtracted.Count];
                    for (var i = 0; i < negated.Length; i++)
                        negated[i] = -1 * subtracted[i];
                    return Sumf.Flatten(Sumf.LinearChildren(@this.Minuend), negated);
                }, this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        /// <summary>
        /// A node of product
        /// </summary>
        public sealed partial record Mulf(Entity Multiplier, Entity Multiplicand) : ContinuousNode, IBinaryNode
        {
            /// <summary>
            /// Multiplies all the terms.
            /// </summary>
            /// <remarks>
            /// The empty product is <c>1</c>, for the reason the empty sum is <c>0</c> — see
            /// <see cref="Sumf.Sum(System.Collections.Generic.IReadOnlyList{Entity})"/>.
            /// </remarks>
            public static Entity Multiply(IReadOnlyList<Entity> terms)
                => terms.Count > 0
                    ? TreeAnalyzer.MultiHangBinary(terms, (a, b) => a * b)
                    : Number.Integer.One;

            /// <summary>
            /// Multiplies all the terms.
            /// </summary>
            public static Entity Multiply(IEnumerable<Entity> terms)
                => Multiply(terms.ToList());

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Mulf New(Entity multiplier, Entity multiplicand) =>
                ReferenceEquals(Multiplier, multiplier) && ReferenceEquals(Multiplicand, multiplicand) ? this : new(multiplier, multiplicand) { Codomain = Codomain };
            internal override Priority Priority => Priority.Mul;

            /// <inheritdoc/>
            public Entity NodeFirstChild => Multiplier;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Multiplicand;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Multiplier.Replace(func), Multiplicand.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Multiplier, Multiplicand };
            /// <summary>
            /// Gathers linear children of a product, e.g.
            /// <code>1 * (x / a^2) * b / 4</code>
            /// would return
            /// <code>{ 1, x, (a^2)^(-1), b, 4^(-1) }</code>
            /// </summary>
            /// <remarks>Cached on the node, as <see cref="Sumf.LinearChildren"/> is.</remarks>
            internal static IReadOnlyList<Entity> LinearChildren(Entity tree) => tree switch
            {
                Mulf product => product.Linear,
                Divf quotient => quotient.Linear,
                _ => new[] { tree }
            };

            private IReadOnlyList<Entity> Linear => linear.GetValue(static @this => Sumf.Flatten(LinearChildren(@this.Multiplier), LinearChildren(@this.Multiplicand)), this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        /// <summary>
        /// A node of division
        /// </summary>
        public sealed partial record Divf(Entity Dividend, Entity Divisor) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            internal Divf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor) { Codomain = Codomain };
            internal override Priority Priority => Priority.Div;

            /// <inheritdoc/>
            public Entity NodeFirstChild => Dividend;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Divisor;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };

            /// <summary>The factors as a product reads them: the divisor's factors each to the power -1. See <see cref="Mulf.LinearChildren"/>.</summary>
            internal IReadOnlyList<Entity> Linear => linear.GetValue(static @this =>
                {
                    var divided = Mulf.LinearChildren(@this.Divisor);
                    var inverted = new Entity[divided.Count];
                    for (var i = 0; i < inverted.Length; i++)
                        inverted[i] = new Powf(divided[i], -1);
                    return Sumf.Flatten(Mulf.LinearChildren(@this.Dividend), inverted);
                }, this);
            private LazyPropertyA<IReadOnlyList<Entity>> linear;
        }

        /// <summary>
        /// A node of modulus, that is, the floored remainder after division,
        /// <c>a - b * floor(a / b)</c>, which takes the sign of the <em>divisor</em>:
        /// <c>(-7) mod 3</c> is <c>2</c> and <c>7 mod (-3)</c> is <c>-2</c>, the convention of
        /// SymPy, Mathematica and Maxima and the one under which the residues modulo <c>n</c>
        /// are <c>0</c> to <c>n - 1</c>. Not C#'s <c>%</c>, which truncates; this comment said
        /// the opposite of what the evaluation and the documentation do.
        /// </summary>
        public sealed partial record Modf(Entity Dividend, Entity Divisor) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            internal Modf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor) { Codomain = Codomain };
            internal override Priority Priority => Priority.Mul;

            /// <inheritdoc/>
            public Entity NodeFirstChild => Dividend;

            /// <inheritdoc/>
            public Entity NodeSecondChild => Divisor;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }
    }
}
