using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;
//[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Samples")]

namespace AngouriMath.Functions
{
    using static Entity;
    internal static partial class TreeAnalyzer
    {
        public static Entity Factorize(Entity res)
        {
            if (res is Number or Variable)
                return res;
            var setOfConstsAndVars = res.VarsAndConsts;
            if (setOfConstsAndVars.Count == 0)
                return res;
            Variable folded;
            var replacements = new Dictionary<Variable, Entity>();
            while (!(res is Variable v && (folded = v) is var _))
                foreach (var var in res.VarsAndConsts)
                {
                    // Get var's Parent
                    var varParent =
                        res.Nodes
                        .TakeWhile(e => e != var) // Requires Entity enumeration to be depth-first!!
                        .Where(e => e.ContainsNode(var)) // e.g. when expr is sin((x+1)^2)+3, this step results in [sin((x+1)^2)+3, sin((x+1)^2), (x+1)^2, x+1]
                        .LastOrDefault(); // e.g. x -> x+1
                    // if varParent is null, either
                    // TakeWhile returned empty: it's root => our expression is just one variable;
                    // or Where returned empty: this variable already got replaced with something else
                    if (varParent is null)
                        break;
                    var key = Variable.CreateTemp(res.Vars.Concat(replacements.Keys));
                    replacements.Add(key, varParent);
                    res = res.Substitute(varParent, key);
                }

            // so by this point we have one variable, let's unfold yet back!
            res = replacements[folded];
            replacements.Remove(folded);
            while (replacements.Count > 0)
            {
                var currVars = res.VarsAndConsts;
                var currState = res;
                res = 0;
                foreach (var currVar in currVars)
                {
                    static (Entity poly, Entity remainder) ExtractPolynomialLocally(Entity expr, Variable x)
                    {
                        var (powInfo, rem) =
                            PolynomialSolver.GatherMonomialInformationAllowingBad<EInteger, PrimitiveInteger>(Sumf.LinearChildren(expr), x);
                        if (
                            powInfo.TryGetValue(EInteger.Zero, out var info) // if we have something line x^5 + a + a over x then we should get (x^5, a + a) so that we could continute working with the remainder
                        )
                        {
                            rem += info;
                            powInfo.Remove(EInteger.Zero);
                        }
                        return (Simplificator.BuildPoly(powInfo, x) ?? 0, rem);
                    }
                    var (poly, rem) = ExtractPolynomialLocally(currState, currVar);
                    res += poly;
                    currState = rem;
                }

                foreach (var _ in currVars) // double loop because some variables contain others parallely
                    foreach (var currVar in currVars)
                        // If not in replacements, it's one of variables we already had (input variables)
                        if (replacements.TryGetValue(currVar, out var replacement))
                            res = res.Substitute(currVar, replacement);
                foreach (var currVar in currVars)
                    replacements.Remove(currVar);
            }
            return res;
        }
    }
}