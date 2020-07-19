
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


using System.Collections.Generic;
using AngouriMath.Core.TreeAnalysis.Division.RationalDiv;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer 
    {   
        /// <summary>
        /// Returns information about all monomials of an expression
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="replaceVars"></param>
        /// <returns></returns>
        internal static PolyInfo GatherAllPossiblePolynomials(Entity expr, bool replaceVars)
        {
            // TODO: refactor

            expr = expr.DeepCopy();

            // Init
            var res = new PolyInfo();
            var mentionedVarList = MathS.Utils.GetUniqueVariables(expr);
            var newList = new List<string>();

            if (replaceVars)
            {
                // Replace all variables we can
                foreach (var varMentioned in mentionedVarList.FiniteSet())
                {
                    if (expr.FindSubtree(varMentioned) is null)
                        continue;
                    var replacement = TreeAnalyzer.GetMinimumSubtree(expr, varMentioned);
                    res.replacementInfo[varMentioned.Name] = replacement;
                    FindAndReplace(ref expr, replacement, new VariableEntity(PolyInfo.NewVarName(varMentioned.Name)));
                    newList.Add(PolyInfo.NewVarName(varMentioned.Name));
                }
            }
            else
            {
                foreach(var v in mentionedVarList.FiniteSet())
                {
                    newList.Add(v.Name);
                }
            }

            // Gather info about each var as if this var was the only argument of the polynomial P(x)
            foreach (var varMentioned in newList)
            {
                List<Entity> children;
                if (expr is OperatorEntity && expr.Name == "sumf" || expr.Name == "minusf")
                    children = LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
                else
                    children = new List<Entity> { expr };
                if (PolynomialSolver.GatherMonomialInformation<EDecimal>(children, MathS.Var(varMentioned)) is { } info)
                    res.monoInfo[varMentioned] = info;
            }

            return res;
        }
    }
}


namespace AngouriMath.Core.TreeAnalysis.Division.RationalDiv
{
    using MonomialInfo = Dictionary<string, Dictionary<EDecimal, Entity>>;
    using ReplacementInfo = Dictionary<string, Entity>;
    internal class PolyInfo
    {
        internal MonomialInfo monoInfo = new MonomialInfo();
        internal ReplacementInfo replacementInfo = new ReplacementInfo();
        internal static string NewVarName(string oldName) => oldName + "_r";
        internal static string OldVarName(string newName) => newName.Substring(0, newName.Length - 2);
    }
}
