/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Functions.Algebra;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoAndTArguments(Expression.Evaled, Var.Evaled, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Integral for negative cases
                        // TODO: should we call InnerSimlified here?
                        (var expr, Variable var, int asInt)
                            when expr.Differentiate(var, asInt) is var res and not Derivativef
                            => res.Evaled,
                        _ => null
                    },
                    (@this, a, b, _) => ((Derivativef)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoAndTArguments(Expression.Evaled, Var.Evaled, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Integral for negative cases
                        // TODO: should we call InnerSimlified here?
                        (var expr, Variable var, int asInt)
                            when expr.Differentiate(var, asInt) is var res and not Derivativef
                            => res.InnerSimplified,
                        _ => null
                    },
                    (@this, a, b, _) => ((Derivativef)@this).New(a, b)
                    );
        }
        public partial record Integralf
        {
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
            protected override Entity InnerEval() =>
                ExpandOnTwoAndTArguments(Expression.Evaled, Var.Evaled, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Derivative for negative cases
                        (var expr, Variable var, int asInt)
                            when SequentialIntegrating(expr, var, asInt) is var res and not Integralf
                            => res.Evaled,
                        _ => null
                    },
                    (@this, a, b, _) => ((Integralf)@this).New(a, b)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoAndTArguments(Expression.InnerSimplified, Var, Iterations,
                    (a, b, c) => (a, b, c) switch
                    {
                        (var expr, _, 0) => expr,
                        // TODO: consider Derivative for negative cases
                        // TODO: should we apply InnerSimplified?
                        (var expr, Variable var, int asInt)
                            when SequentialIntegrating(expr, var, asInt) is var res and not Integralf
                            => res.InnerSimplified,
                        _ => null
                    },
                    (@this, a, b, _) => ((Integralf)@this).New(a, b)
                    );
        }


        // TODO: rewrite this part too
        public partial record Limitf
        {
            /// <inheritdoc/>
            protected override Entity InnerEval() =>
                ExpandOnTwoAndTArguments(
                    Expression.Evaled, Destination.Evaled, (v: Var, ap: ApproachFrom),
                    (expr, dest, vap) => vap.v switch
                    {
                        Variable v when expr.Limit(v, dest, vap.ap) is var res and not Limitf 
                            => res.Evaled,
                        _ => null
                    },
                    (@this, expr, dest, vap) => ((Limitf)@this).New(expr, vap.v, dest, vap.ap)
                    );

            /// <inheritdoc/>
            protected override Entity InnerSimplify() =>
                ExpandOnTwoAndTArguments(
                    Expression.InnerSimplified, Destination.InnerSimplified, (v: Var, ap: ApproachFrom),
                    (expr, dest, vap) => vap.v switch
                    {
                        Variable v when expr.Limit(v, dest, vap.ap) is var res and not Limitf
                            => res.InnerSimplified,
                        _ => null
                    },
                    (@this, expr, dest, vap) => ((Limitf)@this).New(expr, vap.v, dest, vap.ap)
                    );

        }
    }
}
