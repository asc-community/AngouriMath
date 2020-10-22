/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// This node represents all possible values a boolean node might be of
        /// </summary>
        public sealed partial record Boolean(bool Value) : Statement
        {
            /// <summary>
            /// One of the Boolean's state, which also behaves as Entity
            /// That is, hangable
            /// </summary>
            public static readonly Boolean True = new Boolean(true);

            /// <summary>
            /// One of the Boolean's state, which also behaves as Entity
            /// That is, hangable
            /// </summary>
            public static readonly Boolean False = new Boolean(false);

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
                switch (expr)
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
        public sealed partial record Notf(Entity Argument) : Statement
        {
            internal override Priority Priority => Priority.Negation;
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
        public sealed partial record Andf(Entity Left, Entity Right) : Statement
        {
            internal override Priority Priority => Priority.Conjunction;
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
        public sealed partial record Orf(Entity Left, Entity Right) : Statement
        {
            internal override Priority Priority => Priority.Disjunction;
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
        public sealed partial record Xorf(Entity Left, Entity Right) : Statement
        {
            internal override Priority Priority => Priority.XDisjunction;
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
        public sealed partial record Impliesf(Entity Assumption, Entity Conclusion) : Statement
        {
            internal override Priority Priority => Priority.Impliciation;
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
        public sealed partial record Equalsf(Entity Left, Entity Right) : ComparisonSign
        {
            internal override Priority Priority => Priority.Equal;
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
        public sealed partial record Greaterf(Entity Left, Entity Right) : ComparisonSign
        {
            internal override Priority Priority => Priority.GreaterThan;
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
        public sealed partial record GreaterOrEqualf(Entity Left, Entity Right) : ComparisonSign
        {
            internal override Priority Priority => Priority.GreaterThan;
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
        public sealed partial record Lessf(Entity Left, Entity Right) : ComparisonSign
        {
            internal override Priority Priority => Priority.GreaterThan;
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
        public sealed partial record LessOrEqualf(Entity Left, Entity Right) : ComparisonSign
        {
            internal override Priority Priority => Priority.GreaterThan;
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
            public sealed partial record Inf(Entity Element, Entity SupSet) : Statement
            {
                internal override Priority Priority => Priority.ContainsIn;
                internal Inf New(Entity element, Entity supSet)
                    => ReferenceEquals(Element, element) && ReferenceEquals(SupSet, supSet) ? this : new(Element, SupSet);
                /// <inheritdoc/>
                public override Entity Replace(Func<Entity, Entity> func)
                    => func(New(Element.Replace(func), SupSet.Replace(func)));
                /// <inheritdoc/>
                protected override Entity[] InitDirectChildren() => new[] { Element, SupSet };
            }
        }
        #endregion
    }
}
