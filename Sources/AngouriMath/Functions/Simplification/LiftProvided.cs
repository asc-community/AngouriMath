//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using System.Xml.Linq;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions
{
    internal static class ProvidedLifter
    {
        internal static Set MergePredicateIntoSolveResult(Set solveResult, Variable x, Entity predicate) =>
            solveResult switch
                {
                    var s when predicate == Entity.Boolean.True => s,
                    FiniteSet f => f.Apply(e => e.Provided(predicate.Substitute(x, e))),
                    ConditionalSet c => new ConditionalSet(c.Var, c.Predicate & predicate.Substitute(x, c.Var)),
                    var s => new ConditionalSet(x, new Inf(x, s) & predicate)
                };
        /// <summary>
        /// Extracts all <see cref="Providedf"/> predicates to the top level.
        /// </summary>
        internal static bool ExtractProvidedPredicates(ref Entity expression, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Entity? providedPredicate)
        {
            HashSet<Entity> predicates = [];
            expression = expression.LiftProvided(predicates);
            if (predicates.Count == 0)
            {
                providedPredicate = null;
                return false;
            }
            else
            {
                providedPredicate = predicates.Aggregate((acc, curr) => acc & curr);
                return true;
            }
        }
    }
}
namespace AngouriMath
{
    partial record Entity
    {

        /// <summary>
        /// Recursively lifts all <see cref="Providedf"/> nodes in the expression tree,
        /// collecting their predicates into the provided set. This is the internal
        /// implementation for <see cref="ProvidedLifter.ExtractProvidedPredicates"/>.
        /// </summary>
        /// <param name="predicates">
        /// A set that accumulates all provided predicates encountered during traversal.
        /// </param>
        /// <returns>
        /// The expression with all <see cref="Providedf"/> nodes removed.
        /// </returns>
        internal abstract Entity LiftProvided(HashSet<Entity> predicates);

        public partial record Number { internal override Entity LiftProvided(HashSet<Entity> predicates) => this; }
        public partial record Variable { internal override Entity LiftProvided(HashSet<Entity> predicates) => this; }
        partial record Matrix { internal override Entity LiftProvided(HashSet<Entity> predicates) => Elementwise(element => element.LiftProvided(predicates)); }
        public partial record Sumf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Augend.LiftProvided(predicates), Addend.LiftProvided(predicates)); }
        public partial record Minusf {  internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Subtrahend.LiftProvided(predicates), Minuend.LiftProvided(predicates)); }
        public partial record Mulf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Multiplier.LiftProvided(predicates), Multiplicand.LiftProvided(predicates)); }
        public partial record Divf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Dividend.LiftProvided(predicates), Divisor.LiftProvided(predicates)); }
        public partial record Powf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Base.LiftProvided(predicates), Exponent.LiftProvided(predicates)); }
        public partial record Logf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Base.LiftProvided(predicates), Antilogarithm.LiftProvided(predicates)); }
        public partial record Sinf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Cosf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Secantf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Cosecantf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Tanf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Cotanf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arcsecantf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arccosecantf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arcsinf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arccosf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arctanf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Arccotanf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Factorialf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Derivativef { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Expression.LiftProvided(predicates), Var.LiftProvided(predicates)); }
        public partial record Integralf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Expression.LiftProvided(predicates), Var.LiftProvided(predicates)); }
        public partial record Limitf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Expression.LiftProvided(predicates), Var.LiftProvided(predicates), Destination.LiftProvided(predicates), ApproachFrom); }
        public partial record Signumf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Absf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Boolean { internal override Entity LiftProvided(HashSet<Entity> predicates) => this; }
        public partial record Notf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        public partial record Andf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record Orf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record Xorf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record Impliesf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Assumption.LiftProvided(predicates), Conclusion.LiftProvided(predicates)); }
        public partial record Equalsf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record Greaterf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record GreaterOrEqualf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record Lessf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        public partial record LessOrEqualf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
        partial record Set
        {
            partial record FiniteSet
            {
                /// <summary>
                /// For finite sets, lift provided predicates to the set boundary level.
                /// Each element's conditions are kept with that element as a Providedf wrapper,
                /// allowing Set.InnerSimplify to handle per-element conditions appropriately.
                /// </summary>
                internal override Entity LiftProvided(HashSet<Entity> predicates) =>
                    Apply(e => ProvidedLifter.ExtractProvidedPredicates(ref e, out var predicate) ? new Providedf(e, predicate) : e);
            }
            partial record Interval { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), LeftClosed, Right.LiftProvided(predicates), RightClosed); }
            partial record ConditionalSet
            {
                /// <summary>
                /// For conditional sets { x : P(x) }, merge any provided predicates from P(x)
                /// into the predicate itself, since the predicate defines the set membership condition.
                /// </summary>
                internal override Entity LiftProvided(HashSet<Entity> predicates) {
                    HashSet<Entity> innerPredicates = [];
                    var newVar = Var.LiftProvided(innerPredicates);
                    var newPredicate = Predicate.LiftProvided(innerPredicates);
                    return New(newVar, innerPredicates.Aggregate(newPredicate, (acc, curr) => acc & curr));
                }
            }
            partial record SpecialSet { internal override Entity LiftProvided(HashSet<Entity> predicates) => this; }
            partial record Unionf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
            partial record Intersectionf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
            partial record SetMinusf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), Right.LiftProvided(predicates)); }
            partial record Inf { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Element.LiftProvided(predicates), SupSet.LiftProvided(predicates)); }
        }
        partial record Phif { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Argument.LiftProvided(predicates)); }
        partial record Providedf
        {
            /// <summary>
            /// The Providedf node itself lifts its predicate into the
            /// accumulator set and returns its inner expression for continued traversal.
            /// This is the core mechanism that extracts "expr provided P" into (expr, P).
            /// </summary>
            internal override Entity LiftProvided(HashSet<Entity> predicates)
            {
                foreach (var p in Andf.LinearChildren(Predicate.LiftProvided(predicates)))
                    predicates.Add(p);
                return Expression.LiftProvided(predicates);
            }
        }
        partial record Piecewise {
            /// <summary>
            /// For piecewise functions, lift provided predicates to the boundary of each case.
            /// Each case maintains its own (expression, condition) pair as a Providedf,
            /// allowing proper handling of per-branch validity conditions.
            /// </summary>
            internal override Entity LiftProvided(HashSet<Entity> predicates) =>
                Apply(provided =>
                {
                    HashSet<Entity> innerPredicates = [];
                    var newExpr = provided.Expression.LiftProvided(innerPredicates);
                    var newPred = provided.Predicate.LiftProvided(innerPredicates);
                    return new Providedf(newExpr, innerPredicates.Aggregate(newPred, (acc, curr) => acc & curr));
                });
        }
        partial record Application { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Expression.LiftProvided(predicates), Arguments.Select(e => e.LiftProvided(predicates)).ToLList()); }
        partial record Lambda {

            /// For lambdas, lift provided predicates to the lambdas boundary.
            internal override Entity LiftProvided(HashSet<Entity> predicates)
            {
                var body = Body;
                return New(Parameter, ProvidedLifter.ExtractProvidedPredicates(ref body, out var predicate) ? new Providedf(body, predicate) : body);
            }
        }
    }
}
