
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using AngouriMath.Core;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// This node represents all possible values a boolean node might be of
        /// </summary>
        public sealed partial record Boolean(bool Value) : BooleanNode
        {
            public static readonly Boolean True = new Boolean(true);
            public static readonly Boolean False = new Boolean(false);
            public static implicit operator bool(Boolean b) => b.Value;
            public static Boolean Create(bool value) => value ? True : False; // to avoid reallocation

            public override Entity Replace(Func<Entity, Entity> func) 
                => this;
            public override Priority Priority => Priority.Number;
            protected override Entity[] InitDirectChildren() => new Entity[] { };
        }

        #region Operators

        /// <summary>
        /// Whatever its argument is, the result will be inverted
        /// </summary>
        public sealed partial record Notf(Entity Argument) : BooleanNode
        {
            public override Priority Priority => Priority.Negation;
            private Notf New(Entity negated) =>
                ReferenceEquals(Argument, negated) ? this : new(negated);
            public override Entity Replace(Func<Entity, Entity> func) => func(New(Argument.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Argument };
        }

        public sealed partial record Andf(Entity Left, Entity Right) : BooleanNode
        {
            public override Priority Priority => Priority.Conjunction;
            private Andf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        public sealed partial record Orf(Entity Left, Entity Right) : BooleanNode
        {
            public override Priority Priority => Priority.Disjunction;
            private Orf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        public sealed partial record Xorf(Entity Left, Entity Right) : BooleanNode
        {
            public override Priority Priority => Priority.XDisjunction;
            private Xorf New(Entity left, Entity right) =>
                ReferenceEquals(Left, left) && ReferenceEquals(Right, right) ? this : new(left, right);
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Left.Replace(func), Right.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Left, Right };
        }

        public sealed partial record Impliesf(Entity Assumption, Entity Conclusion) : BooleanNode
        {
            public override Priority Priority => Priority.Impliciation;
            private Impliesf New(Entity left, Entity right) =>
                ReferenceEquals(Assumption, left) && ReferenceEquals(Conclusion, right) ? this : new(left, right);
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Assumption.Replace(func), Conclusion.Replace(func)));
            protected override Entity[] InitDirectChildren() => new[] { Assumption, Conclusion };
        }

        #endregion
    }
}
