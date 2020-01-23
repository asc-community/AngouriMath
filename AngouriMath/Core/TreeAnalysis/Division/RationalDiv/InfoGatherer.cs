using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Core.TreeAnalysis.Division.RationalDiv;
using AngouriMath.Functions.Algebra.AnalyticalSolver;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer 
    {   
        internal static PolyInfo GatherAllPossiblePolynomials(Entity expr)
        {
            // Init
            var res = new PolyInfo();
            var mentionedVarList = MathS.GetUniqueVariables(expr);
            var newList = new List<string>();

            // Replace all variables we can
            foreach (var varMentioned in mentionedVarList)
            {
                var replacement = TreeAnalyzer.GetMinimumSubtree(expr, varMentioned);
                res.replacementInfo[varMentioned.Name] = replacement;
                expr = expr.FindAndReplace(replacement, new VariableEntity(PolyInfo.NewVarName(varMentioned.Name)));
                newList.Add(PolyInfo.NewVarName(varMentioned.Name));
            }

            // Gather info about each var as if this var was the only argument of the polynomial P(x)
            List<Entity> children;
            foreach (var varMentioned in newList)
            {
                if (expr.entType == Entity.EntType.OPERATOR && expr.Name == "sumf" || expr.Name == "minusf")
                    children = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
                else
                    children = new List<Entity> { expr };
                res.monoInfo[varMentioned] = PolynomialSolver.GatherMonomialInformation<double>(children, MathS.Var(varMentioned));
            }

            return res;
        }
    }
}


namespace AngouriMath.Core.TreeAnalysis.Division.RationalDiv
{
    using MonomialInfo = Dictionary<string, Dictionary<double, Entity>>;
    using ReplacementInfo = Dictionary<string, Entity>;
    public class PolyInfo
    {
        public MonomialInfo monoInfo = new MonomialInfo();
        public ReplacementInfo replacementInfo = new ReplacementInfo();
        internal static string NewVarName(string oldName) => oldName + "_r";
        internal static string OldVarName(string newName) => newName.Substring(0, newName.Length - 2);
    }
}
