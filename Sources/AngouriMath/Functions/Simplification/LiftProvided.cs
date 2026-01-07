//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using System.Xml.Linq;

namespace AngouriMath
{
    partial record Entity
    {
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
                // Special handling for FiniteSet: only lift predicates to the set boundary, let Set InnerSimplify handle it
                internal override Entity LiftProvided(HashSet<Entity> predicates) =>
                    Apply(e =>
                    {
                        HashSet<Entity> innerPredicates = [];
                        var newElement = e.LiftProvided(innerPredicates);
                        return innerPredicates.Count == 0 ? newElement : new Providedf(newElement, innerPredicates.Aggregate((acc, curr) => acc & curr));
                    });
            }
            partial record Interval { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Left.LiftProvided(predicates), LeftClosed, Right.LiftProvided(predicates), RightClosed); }
            partial record ConditionalSet
            {
                // For ConditionalSet, merge predicates
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
            // Lift provided nodes to the outermost level
            internal override Entity LiftProvided(HashSet<Entity> predicates)
            {
                predicates.Add(Predicate.LiftProvided(predicates));
                return Expression.LiftProvided(predicates);
            }
        }
        partial record Piecewise {
            // Lift provided nodes to the the piecewise boundary
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
        partial record Lambda { internal override Entity LiftProvided(HashSet<Entity> predicates) => New(Parameter, Body.LiftProvided(predicates)); }
    }
}
