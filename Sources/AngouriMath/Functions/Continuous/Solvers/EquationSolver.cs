//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;

namespace AngouriMath.Functions.Algebra
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    internal static class EquationSolver
    {
        /// <summary>Solves one equation</summary>
        internal static Set Solve(Entity equation, Variable x)
        {
            using var _ = MathS.Settings.PrecisionErrorZeroRange.Set(1e-12m);
            using var __ = MathS.Settings.FloatToRationalIterCount.Set(0);
            using var ___ = MathS.Settings.MaxExpansionTermCount.Set(50);
            var solutions = AnalyticalEquationSolver.Solve(equation, x);

            static Entity simplifier(Entity entity) => entity.InnerSimplified;
            static Entity evaluator(Entity entity) => entity.Evaled;
            var factorizer = equation.Vars.Count == 1 ? (Func<Entity, Entity>)evaluator : simplifier;


            if (solutions is FiniteSet finiteSet)
            {
                return finiteSet.Select(simplifier)
                    .Where(elem => elem.IsFinite && factorizer(equation.Substitute(x, elem)).IsFinite).ToSet();
            }
            else
                return solutions;
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
        internal static Matrix? SolveSystem(IEnumerable<Entity> inputEquations, ReadOnlySpan<Variable> vars)
        {
            var equations = new List<Entity>(inputEquations.Select(equation => equation.InnerSimplified));
            if (equations.Count != vars.Length)
                throw new WrongNumberOfArgumentsException("Number of equations must be equal to that of vars");
            int initVarCount = vars.Length;

            var res = InSolveSystem(equations, vars);
            foreach (var tuple in res)
                if (tuple.Count != initVarCount)
                    throw new AngouriBugException("InSolveSystem incorrect output");
            if (res.Count == 0)
                return null;
            var tb = new MatrixBuilder(res, initVarCount);
            return tb.ToMatrix();
        }

        /// <summary>Solves system of equations</summary>
        /// <param name="equations"><see cref="List{T}"/> of <see cref="Entity"/></param>
        /// <param name="vars">
        /// <see cref="List{T}"/> of <see cref="Variable"/>s,
        /// where each of them must be mentioned in at least one entity from equations
        /// </param>
        internal static List<List<Entity>> InSolveSystem(List<Entity> equations, ReadOnlySpan<Variable> vars)
        {
            var var = vars[^1];
            if (equations.Count == 1)
                return equations[0].InnerSimplified.SolveEquation(var).InnerSimplified is FiniteSet els 
                       ? els.Select(sol => new List<Entity> { sol }).ToList()
                       : new();
            var result = new List<List<Entity>>();
            var replacements = new Dictionary<Variable, Entity>();
            for (int i = 0; i < equations.Count; i++)
                if (equations[i].ContainsNode(var))
                {
                    var solutionsOverVar = equations[i].SolveEquation(var).InnerSimplified;
                    equations.RemoveAt(i);
                    vars = vars.Slice(0, vars.Length - 1);

                    if (solutionsOverVar is FiniteSet sols)
                        foreach (var sol in sols)
                        foreach (var j in InSolveSystem(equations.Select(eq => eq.Substitute(var, sol)).ToList(), vars))
                        {
                            replacements.Clear();
                            for (int varid = 0; varid < vars.Length; varid++)
                                replacements.Add(vars[varid], j[varid]);
                            j.Add(sol.Substitute(replacements).InnerSimplified);
                            result.Add(j);
                        }
                    break;
                }
            return result;
        }
    }
}