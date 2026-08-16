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
        /// <summary>
        /// A node of derivative
        /// </summary>
        /// <remarks>
        /// Negative iterations convert to integrals.
        /// </remarks>
        public sealed partial record Derivativef(Entity Expression, Entity Var, int Iterations) : CalculusOperator(Expression, Var)
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Derivativef New(Entity expression, Entity var) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                ? this : new(expression, var, Iterations) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Iterations };
        }

        /// <summary>
        /// A node of integral
        /// </summary>
        public sealed partial record Integralf(Entity Expression, Entity Var, (Entity from, Entity to)? Range) : CalculusOperator(Expression, Var)
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Integralf New(Entity expression, Entity var, (Entity from, Entity to)? range) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                && (range is null && Range is null || range is var (newFrom, newTo) && Range is var (oldFrom, oldTo)
                    && ReferenceEquals(newFrom, oldFrom) && ReferenceEquals(newTo, oldTo))
                ? this : new(expression, var, range) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var, Range is var (from, to) ? (from.Replace(func), to.Replace(func)) : null));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Range is var (from, to) ? [Expression, Var, from, to] : [Expression, Var];
        }

        /// <summary>
        /// A summation over a bound variable: <c>sum(f, i, from, to)</c> is
        /// <c>f(from) + … + f(to)</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="CalculusOperator"/> rather than a <see cref="Sumf"/> of many terms,
        /// because the bounds may be symbolic — <c>sum(i, i, 1, n)</c> has no finite expansion
        /// and is still a well-formed expression. Where they are concrete integers it expands,
        /// which is what makes the common case useful without the general case being wrong.
        /// </para>
        /// <para>
        /// The index is <b>bound</b>: it is the summation index, not a free variable of
        /// the expression, so substituting it from outside must not reach inside. That is the
        /// property <c>ConditionalSet</c> already has and the one
        /// <a href="https://github.com/asc-community/AngouriMath/issues/878">#878</a> was about
        /// getting wrong.
        /// </para>
        /// <para><a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a></para>
        /// </remarks>
        public sealed partial record Summationf(Entity Expression, Entity Var, Entity From, Entity To) : CalculusOperator(Expression, Var)
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Summationf New(Entity expression, Entity var, Entity from, Entity to) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                && ReferenceEquals(From, from) && ReferenceEquals(To, to)
                ? this : new(expression, var, from, to) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var, From.Replace(func), To.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, From, To };
        }

        /// <summary>
        /// A product over a bound variable: <c>product(f, i, from, to)</c> is
        /// <c>f(from) · … · f(to)</c>. See <see cref="Summationf"/>, which it mirrors exactly.
        /// </summary>
        /// <remarks><a href="https://github.com/asc-community/AngouriMath/issues/248">#248</a></remarks>
        public sealed partial record Productf(Entity Expression, Entity Var, Entity From, Entity To) : CalculusOperator(Expression, Var)
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Productf New(Entity expression, Entity var, Entity from, Entity to) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var)
                && ReferenceEquals(From, from) && ReferenceEquals(To, to)
                ? this : new(expression, var, from, to) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var, From.Replace(func), To.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, From, To };
        }

        /// <summary>
        /// A node of limit
        /// </summary>
        public sealed partial record Limitf(Entity Expression, Entity Var, Entity Destination, ApproachFrom ApproachFrom) : CalculusOperator(Expression, Var)
        {
            /// <summary>Reuse the cache by returning the same object if possible</summary>
            private Limitf New(Entity expression, Entity var, Entity destination, ApproachFrom approachFrom) =>
                ReferenceEquals(Expression, expression) && ReferenceEquals(Var, var) && ReferenceEquals(Destination, destination)
                && ApproachFrom == approachFrom ? this : new(expression, var, destination, approachFrom) { Codomain = Codomain };
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) =>
                func(New(Expression.Replace(func), Var.Replace(func), Destination.Replace(func), ApproachFrom));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Var, Destination };
        }
    }
}
