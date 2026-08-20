//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions.Algebra;

using System;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            // The derivative operator is always defined symbolically, even though
            // the resulting expression may be undefined at certain points.
            private protected override Entity IntrinsicCondition => Boolean.True;
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoAndTArguments(Expression, Var, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        // TODO: should we call InnerSimplified here?
                        (var expr, Variable var, int asInt)
                            when expr.Differentiate(var, asInt) is var res and not Derivativef
                            => Core.Binding.Written(var, res.InnerSimplified(isExact)),
                        (var expr, Variable var, int asInt) => null,
                        (Application, _, _) => null,
                        (var expr, Entity otherExpr, int asInt)
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Differentiate(tempVar, asInt) is var res and not Derivativef
                            => res.Substitute(tempVar, otherExpr).InnerSimplified(isExact),
                        _ => null
                    },
                    (@this, a, b, _) => ((Derivativef)@this).New(a, b), isExact);
        }
        
        public partial record Integralf
        {
            // The integral operator is always defined symbolically, even though
            // the antiderivative may not exist in closed form or may be undefined at certain points.
            private protected override Entity IntrinsicCondition => Boolean.True;

            private static Entity? ConditionallySimplified(Entity e, bool isExact) => e is Integralf ? null : e.InnerSimplified(isExact);
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoAndTArguments(Expression, Var, Range,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, Variable var, var (from, to)) => Core.Binding.Written(var, ConditionallySimplified(expr.Integrate(var, from, to), isExact)),
                        (var expr, var otherExpr, var (from, to))
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Integrate(tempVar, from, to) is var res => ConditionallySimplified(res.Substitute(tempVar, otherExpr), isExact),
                        (var expr, Variable var, null) => Core.Binding.Written(var, ConditionallySimplified(expr.Integrate(var), isExact)),
                        (var expr, var otherExpr, null)
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Integrate(tempVar) is var res => ConditionallySimplified(res.Substitute(tempVar, otherExpr), isExact),
                        _ => null
                    },
                    (@this, a, b, c) => ((Integralf)@this).New(a, b, c), isExact);
        }


        // TODO: rewrite this part too
        public partial record Summationf
        {
            /// <summary>
            /// A summation is a well-formed expression whatever its bounds, so it is defined
            /// everywhere its expression is; the expansion below is an evaluation, not a
            /// definedness claim.
            /// </summary>
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <summary>
            /// How many terms are worth writing out. A sum of a thousand terms is a correct
            /// expansion and a useless expression, and the cost is paid by everything downstream
            /// that then walks it.
            /// </summary>
            internal const int MaxExpandedTerms = 100;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                Expanded(this, Expression, Var, From, To, static (a, b) => a + b, 0, isExact) ?? this;

            /// <summary>
            /// Writes the operator out where its bounds are concrete integers and there are few
            /// enough terms, and returns null otherwise -- an unevaluated operator rather than a
            /// wrong or enormous answer.
            /// </summary>
            internal static Entity? Expanded(Entity self, Entity expression, Entity var, Entity from, Entity to,
                Func<Entity, Entity, Entity> combine, Entity empty, bool isExact)
            {
                if (var is not Variable index)
                    return null;
                if (from.Evaled is not Integer lower || to.Evaled is not Integer upper)
                    return null;
                if (!lower.EInteger.CanFitInInt32() || !upper.EInteger.CanFitInInt32())
                    return null;
                var lo = lower.EInteger.ToInt32Checked();
                var hi = upper.EInteger.ToInt32Checked();
                // An empty range is the operator's identity: nothing summed is 0, nothing
                // multiplied is 1. Stated rather than left to the loop, which would return the
                // accumulator's initial value by accident and read as a coincidence.
                if (hi < lo)
                    return empty;
                if ((long)hi - lo + 1 > MaxExpandedTerms)
                    return null;
                var accumulated = empty;
                for (var i = lo; i <= hi; i++)
                    accumulated = combine(accumulated, expression.Substitute(index, i));
                return accumulated.InnerSimplified;
            }
        }

        public partial record Productf
        {
            /// <summary>See <see cref="Summationf"/>; a product is defined wherever its expression is.</summary>
            private protected override Entity IntrinsicCondition => Boolean.True;

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                Summationf.Expanded(this, Expression, Var, From, To, static (a, b) => a * b, 1, isExact) ?? this;
        }

        public partial record Limitf
        {
            // The limit operator is always defined symbolically, even though
            // the limit may not exist (returns NaN/undefined) for certain functions.
            private protected override Entity IntrinsicCondition => Boolean.True;
            
            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoAndTArguments(
                    Expression, Destination, (v: Var, ap: ApproachFrom),
                    (expr, dest, vap) => vap.v switch
                    {
                        Variable v when expr.Limit(v, dest, vap.ap) is var res and not Limitf => res.InnerSimplified(isExact),
                        _ => null
                    },
                    (@this, expr, dest, vap) => ((Limitf)@this).New(expr, vap.v, dest, vap.ap), isExact);
        }
    }
}
