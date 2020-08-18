
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
using AngouriMath.Core.Exceptions;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AngouriMath.Functions.Algebra
{
    using static Entity;
    internal static class EquationSolver
    {
        /// <summary>Solves one equation</summary>
        internal static Set Solve(Entity equation, Variable x)
        {
            var res = new Set();

            MathS.Settings.PrecisionErrorZeroRange.As(1e-12m, () =>
            MathS.Settings.FloatToRationalIterCount.As(0, () =>
                AnalyticalSolver.Solve(equation, x, res)
            ));

            if (res.Power == Set.PowerLevel.FINITE)
            {
                static Entity simplifier(Entity entity) => entity.InnerSimplify();
                static Entity evaluator(Entity entity) => entity.Evaled;
                Entity collapser(Entity expr) => equation.Vars.Count == 1 ? evaluator(expr) : simplifier(expr);

                res.FiniteApply(simplifier);
                var finalSet = new Set { FastAddingMode = true };
                foreach (var elem in res.FiniteSet())
                    if (elem.IsFinite && collapser(equation.Substitute(x, elem)).IsFinite)
                        finalSet.Add(elem);
                finalSet.FastAddingMode = false;
                res = finalSet;
            }

            return res;
        }

        /// <summary>
        /// Solves a system of equations by solving one after another with substitution, e.g. <br/>
        /// let { x - y + a = 0, y + 2a = 0 } be a system of equations for variables { x, y } <br/>
        /// Then we first find y from the first equation, <br/>
        /// y = x + a <br/>
        /// then we substitute it to all others <br/>
        /// x + a + 2a = 0 <br/>
        /// then we find x <br/>
        /// x = -3a <br/>
        /// Then we substitute back <br/>
        /// y = -3a + a = -2a <br/>
        /// </summary>
        internal static Tensor? SolveSystem(List<Entity> equations, ReadOnlySpan<Variable> vars)
        {
            if (equations.Count != vars.Length)
                throw new MathSException("Amount of equations must be equal to that of vars");
            equations = new List<Entity>(equations.Select(c => c));
            int initVarCount = vars.Length;
            for (int i = 0; i < equations.Count; i++)
                equations[i] = equations[i].InnerSimplify();

            var res = InSolveSystem(equations, vars);

            foreach (var tuple in res)
                if (tuple.Count != initVarCount)
                    throw new AngouriBugException("InSolveSystem incorrect output");
            if (res.Count == 0)
                return null;
            return new Tensor(indices => res[indices[0]][indices[1]], res.Count, initVarCount);
        }

        /// <summary>Solves system of equations</summary>
        /// <param name="equations"><see cref="List{T}"/> of <see cref="Entity"/></param>
        /// <param name="vars">
        /// <see cref="List{T}"/> of <see cref="Variable"/>s,
        /// where each of them must be mentioned in at least one entity from equations
        /// </param>
        internal static List<List<Entity>> InSolveSystem(List<Entity> equations, ReadOnlySpan<Variable> vars)
        {
            var var = vars[vars.Length - 1];
            if (equations.Count == 1)
                return equations[0].InnerSimplify().SolveEquation(var).FiniteSet()
                       .Select(sol => new List<Entity> { sol }).ToList();
            var result = new List<List<Entity>>();
            var replacements = new Dictionary<Variable, Entity>();
            for (int i = 0; i < equations.Count; i++)
                if (equations[i].Vars.Contains(var))
                {
                    var solutionsOverVar = equations[i].SolveEquation(var);
                    equations.RemoveAt(i);
                    vars = vars.Slice(0, vars.Length - 1);

                    foreach (var sol in solutionsOverVar.FiniteSet())
                        foreach (var j in
                            InSolveSystem(equations.Select(eq => eq.Substitute(var, sol)).ToList(), vars))
                        {
                            replacements.Clear();
                            for (int varid = 0; varid < vars.Length; varid++)
                                replacements.Add(vars[varid], j[varid]);
                            j.Add(sol.Substitute(replacements).InnerSimplify());
                            result.Add(j);
                        }
                    break;
                }
            return result;
        }
    }
}