using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Functions.Algebra.Solver
{
    internal static class EquationSolver
    {
        internal static EntitySet Solve(Entity equation, VariableEntity x)
        {
            var res = new EntitySet();
            AnalyticalSolver.Solve(equation, x, res);
            return res;
        }
    }
}
