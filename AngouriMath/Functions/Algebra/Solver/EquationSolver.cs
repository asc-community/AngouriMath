
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



﻿using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace AngouriMath.Functions.Algebra.Solver
{
    internal static class EquationSolver
    {
        internal static EntitySet Solve(Entity equation, VariableEntity x)
        {
            var res = new EntitySet();
            AnalyticalSolver.Solve(equation.DeepCopy(), x, res);
            for (int i = 0; i < res.Count; i++)
                res[i] = res[i].InnerSimplify();
            return res;
        }
        internal static Tensor SolveSystem(List<Entity> equations, List<VariableEntity> vars)
        {
            if (equations.Count != vars.Count)
                throw new MathSException("Amount of equations must be equal to that of vars");
            foreach (var v in vars)
                if (!(v is VariableEntity))
                    throw new TreeException("Arguments passed under vars should be Variable Entities");
            equations = new List<Entity>(equations.Select(c => c));
            vars = new List<VariableEntity>(vars.Select(c => c));
            int initVarCount = vars.Count;
            for (int i = 0; i < equations.Count; i++)
                equations[i] = equations[i].InnerSimplify();

            var res = EquationSolver.InSolveSystem(equations, vars);

            foreach (var tuple in res)
                if (tuple.Count != initVarCount)
                    throw new SysException("InSolveSystem incorrect output");
            var result = new Tensor(res.Count, initVarCount);
            for (int i = 0; i < res.Count; i++)
                for (int j = 0; j < initVarCount; j++)
                    result[i, j] = res[i][j];
            return result;
        }
        internal static List<List<Entity>> InSolveSystemOne(Entity eq, VariableEntity var)
        {
            var result = new List<List<Entity>>();
            foreach (var sol in eq.Solve(var))
                result.Add(new List<Entity>() { sol });
            return result;
        }
        internal static List<List<Entity>> InSolveSystem(List<Entity> equations, List<VariableEntity> vars)
        {
            var var = vars.Last();
            vars = new List<VariableEntity>(vars); // copying
            List<List<Entity>> result;
            if (equations.Count == 1)
                return InSolveSystemOne(equations[0], var);
            else
                result = new List<List<Entity>>();
            for (int i = 0; i < equations.Count; i++)
                if (equations[i].FindSubtree(var) != null)
                {
                    var solutionsOverVar = equations[i].Solve(var);
                    equations.RemoveAt(i);
                    vars.RemoveAt(vars.Count - 1);
                    
                    foreach (var sol in solutionsOverVar)
                    {
                        var newequations = new List<Entity>();
                        for (int eqid = 0; eqid < equations.Count; eqid++)
                            newequations.Add(equations[eqid].Substitute(var, sol));
                        var newvars = new List<VariableEntity>();
                        for (int vrid = 0; vrid < vars.Count; vrid++)
                            newvars.Add(vars[vrid]);
                        var inSol = InSolveSystem(newequations, newvars);
                        for(int j = 0; j < inSol.Count; j++)
                        {
                            var Z = sol.DeepCopy();
                            for (int varid = 0; varid < newvars.Count; varid++)
                                Z = Z.Substitute(newvars[varid], inSol[j][varid]);

                            Z = Z.Simplify();
                            inSol[j].Add(Z);
                        }
                        result.AddRange(inSol);
                    }
                    break;
                }
            return result;
        }
    }
}
