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
                        (var expr, _, 0) => expr,
                        (_, _, int.MinValue) => null,
                        (_, _, < 0) => new Integralf(a, b, -c).InnerSimplified(isExact),
                        // TODO: should we call InnerSimplified here?
                        (var expr, Variable var, int asInt)
                            when expr.Differentiate(var, asInt) is var res and not Derivativef
                            => res.InnerSimplified(isExact),
                        (var expr, Variable var, int asInt) => null,
                        (Application, _, _) => null,
                        (var expr, Entity otherExpr, int asInt)
                            when Variable.CreateTemp(otherExpr.Vars) is var tempVar
                            && expr.Substitute(otherExpr, tempVar) is var tempSubstituted
                            && tempSubstituted.Differentiate(tempVar) is var res and not Derivativef
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
            
            private Entity SequentialIntegrating(Entity expr, Variable var, int iterations)
            {
                if (iterations < 0)
                    return this;
                var changed = expr;
                for (int i = 0; i < iterations; i++)
                    changed = Integration.ComputeIndefiniteIntegral(changed, var);
                return changed;
            }

            /// <inheritdoc/>
            protected override Entity InnerSimplify(bool isExact) =>
                ExpandOnTwoAndTArguments(Expression, Var, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        (_, _, int.MinValue) => null,
                        (_, _, < 0 and not int.MinValue) => new Derivativef(a, b, -c).InnerSimplified(isExact),
                        // TODO: should we apply InnerSimplified?
                        (var expr, Variable var, int asInt)
                            when SequentialIntegrating(expr, var, asInt) is var res and not Integralf
                            && !res.Nodes.Any(n => n is Integralf)
                            => res.InnerSimplified(isExact),
                        _ => null
                    },
                    (@this, a, b, _) => ((Integralf)@this).New(a, b), isExact);
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
