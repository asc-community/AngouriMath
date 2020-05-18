using AngouriMath.Core;
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
        private static Entity ReplaceTrigonometry(Entity expr, VariableEntity variable, VariableEntity replacement)
        {
            // SolveLinear should also solve tan and cotan equations, but currently Polynomial solver cannot handle big powers
            // uncomment lines above when it will be fixed (TODO)

            var sin = expr.FindSubtree(MathS.Sin(variable));
            var cos = expr.FindSubtree(MathS.Cos(variable));
            //var tan = expr.FindSubtree(MathS.Tan  (variable));
            //var cot = expr.FindSubtree(MathS.Cotan(variable));
            var sinReplacement = replacement / (2 * MathS.i) - MathS.Pow(replacement, -1) / (2 * MathS.i);
            var cosReplacement = replacement / 2 + MathS.Pow(replacement, -1) / 2;
            // var tanReplacement = (1 - MathS.Sqr(replacement)) * MathS.i * MathS.Pow(MathS.Sqr(replacement) + 1, -1);
            // var cotReplacement = (MathS.Sqr(replacement) + 1) * MathS.i * MathS.Pow(MathS.Sqr(replacement) - 1, -1);
            TreeAnalyzer.FindAndReplace(ref expr, sin, sinReplacement);
            TreeAnalyzer.FindAndReplace(ref expr, cos, cosReplacement);
            // TreeAnalyzer.FindAndReplace(ref expr, tan, tanReplacement);
            // TreeAnalyzer.FindAndReplace(ref expr, cot, cotReplacement);
  
            var pattern1 = Sinf.PHang(Patterns.const1 * Patterns.any1 + Patterns.const2);
            var pattern2 = Sinf.PHang(Patterns.any1 * Patterns.const1 + Patterns.const2);
            var pattern3 = Sinf.PHang(Patterns.const2 + Patterns.const1 * Patterns.any1);
            var pattern4 = Sinf.PHang(Patterns.const2 + Patterns.any1 * Patterns.const1);

            var pattern5 = Sinf.PHang(Patterns.const1 * Patterns.any1 + Patterns.const2);
            var pattern6 = Sinf.PHang(Patterns.any1 * Patterns.const1 + Patterns.const2);
            var pattern7 = Sinf.PHang(Patterns.const2 + Patterns.const1 * Patterns.any1);
            var pattern8 = Sinf.PHang(Patterns.const2 + Patterns.any1 * Patterns.const1);

            Entity found = null;

            // workaround for missing Patterns.Variable(x), replace all expressions, false-matching pattern to variables
            // which will be returned back after all actual replacements
            var falseReplacements = new List<KeyValuePair<VariableEntity, Entity>>();
            string falseReplacementName = "__false";

            // sin(a * x + b)
            while ((found = expr.FindPatternSubtree(pattern1)) != null)
            {
                var sinarg = found.Children[0];
                if (sinarg.Children[0].Children[1] != variable)
                {
                    var repl = new VariableEntity(falseReplacementName + falseReplacements.Count);
                    falseReplacements.Add(new KeyValuePair<VariableEntity, Entity>(repl, found));
                    TreeAnalyzer.FindAndReplace(ref expr, found, repl);
                }
                else
                {
                    var a = sinarg.Children[0].Children[0];
                    var b = sinarg.Children[1];

                    // (t^(2a) * e^(i*b) - e^(-i*b)) / (2i * t^a)
                    var resultReplacement = (MathS.Pow(replacement, a) * (MathS.Pow(MathS.e, b * MathS.i) / (2 * MathS.i)) - MathS.Pow(replacement, -a) * (MathS.Pow(MathS.e, -b * MathS.i)) / (2 * MathS.i));
                    TreeAnalyzer.FindAndReplace(ref expr, found, resultReplacement);
                }
            }

            foreach (var repl in falseReplacements)
            {
                TreeAnalyzer.FindAndReplace(ref expr, repl.Key, repl.Value);
            }

            return expr;
        }

        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static Set SolveLinear(Entity expr, VariableEntity variable)
        {
            var replacement = new VariableEntity(variable.Name + "_trig");
            expr = ReplaceTrigonometry(expr, variable, replacement);

            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.FindSubtree(variable) != null)
            {
                return null;
            }

            var solutions = EquationSolver.Solve(expr, replacement);
            if (solutions == null) return null;

            var actualSolutions = new Set();
            foreach(var solution in solutions)
            {
                var sol = EquationSolver.Solve(MathS.Pow(MathS.e, MathS.i * variable) - (Entity)solution, variable);
                if (sol != null)
                    actualSolutions.AddRange(sol);
            }
            return actualSolutions;
        }
    }
}
