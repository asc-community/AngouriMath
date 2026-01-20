//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions.Algebra;

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
                            => res.InnerSimplified(isExact),
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
                        (var expr, Variable var, var (from, to)) => ConditionallySimplified(expr.Integrate(var, from, to), isExact),
                        (var expr, var otherExpr, var (from, to))
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Integrate(tempVar, from, to) is var res => ConditionallySimplified(res.Substitute(tempVar, otherExpr), isExact),
                        (var expr, Variable var, null) => ConditionallySimplified(expr.Integrate(var), isExact),
                        (var expr, var otherExpr, null)
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Integrate(tempVar) is var res => ConditionallySimplified(res.Substitute(tempVar, otherExpr), isExact),
                        _ => null
                    },
                    (@this, a, b, c) => ((Integralf)@this).New(a, b, c), isExact);
        }


        // TODO: rewrite this part too
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
