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
using System.Collections.Generic;
using System.Linq;
using AngouriMath;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591 // TODO: add docs for records' arguments
        /// <summary>
        /// That is a node which equals Expression if Predicate is true, otherwise <see cref="MathS.NaN"/>
        /// </summary>
        public sealed partial record Providedf(Entity Expression, Entity Predicate) : Entity
        {
            internal Providedf New(Entity expression, Entity predicate)
                => ReferenceEquals(expression, Expression) && ReferenceEquals(predicate, Predicate) ? this :
                new Providedf(expression, predicate);

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(func(Expression), func(Predicate)));

            internal override Priority Priority => Priority.Provided;

            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => new[] { Expression, Predicate };
        }

        /// <summary>
        /// This is a node which defined on different subsets differently. When evaluating, it will turn
        /// into a particular case once all cases' predicates before are false, and this case's predicate is true.
        /// 
        /// That is, the order counts. An example:
        /// Piecewise(a provided false, b provided true, c provided true)
        /// Will be evaluated into b,
        /// 
        /// Piecewise(a provided false, b provided c, c provided false)
        /// Will remain as it is (although unreachable cases will be removed)
        /// </summary>
        public sealed partial record Piecewise : Entity
        {
            public IEnumerable<Providedf> Cases => cases;
            private IEnumerable<Providedf> cases = Enumerable.Empty<Providedf>();

            // internal override Priority Priority => Priority.Provided;
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Cases.Select(c => (c.Expression, c.Predicate)).ConcatTuples().ToArray();

            internal Piecewise New(IEnumerable<Providedf> newCases)
                => (Cases, newCases).SequencesAreEqualReferences() ? this : new Piecewise(newCases);

            /// <summary>
            /// Creates an instance of Piecewise
            /// </summary>
            /// <param name="cases">
            /// This is an ordered sequence of <see cref="Providedf"/>
            /// </param>
            public Piecewise(IEnumerable<Providedf> cases)
                => this.cases = cases;

            public Piecewise Apply(Func<Providedf, Providedf> func)
                => New(Cases.Select(func));

            internal override Priority Priority => Priority.Leaf;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Cases.Select(c => c.New(func(c.Expression), func(c.Predicate)))));
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    
}
