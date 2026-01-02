//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using AngouriMath.Extensions;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    internal static class TrigonometricSolver
    {
        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static bool TrySolveLinear(Entity expr, Variable variable, out Set res)
        {
            res = Empty;
            var replacement = Variable.CreateTemp(expr.Vars);
            expr = expr.Replace(Patterns.NormalTrigonometricForm);
            expr = expr.Replace(Patterns.TrigonometricToExponentialRules(variable, replacement));
            MultithreadingFunctional.ExitIfCancelled();
            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.ContainsNode(variable))
                return false;

            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els)
            {
                MultithreadingFunctional.ExitIfCancelled();
                res = (Set)els.Select(sol => MathS.Pow(MathS.e, MathS.i * variable).Invert(sol, variable).ToSet()).Unite().InnerSimplified;
                return true;
            }
            else
                return false;
        }
    }
}