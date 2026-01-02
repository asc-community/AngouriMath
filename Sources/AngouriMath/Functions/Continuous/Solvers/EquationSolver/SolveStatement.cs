//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Functions.Continuous.Solvers.SetSolver;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class StatementSolver
    {
        private static Entity Minus(Entity left, Entity right)
        {
            if (left.Evaled == 0)
                return -right;
            if (right.Evaled == 0)
                return left;
            return left - right;
        }

        internal static Set Solve(Entity expr, Variable x)
            => expr switch
            {
                Equalsf(var left, var right) when left is Set || right is Set
                    => AnalyticalSetSolver.Solve(left, right, x),

                Equalsf(var left, var right) when left is not Set && right is not Set
                    => AnalyticalEquationSolver.Solve(left - right, x),

                Equalsf => Empty,

                Andf(var left, var right) => 
                    MathS.Intersection(Solve(left, x), Solve(right, x)),
                Orf(var left, var right) => 
                    MathS.Union(Solve(left, x), Solve(right, x)),
                Impliesf(var left, var right) => 
                    MathS.Union(MathS.SetSubtraction(expr.Codomain, Solve(left, x)), Solve(right, x)),

                // TODO: there should be universal set to subtract from when inverting
                Greaterf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(left, right), x),
                LessOrEqualf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x)
                    .Unite(AnalyticalEquationSolver.Solve(Minus(left, right), x)),
                GreaterOrEqualf(var left, var right) => MathS.Union(AnalyticalInequalitySolver.Solve(Minus(left, right), x), AnalyticalEquationSolver.Solve(Minus(left, right), x)),

                Lessf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x),

                Variable when expr == x => new FiniteSet(true),

                Inf(var var, Set set) when var == x => set,

                // TODO: Although piecewise needed?
                _ => Set.Empty
            };
    }
}
