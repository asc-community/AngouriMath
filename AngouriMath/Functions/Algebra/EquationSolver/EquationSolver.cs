
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


using System;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
 using AngouriMath.Functions.Algebra.AnalyticalSolving;
 using System.Collections.Generic;
using System.Linq;
 using AngouriMath.Core.TreeAnalysis;
using GenericTensor.Core;


namespace AngouriMath.Functions.Algebra.Solver
{
    internal static class EquationSolver
    {
        /// <summary>
        /// Solves one equation
        /// </summary>
        /// <param name="equation"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        internal static Set Solve(Entity equation, VariableEntity x)
        {
            var res = new Set();

            MathS.Settings.PrecisionErrorZeroRange.As(1e-12m, () =>
            MathS.Settings.FloatToRationalIterCount.As(0, () => 
                AnalyticalSolver.Solve(equation, x, res)
            ));

            if (res.Power == Set.PowerLevel.FINITE)
            {
                static Entity simplifier(Entity entity) => entity.InnerSimplify();
                static Entity evaluator(Entity entity) => entity.InnerEval();
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
        /// Solves a system of equations by solving one after another with substitution, e. g.
        /// let 
        /// x - y + a = 0
        /// y + 2a = 0
        /// be a system of equations for variables { x, y }
        /// Then we first find y from the first equation,
        /// y = x + a
        /// then we substitute it to all others
        /// x + a + 2a = 0
        /// then we find x
        /// x = -3a
        /// Then we substitute back
        /// y = -3a + a = -2a
        /// </summary>
        /// <param name="equations"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        internal static Tensor? SolveSystem(List<Entity> equations, ReadOnlySpan<VariableEntity> vars)
        {
            if (equations.Count != vars.Length)
                throw new MathSException("Amount of equations must be equal to that of vars");
            equations = new List<Entity>(equations.Select(c => c));
            int initVarCount = vars.Length;
            for (int i = 0; i < equations.Count; i++)
                equations[i] = equations[i].InnerSimplify();

            var res = InSolveSystem(equations, vars).ToList();

            foreach (var tuple in res)
                if (tuple.Count != initVarCount)
                    throw new AngouriBugException("InSolveSystem incorrect output");
            if (res.Count == 0)
                return null;
            var result = new Tensor(res.Count, initVarCount);
            for (int i = 0; i < res.Count; i++)
                for (int j = 0; j < initVarCount; j++)
                    result[i, j] = res[i][j];
            return result;
        }

        /// <summary>
        /// Solves an equation, useful once InSolveSystem reaches equations.Count == 1
        /// </summary>
        /// <param name="eq">
        /// The equation to solve
        /// </param>
        /// <param name="var">
        /// Variable to solve for
        /// </param>
        /// <returns></returns>
        internal static IEnumerable<List<Entity>> InSolveSystemOne(Entity eq, VariableEntity var) =>
            eq.InnerSimplify().SolveEquation(var).FiniteSet().Select(sol => new List<Entity> { sol });

        /// <summary>
        /// Solves system of equations
        /// </summary>
        /// <param name="equations">
        /// List of Entities
        /// </param>
        /// <param name="vars">
        /// List of variables, where each of them must be mentioned in at least one entity from equations
        /// </param>
        /// <returns></returns>
        internal static IEnumerable<List<Entity>> InSolveSystem(List<Entity> equations, ReadOnlySpan<VariableEntity> vars)
        {
            var var = vars[vars.Length - 1];
            if (equations.Count == 1)
                return InSolveSystemOne(equations[0], var);
            var result = Enumerable.Empty<List<Entity>>();
            var replacements = new Dictionary<VariableEntity, Entity>();
            for (int i = 0; i < equations.Count; i++)
                if (equations[i].Vars.Contains(var))
                {
                    var solutionsOverVar = equations[i].SolveEquation(var);
                    equations.RemoveAt(i);
                    vars = vars.Slice(0, vars.Length - 2);
                    
                    foreach (var sol in solutionsOverVar.FiniteSet())
                    {
                        var newequations = new List<Entity>();
                        for (int eqid = 0; eqid < equations.Count; eqid++)
                            newequations.Add(equations[eqid].Substitute(var, sol));
                        var inSol = InSolveSystem(newequations, vars);
                        foreach (var j in inSol)
                        {
                            replacements.Clear();
                            for (int varid = 0; varid < vars.Length; varid++)
                                replacements.Add(vars[varid], j[varid]);

                            var Z = sol.Substitute(replacements);

                            Z = Z.InnerSimplify();
                            j.Add(Z);
                        }
                        result = result.Concat(inSol);
                    }
                    break;
                }
            return result;
        }
    }
}
