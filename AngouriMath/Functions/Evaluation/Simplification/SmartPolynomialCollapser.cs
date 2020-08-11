using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;

namespace AngouriMath.Functions.Evaluation.Simplification
{
    public static class SmartPolynomialCollapser
    {
        internal static (Entity polyCollapsed, Entity remainder) ExtractPolynomialLocally(Entity expr, VariableEntity x)
        {
            var children = TreeAnalyzer.LinearChildrenOverSum(expr);
            var (powInfo, rem) = PolynomialSolver.GatherMonomialInformationAllowingBad<EInteger>(children, x);
            if (
                powInfo.ContainsKey(EInteger.Zero) // if we have something line x^5 + a + a over x then we should get (x^5, a + a) so that we could continute working with the remainder
               )
            {
                rem += powInfo[EInteger.Zero];
                powInfo.Remove(EInteger.Zero);
            }
            var poly = Utils.BuildPoly(powInfo, x);
            if (poly is null)
                poly = 0;
            return (poly, rem);
        }

        public static void Collapse(ref Entity res)
        {
            Set setOfConstsAndVars;

            if (res.IsLeaf || (setOfConstsAndVars = TreeAnalyzer.GetUniqueVariablesAndConsts(res)).IsEmpty())
                return;
            
            const string replacingPrefix = "repl";
            var replacements = new Dictionary<string, Entity>();
            var currIndexRepl = Utils.FindMaxIndex(res, replacingPrefix) + 1;
            while (!res.IsLeaf)
            {
                foreach (VariableEntity var in setOfConstsAndVars )
                {
                    var nearestSubtree = res.FindSubtree(var);
                    if (nearestSubtree is {})
                    {
                        var key = replacingPrefix + "_" + (currIndexRepl++);
                        var val = nearestSubtree.Parent;
                        if (val is null)
                            break; // if one does not have parent, it's root => our expression is just one variable
                        replacements[key] = val;
                        TreeAnalyzer.FindAndReplace(ref res, val, (VariableEntity)key);
                    }
                    // else this variable already got replaced with something else
                }
                setOfConstsAndVars = TreeAnalyzer.GetUniqueVariablesAndConsts(res);
            }

            // so by this point we have one variable, let's unfold yet back!
            var oldResName = res.Name;
            res = replacements[res.Name];
            replacements.Remove(oldResName);
            while (replacements.Count > 0)
            {
                Entity newRes = 0;
                Entity currState = res;
                var currVars = TreeAnalyzer.GetUniqueVariablesAndConsts(res);
                foreach (VariableEntity currVar in currVars.FiniteSet())
                {
                    var (poly, rem) = ExtractPolynomialLocally(currState, currVar);
                    newRes += poly;
                    currState = rem;
                }

                foreach (var _ in currVars.FiniteSet()) // double loop because some variables contain others parallely
                foreach (VariableEntity currVar in currVars.FiniteSet())
                    if (replacements.ContainsKey(currVar.Name)) // otherwise it's one of variables we already had (input variables)
                        TreeAnalyzer.FindAndReplace(ref newRes, currVar, replacements[currVar.Name]);

                res = newRes;
                foreach (VariableEntity currVar in currVars.FiniteSet())
                    replacements.Remove(currVar.Name);
            }
            
        }
    }
}
