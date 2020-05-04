using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AngouriMath.Functions.Algebra.Solver.Analytical
{
    internal static class TrigonometricSolver
    {
        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static EntitySet SolveLinear(Entity expr, VariableEntity variable)
        {
            // SolveLinear should also solve tan and cotan equations, but currently Polynomial solver cannot handle big powers
            // uncomment lines above when it will be fixed (TODO)

            var replacement = new VariableEntity(variable.Name + "_trig");
            var sin = expr.FindSubtree(MathS.Sin  (variable));
            var cos = expr.FindSubtree(MathS.Cos  (variable));
            var tan = expr.FindSubtree(MathS.Tan  (variable));
            var cot = expr.FindSubtree(MathS.Cotan(variable));
            var sinReplacement = (MathS.Sqr(replacement) - 1) / (2 * MathS.i * replacement);
            var cosReplacement = (MathS.Sqr(replacement) + 1) / (2 * replacement);
            // var tanReplacement = (1 - MathS.Sqr(replacement)) * MathS.i / (MathS.Sqr(replacement) + 1);
            // var cotReplacement = (MathS.Sqr(replacement) + 1) * MathS.i / (MathS.Sqr(replacement) - 1);
            TreeAnalyzer.FindAndReplace(ref expr, sin, sinReplacement);
            TreeAnalyzer.FindAndReplace(ref expr, cos, cosReplacement);
            // TreeAnalyzer.FindAndReplace(ref expr, tan, tanReplacement);
            // TreeAnalyzer.FindAndReplace(ref expr, cot, cotReplacement);

            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.FindSubtree(variable) != null)
            {
                return null;
            }

            var solutions = EquationSolver.Solve(expr, replacement);
            if (solutions == null) return null;

            var actualSolutions = new EntitySet();
            foreach(var solution in solutions)
            {
                var sol = EquationSolver.Solve(MathS.Pow(MathS.e, MathS.i * variable) - solution, variable);
                if (sol != null)
                    actualSolutions.AddRange(sol);
            }
            return actualSolutions;
        }
    }
}
