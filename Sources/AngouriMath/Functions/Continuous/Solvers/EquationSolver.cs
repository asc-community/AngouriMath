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
            var remainingVars = vars.Slice(0, vars.Length - 1);
            // Which equation to eliminate `var` from is decided by whether it occurs in
            // one, and occurring is a syntactic question. Substituting an earlier variable
            // routinely leaves an occurrence that cancels: eliminating x_4 from a dense
            // 4x4 turns the next equation into x_1 + 2*x_2 + x_3 - (x_1 + x_2 + x_3 - 4) - 5,
            // where x_3 is written twice and worth nothing. Solving that for x_3 has no
            // answer, and committing to the first candidate meant the whole system was
            // then declared unsolvable -- https://github.com/asc-community/AngouriMath/issues/608.
            // So a candidate that yields nothing is passed over for the next one rather than ending the search.
            for (int i = 0; i < equations.Count; i++)
                if (equations[i].ContainsNode(var))
                {
                    if (equations[i].SolveEquation(var).InnerSimplified is not FiniteSet sols
                        || sols.Count == 0)
                        continue;

                    var rest = new List<Entity>(equations);
                    rest.RemoveAt(i);

                    foreach (var sol in sols)
                        foreach (var j in InSolveSystem(rest.Select(eq => eq.Substitute(var, sol)).ToList(), remainingVars))
                        {
                            replacements.Clear();
                            for (int varid = 0; varid < remainingVars.Length; varid++)
                                replacements.Add(remainingVars[varid], j[varid]);
                            j.Add(sol.Substitute(replacements).InnerSimplified);
                            result.Add(j);
                        }

                    if (result.Count > 0)
                        return result;
                }
            return result;
        }
        internal static Set SolvePiecewise(Piecewise piecewise, Variable x, Func<Entity, Variable, Set> solve)
        {
            Entity cond = true;
            var res = new List<Set>();
            foreach (var c in piecewise.Cases)
            {
                res.Add(solve(c.Expression, x).Filter(c.Predicate & cond, x));
                cond &= !c.Predicate;
            }
            return res.Unite();
        }
    }
}