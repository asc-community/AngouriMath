using AngouriMath.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    class ExponentialSolver
    {
        internal static Entity.SetNode? SolveLinear(Entity expr, Entity.Variable x)
        {
            var replacement = Entity.Variable.CreateTemp(expr.Vars);

            Func<Entity, Entity> preparator = e => e switch
            {
                Entity.Powf(var @base, var arg) =>
                    TreeAnalyzer.TryGetPolyLinear(arg, x, out var a, out var b) ?
                        MathS.Pow(MathS.e, b) * MathS.Pow(MathS.Pow(MathS.e, x), MathS.Ln(@base) * a) : e,

                _ => e
            };

            Func<Entity, Entity> replacer = e => e switch
            {
                Entity.Powf(var @base, var arg) =>
                    @base == MathS.e && arg == x ?
                        replacement : e,

                _ => e,
            };

            expr = expr.Replace(preparator);
            expr = expr.Replace(replacer);

            if (expr.Contains(x)) return null; // cannot be solved, not a pure exponential

            expr = expr.InnerSimplify();
            if (AnalyticalEquationSolver.Solve(expr, replacement).IsFiniteSet(out var els))
                return els.Select(sol => MathS.Pow(MathS.e, x).Invert(sol, x).ToSetNode()).Unite();
            else
                return null;
        }
    }
}
