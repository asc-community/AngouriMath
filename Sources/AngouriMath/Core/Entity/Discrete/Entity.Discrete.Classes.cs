//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters

        /// <summary>
        /// This node represents all possible values a boolean node might be of
        /// </summary>
        public sealed partial record Boolean(bool Value) : Statement
        {
            /// <summary>
            /// One of the Boolean's state, which also behaves as Entity
            /// That is, hangable
            /// </summary>
            [ConstantField] public static readonly Boolean True = new Boolean(true);

            /// <summary>
            /// One of the Boolean's state, which also behaves as Entity
            /// That is, hangable
            /// </summary>
            [ConstantField] public static readonly Boolean False = new Boolean(false);

            /// <summary>
            /// This conversation is 100% free, no need to manually choose between
            /// <see cref="True"/> and <see cref="False"/>
            /// </summary>
            /// <param name="b">To convert from</param>
            public static implicit operator bool(Boolean b) => b.Value;

            /// <summary>
            /// Creates a Boolean at 0 cost
            /// No need to manually choose between <see cref="True"/> and <see cref="False"/>
            /// </summary>
            /// <param name="value">
            /// From which to create
            /// </param>
            public static Boolean Create(bool value) => value ? True : False; // to avoid reallocation

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) 
                => func(this);
            internal override Priority Priority => Priority.Leaf;
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Array.Empty<Entity>();

            /// <summary>
            /// Use this when parsing one boolean value
            /// </summary>
            /// <param name="expr">A string to parse from</param>
            /// <param name="dst">Where to store the result</param>
            /// <returns>
            /// true if the parsing completed successfully, 
            /// false otherwise
            /// </returns>
            public static bool TryParse(string expr, out Boolean dst)
            {
                switch (expr.ToLower())
                {
                    case "false":
                        dst = False;
                        return true;
                    case "true":
                        dst = True;
                        return true;
                }
                dst = False;
                return false;
            }

            /// <summary>
            /// Unlike <see cref="TryParse"/> this will throw a
            /// <see cref="ParseException"/> if parsing is not successful
            /// </summary>
            public static Boolean Parse(string expr)
                => TryParse(expr, out var res) ? res : throw new CannotParseInstanceException(typeof(Boolean), expr);
        }

        #region Logical gates

        /// <summary>
        /// Whatever its argument is, the result will be inverted
        /// </summary>
        public sealed partial record Notf(Entity Argument) : Statement, IUnaryNode
        {
            internal override Priority Priority => Priority.Negation;

            public Entity NodeChild => Argument;

            private Notf New(Entity negated) =>
                ReferenceEquals(Argument, negated) ? this : new(negated);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };

        }

        /// <summary>
        /// Is true iff both operands are true
        /// </summary>
        public sealed partial record Andf(Entity Left, Entity Right) : Statement, IBinaryNode
        {
            internal override Priority Priority => Priority.Conjunction;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            private Andf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// Is true iff at least one operand is true,
        /// </summary>
        public sealed partial record Orf(Entity Left, Entity Right) : Statement, IBinaryNode
        {
            internal override Priority Priority => Priority.Disjunction;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            private Orf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// Is true iff one operand is true
        /// </summary>
        public sealed partial record Xorf(Entity Left, Entity Right) : Statement, IBinaryNode
        {
            internal override Priority Priority => Priority.XDisjunction;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            private Xorf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// Is true iff assumption is false or conclusion is true
        /// </summary>
        public sealed partial record Impliesf(Entity Assumption, Entity Conclusion) : Statement, IBinaryNode
        {
            internal override Priority Priority => Priority.Impliciation;

            public Entity NodeFirstChild => Assumption;

            public Entity NodeSecondChild => Conclusion;

            private Impliesf New(Entity left, Entity right) =>
                ReferenceEquals(Assumption, left) && ReferenceEquals(Conclusion, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Assumption.Replace(func), Conclusion.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Assumption, Conclusion };
        }

        #endregion

        #region Equality/inequality operators

        /// <summary>
        /// It is true if left and right are equal
        /// </summary>
        public sealed partial record Equalsf(Entity Left, Entity Right) : ComparisonSign, IBinaryNode
        {
            internal override Priority Priority => Priority.Equal;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            internal Equalsf New(Entity left, Entity right)
                => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new Equalsf(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// It is true iff both parts are numeric and real, and left number is greater
        /// than the right one
        /// It is false iff both parts are numeric and real, and left number is less or equal 
        /// the right one
        /// It is NaN/unsimplified otherwise.
        /// </summary>
        public sealed partial record Greaterf(Entity Left, Entity Right) : ComparisonSign, IBinaryNode
        {
            internal override Priority Priority => Priority.GreaterThan;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            internal Greaterf New(Entity left, Entity right)
                => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// It is true iff both parts are numeric and real, and left number is greater
        /// than the right one or equal to it
        /// It is false iff both parts are numeric and real, and left number is less 
        /// the right one
        /// It is NaN/unsimplified otherwise.
        /// </summary>
        public sealed partial record GreaterOrEqualf(Entity Left, Entity Right) : ComparisonSign, IBinaryNode
        {
            internal override Priority Priority => Priority.GreaterThan;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            internal GreaterOrEqualf New(Entity left, Entity right)
                => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// It is true iff both parts are numeric and real, and left number is less
        /// than the right one
        /// It is false iff both parts are numeric and real, and left number is greater or equal 
        /// the right one
        /// It is NaN/unsimplified otherwise.
        /// </summary>
        public sealed partial record Lessf(Entity Left, Entity Right) : ComparisonSign, IBinaryNode
        {
            internal override Priority Priority => Priority.GreaterThan;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            internal Lessf New(Entity left, Entity right)
                => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        /// <summary>
        /// It is true iff both parts are numeric and real, and left number is less
        /// than the right one or equal to it
        /// It is false iff both parts are numeric and real, and left number is greater
        /// the right one
        /// It is NaN/unsimplified otherwise.
        /// </summary>
        public sealed partial record LessOrEqualf(Entity Left, Entity Right) : ComparisonSign, IBinaryNode
        {
            internal override Priority Priority => Priority.GreaterThan;

            public Entity NodeFirstChild => Left;

            public Entity NodeSecondChild => Right;

            internal LessOrEqualf New(Entity left, Entity right)
                => ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        #endregion

        #region Set statements
        partial record Set
        {
            /// <summary>
            /// This node represents whether the given element is in the set
            /// </summary>
            public sealed partial record Inf(Entity Element, Entity SupSet) : Statement, IBinaryNode
            {
                internal override Priority Priority => Priority.ContainsIn;

                public Entity NodeFirstChild => Element;

                public Entity NodeSecondChild => SupSet;

                internal Inf New(Entity element, Entity supSet)
                    => ReferenceEquals(Element, element) && ReferenceEquals(SupSet, supSet) ? this : new(element, supSet);
                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Element.Replace(func), SupSet.Replace(func)));
                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Element, SupSet };
            }
        }
        #endregion

        #region Number theory
        /// <summary>
        /// This node represents the Euler totient function (phi)
        /// </summary>
        public sealed partial record Phif(Entity Argument) : Function, IUnaryNode
        {
            public Entity NodeChild => Argument;

            internal Phif New(Entity argument)
                   => ReferenceEquals(argument, Argument) ? this : new(argument);

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));

            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }
        #endregion

#pragma warning restore CS1591 // TODO: it's only for records' parameters! Remove it once you can document records parameters
    }
}
