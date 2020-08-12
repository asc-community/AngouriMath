
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
using System.Linq;
using AngouriMath.Core.Numerix;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer 
    {   
        internal static Dictionary<T, Entity>? ParseAsPolynomial<T>(Entity expr, VariableEntity x)
        {
            var children = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(
                 expr, entity => entity.Vars.Contains(x)
            );

            if (children is null)
                return null;

            var monomials = PolynomialSolver.GatherMonomialInformation<T>(children, x);
            if (monomials is null) return null;
            var filteredDictionary = new Dictionary<T, Entity>();
            foreach(var monomial in monomials)
            {
                var simplified = monomial.Value.InnerSimplify();
                if (simplified != IntegerNumber.Zero)
                {
                    filteredDictionary.Add(monomial.Key, simplified);
                }
            }
            return filteredDictionary;
        }

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
            var mentionedVarList = expr.Vars;
            var newList = new List<VariableEntity>();

            if (replaceVars)
            {
                var initialReplacements = new Dictionary<Entity, VariableEntity>();
                // Replace all variables we can
                foreach (var varMentioned in mentionedVarList)
                {
                    var replacement = GetMinimumSubtree(expr, varMentioned);
                    res.replacementInfo[varMentioned] = replacement;
                    FindAndReplace(ref expr, replacement, PolyInfo.NewVarName(varMentioned));
                    newList.Add(PolyInfo.NewVarName(varMentioned));
                }
            }
            else
                foreach(var v in mentionedVarList)
                    newList.Add(v.Name);

            // Gather info about each var as if this var was the only argument of the polynomial P(x)
            foreach (var varMentioned in newList)
            {
                var children = Sumf.LinearChildren(expr);
                if (PolynomialSolver.GatherMonomialInformation<EDecimal>(children, varMentioned) is { } info)
                    res.monoInfo[varMentioned] = info;
            }

            return res;
        }
        internal class PolyInfo
        {
            internal Dictionary<VariableEntity, Dictionary<EDecimal, Entity>> monoInfo = new();
            internal Dictionary<VariableEntity, Entity> replacementInfo = new();
            internal static VariableEntity NewVarName(VariableEntity oldName) => new(oldName.Name + "_r");
        }
    }
}