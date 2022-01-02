//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
using AngouriMath.Core.HashCode;
using AngouriMath.Extensions;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591 // TODO: add docs for records' arguments
        /// <summary>
        /// That is a node which equals Expression if Predicate is true, otherwise <see cref="MathS.NaN"/>
        /// </summary>
        public sealed partial record Providedf(Entity Expression, Entity Predicate) : Entity, IBinaryNode
        {
            public Entity NodeFirstChild => Expression;
            public Entity NodeSecondChild => Predicate;
            
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
        public sealed partial record Piecewise : Entity, IEquatable<Piecewise?>
        {
            public IEnumerable<Providedf> Cases => cases;
            private readonly IEnumerable<Providedf> cases = Enumerable.Empty<Providedf>();

            // internal override Priority Priority => Priority.Provided;
            /// <inheritdoc/>
            protected override Entity[] InitDirectChildren() => Cases.Select(c => (c.Expression, c.Predicate)).ConcatTuples().ToArray();

            private Piecewise New(IEnumerable<Providedf> newCases)
                => (Cases, newCases).SequencesAreEqualReferences() ? this : new Piecewise(newCases);

            /// <summary>
            /// Creates an instance of Piecewise
            /// </summary>
            /// <param name="cases">
            /// This is an ordered sequence of <see cref="Entity.Providedf"/>
            /// </param>
            public Piecewise(IEnumerable<Providedf> cases)
                => this.cases = cases;

            /// <summary>
            /// Returns a mapped piecewise, with every
            /// case replaced by the provided case
            /// </summary>
            /// <param name="func">
            /// Map function from a Providedf to a Providedf
            /// </param>
            public Piecewise Apply(Func<Providedf, Providedf> func)
                => New(Cases.Select(func));

            internal override Priority Priority => Priority.Leaf;

            /// <inheritdoc/>
            public override Entity Replace(Func<Entity, Entity> func)
                => func(New(Cases.Select(c => c.New(func(c.Expression), func(c.Predicate)))));

            /// <summary>
            /// Checks that two Piecewise are equal
            /// If one is not Piecewise, the method returns false
            /// </summary>
            public bool Equals(Piecewise? other)
            {
                if (other is null)
                    return false;
                if (Cases.Count() != other.Cases.Count())
                    return false;
                foreach (var (l, r) in (Cases, other.Cases).Zip())
                    if (l != r)
                        return false;
                return true;
            }

            /// <inheritdoc/>
            public override int GetHashCode()
                => Cases.HashCodeOfSequence(HashCodeFunctional.HashCodeShifts.Piecewise);

            /// <summary>
            /// Applies the given transformation to every expression of each case
            /// Predicates, however, remain unchanged
            /// </summary>
            public Piecewise ApplyToValues(Func<Entity, Entity> transformation)
                => Cases.Select(c => c.New(transformation(c.Expression), c.Predicate)).ToPiecewise();
        }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }

    
}
