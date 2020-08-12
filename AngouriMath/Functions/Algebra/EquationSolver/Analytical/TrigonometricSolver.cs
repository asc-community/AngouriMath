
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Functions.Algebra.Solver.Analytical
{
    internal static class TrigonometricSolver
    {
        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static Set? SolveLinear(Entity expr, VariableEntity variable)
        {
            var replacement = Utils.FindNextIndex(expr, variable.Name);
            expr = expr.Replace(Patterns.TrigonometricToExponentialRules(variable, replacement));

            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.Vars.Contains(variable))
                return null;

            var solutions = EquationSolver.Solve(expr, replacement);
            if (solutions == null) return null;

            var actualSolutions = new Set();
            // TODO: make check for infinite solutions
            foreach(var solution in solutions.FiniteSet())
            {
                var func = MathS.Pow(MathS.e, MathS.i * variable);
                foreach (var sol in func.Invert(solution, variable))
                    actualSolutions.AddPiece(sol);
            }
            return actualSolutions;
        }
    }
}
