using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;

namespace AngouriMath.Functions.Algebra.Solver.Analytical
{
    internal static class TrigonometricSolver
    {
        // sin(x) -> sin(1 * x)
        // sin(x) + sin(a * x)


        // if x * a
        //     x * a => a * x
        // if ! a * x
        //     x => 1 * x
        // if ! a * x + b
        //     a * x => a * x + 0
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

            // workaround for missing Patterns.Variable(x), replace all expressions, false-matching pattern to variables
            // which will be returned back after all actual replacements
            var falseReplacements = new List<KeyValuePair<VariableEntity, Entity>>();
            string falseReplacementName = "trig";

            void ReplaceSinSubExpression(ref Entity expr, Entity toReplace, Entity a, Entity b, VariableEntity replacement)
            {
                // sin(ax + b) = (t^a * e^(i*b) - t^(-a) * e^(-i*b)) / (2i)
                var resultReplacement = (MathS.Pow(replacement, a) * (MathS.Pow(MathS.e, b * MathS.i) / (2 * MathS.i)) - MathS.Pow(replacement, -a) * (MathS.Pow(MathS.e, -b * MathS.i)) / (2 * MathS.i));
                TreeAnalyzer.FindAndReplace(ref expr, toReplace, resultReplacement);
            }

            void ReplaceCosSubExpression(ref Entity expr, Entity toReplace, Entity a, Entity b, VariableEntity replacement)
            {
                // cos(ax + b) = (t^a * e^(i*b) + t^(-a) * e^(-i*b)) / 2
                var resultReplacement = (MathS.Pow(replacement, a) * (MathS.Pow(MathS.e, b * MathS.i) / 2) + MathS.Pow(replacement, -a) * (MathS.Pow(MathS.e, -b * MathS.i)) / 2);
                TreeAnalyzer.FindAndReplace(ref expr, toReplace, resultReplacement);
            }

            // checks if pattern-matching succeeded. If not, replace subexpression with some variable
            bool CheckIfReplacementIsSuitable(ref Entity expr, Entity found, Entity variable, Entity variableToCheckFor)
            {
                if(variable != variableToCheckFor)
                {
                    var repl = Utils.FindNextIndex(expr, falseReplacementName);
                    falseReplacements.Add(new KeyValuePair<VariableEntity, Entity>(repl, found));
                    TreeAnalyzer.FindAndReplace(ref expr, found, repl);
                    return false;
                }
                return true;
            }

            void MatchSinUntil(Pattern p, Func<Entity, (Entity, Entity, Entity)> variableGetter)
            {
                Entity found;
                while ((found = expr.FindPatternSubtree(p)) != null)
                {
                    (Entity x, Entity a, Entity b) = variableGetter(found.Children[0]);
                    if (CheckIfReplacementIsSuitable(ref expr, found, x, variable))
                    {
                        ReplaceSinSubExpression(ref expr, found, a, b, replacement);
                    }
                }    
            }

            void MatchCosUntil(Pattern p, Func<Entity, (Entity, Entity, Entity)> variableGetter)
            {
                Entity found;
                while ((found = expr.FindPatternSubtree(p)) != null)
                {
                    (Entity x, Entity a, Entity b) = variableGetter(found.Children[0]);
                    if (CheckIfReplacementIsSuitable(ref expr, found, x, variable))
                    {
                        ReplaceCosSubExpression(ref expr, found, a, b, replacement);
                    }
                }
            }

            // arg => (x, a, b)
            var pattern1 = Sinf.PHang(Patterns.const1 * Patterns.var1 + Patterns.any1);
            MatchSinUntil(pattern1, arg => (arg.Children[0].Children[1], arg.Children[0].Children[0], arg.Children[1]));
            var pattern2 = Sinf.PHang(Patterns.var1 * Patterns.const1 + Patterns.any1);
            MatchSinUntil(pattern2, arg => (arg.Children[0].Children[0], arg.Children[0].Children[1], arg.Children[1]));

