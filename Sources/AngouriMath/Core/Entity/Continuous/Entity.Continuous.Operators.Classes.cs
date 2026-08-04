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
        /// A node of sum
        /// </summary>
        public sealed partial record Sumf(Entity Augend, Entity Addend) : ContinuousNode, IBinaryNode
        {
            /// <summary>
            /// Sums all the terms.
            /// </summary>
            public static Entity Sum(IReadOnlyList<Entity> terms)
                => TreeAnalyzer.MultiHangBinary(terms, (a, b) => a + b);

            /// <summary>
            /// Sums all the terms.
            /// </summary>
            public static Entity Sum(IEnumerable<Entity> terms)
                => Sum(terms.ToList());

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Sumf New(Entity augend, Entity addend) =>
                ReferenceEquals(Augend, augend) && ReferenceEquals(Addend, addend) ? this : new(augend, addend);
            internal override Priority Priority => Priority.Sum;

            public Entity NodeFirstChild => Augend;

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
            internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
            {
                Sumf(var augend, var addend) => LinearChildren(augend).Concat(LinearChildren(addend)),
                Minusf(var minuend, var subtrahend) =>
                    LinearChildren(minuend).Concat(LinearChildren(subtrahend).Select(entity => -1 * entity)),
                _ => new[] { tree }
            };
        }

        /// <summary>
        /// A node of difference
        /// </summary>
        public sealed partial record Minusf(Entity Minuend, Entity Subtrahend) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Minusf New(Entity minuend, Entity subtrahend) =>
                ReferenceEquals(Minuend, minuend) && ReferenceEquals(Subtrahend, subtrahend) ? this : new(minuend, subtrahend);
            internal override Priority Priority => Priority.Minus;

            public Entity NodeFirstChild => Minuend;

            public Entity NodeSecondChild => Subtrahend;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Minuend.Replace(func), Subtrahend.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Minuend, Subtrahend };
        }

        /// <summary>
        /// A node of product
        /// </summary>
        public sealed partial record Mulf(Entity Multiplier, Entity Multiplicand) : ContinuousNode, IBinaryNode
        {
            /// <summary>
            /// Multiplies all the terms.
            /// </summary>
            public static Entity Multiply(IReadOnlyList<Entity> terms)
                => TreeAnalyzer.MultiHangBinary(terms, (a, b) => a * b);

            /// <summary>
            /// Multiplies all the terms.
            /// </summary>
            public static Entity Multiply(IEnumerable<Entity> terms)
                => Multiply(terms.ToList());

            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Mulf New(Entity multiplier, Entity multiplicand) =>
                ReferenceEquals(Multiplier, multiplier) && ReferenceEquals(Multiplicand, multiplicand) ? this : new(multiplier, multiplicand);
            internal override Priority Priority => Priority.Mul;

            public Entity NodeFirstChild => Multiplier;

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
            internal static IEnumerable<Entity> LinearChildren(Entity tree) => tree switch
            {
                Mulf(var multiplier, var multiplicand) => LinearChildren(multiplier).Concat(LinearChildren(multiplicand)),
                Divf(var dividend, var divisor) =>
                    LinearChildren(dividend).Concat(LinearChildren(divisor).Select(entity => new Powf(entity, -1))),
                _ => new[] { tree }
            };
        }

        /// <summary>
        /// A node of division
        /// </summary>
        public sealed partial record Divf(Entity Dividend, Entity Divisor) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            internal Divf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
            internal override Priority Priority => Priority.Div;

            public Entity NodeFirstChild => Dividend;

            public Entity NodeSecondChild => Divisor;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }

        /// <summary>
        /// A node of modulus, that is, the remainder after division. Follows the sign of
        /// the dividend, as C#'s own <c>%</c> does and as
        /// PeterO.Numbers does underneath: -7 % 3 is -1,
        /// not 2.
        /// </summary>
        public sealed partial record Modf(Entity Dividend, Entity Divisor) : ContinuousNode, IBinaryNode
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            internal Modf New(Entity dividend, Entity divisor) =>
                ReferenceEquals(Dividend, dividend) && ReferenceEquals(Divisor, divisor) ? this : new(dividend, divisor);
            internal override Priority Priority => Priority.Mul;

            public Entity NodeFirstChild => Dividend;

            public Entity NodeSecondChild => Divisor;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Dividend.Replace(func), Divisor.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Dividend, Divisor };
        }
#pragma warning restore CS1591  // only while records' parameters cannot be documented
    }
}
