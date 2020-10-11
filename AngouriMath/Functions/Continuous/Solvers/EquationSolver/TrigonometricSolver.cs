/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Extensions;
using System.Linq;

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
            expr = expr.Replace(Patterns.TrigonometricToExponentialRules(variable, replacement));

            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.ContainsNode(variable))
                return false;

            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els)
            {
                res = (Set)els.Select(sol => MathS.Pow(MathS.e, MathS.i * variable).Invert(sol, variable).ToSet()).Unite().InnerSimplified;
                return true;
            }
            else
                return false;
        }
    }
}