            var pattern3 = Sinf.PHang(Patterns.any1 + Patterns.const1 * Patterns.var1);
            MatchSinUntil(pattern3, arg => (arg.Children[1].Children[1], arg.Children[1].Children[0], arg.Children[0]));
            var pattern4 = Sinf.PHang(Patterns.any1 + Patterns.var1 * Patterns.const1);
            MatchSinUntil(pattern4, arg => (arg.Children[1].Children[0], arg.Children[1].Children[1], arg.Children[0]));

            var pattern5 = Sinf.PHang(Patterns.const1 * Patterns.var1);
            MatchSinUntil(pattern5, arg => (arg.Children[1], arg.Children[0], 0));
            var pattern6 = Sinf.PHang(Patterns.var1 * Patterns.const1);
            MatchSinUntil(pattern6, arg => (arg.Children[0], arg.Children[1], 0));
            var pattern7 = Sinf.PHang(Patterns.any1 + Patterns.var1);
            MatchSinUntil(pattern7, arg => (arg.Children[1], 1, arg.Children[0]));
            var pattern8 = Sinf.PHang(Patterns.var1 + Patterns.any1);
            MatchSinUntil(pattern8, arg => (arg.Children[0], 1, arg.Children[1]));

            var pattern9 = Cosf.PHang(Patterns.const1 * Patterns.var1 + Patterns.any1);
            MatchCosUntil(pattern9, arg => (arg.Children[0].Children[1], arg.Children[0].Children[0], arg.Children[1]));
            var pattern10 = Cosf.PHang(Patterns.var1 * Patterns.const1 + Patterns.any1);
            MatchCosUntil(pattern10, arg => (arg.Children[0].Children[0], arg.Children[0].Children[1], arg.Children[1]));

            var pattern11 = Cosf.PHang(Patterns.any1 + Patterns.const1 * Patterns.var1);
            MatchCosUntil(pattern11, arg => (arg.Children[1].Children[1], arg.Children[1].Children[0], arg.Children[0]));
            var pattern12 = Cosf.PHang(Patterns.any1 + Patterns.var1 * Patterns.const1);
            MatchCosUntil(pattern12, arg => (arg.Children[1].Children[0], arg.Children[1].Children[1], arg.Children[0]));

            var pattern13 = Cosf.PHang(Patterns.const1 * Patterns.var1);
            MatchCosUntil(pattern13, arg => (arg.Children[1], arg.Children[0], 0));
            var pattern14 = Cosf.PHang(Patterns.var1 * Patterns.const1);
            MatchCosUntil(pattern14, arg => (arg.Children[0], arg.Children[1], 0));
            var pattern15 = Cosf.PHang(Patterns.any1 + Patterns.var1);
            MatchCosUntil(pattern15, arg => (arg.Children[1], 1, arg.Children[0]));
            var pattern16 = Cosf.PHang(Patterns.var1 + Patterns.any1);
            MatchCosUntil(pattern16, arg => (arg.Children[0], 1, arg.Children[1]));

            // re-substitute all false replacement to return expression to normal
            foreach (var repl in falseReplacements)
            {
                TreeAnalyzer.FindAndReplace(ref expr, repl.Key, repl.Value);
            }

            return expr;
        }

        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static Set SolveLinear(Entity expr, VariableEntity variable)
        {
            var replacement = Utils.FindNextIndex(expr, variable.Name);
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
            // TODO: make check for infinite solutions
            foreach(var solution in solutions.FiniteSet())
            {
                var sol = TreeAnalyzer.FindInvertExpression(MathS.Pow(MathS.e, MathS.i * variable), solution, variable);
                //var sol = EquationSolver.Solve(MathS.Pow(MathS.e, MathS.i * variable) - (Entity)solution, variable);
                if (sol != null)
                    actualSolutions.AddRange(sol);
            }
            return actualSolutions;
        }
    }
}
