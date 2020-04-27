using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;

namespace AngouriMath.Functions
{
    internal static class Utils
    {
        /// <summary>
        /// Sorts an expression into a polynomial.
        /// See more MathS.Utils.TryPolynomial
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="variable"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
        internal static bool TryPolynomial(Entity expr, VariableEntity variable, out Entity dst)
        {
            dst = null;
            var children = TreeAnalyzer.LinearChildren(expr.Expand(), "sumf", "minusf", Const.FuncIfSum);
            var monomialsByPower = PolynomialSolver.GatherMonomialInformation<int>(children, variable);
            if (monomialsByPower == null)
                return false;
            var newMonomialsByPower = new Dictionary<int, Entity>();
            var keys = monomialsByPower.Keys.ToList();
            keys.Sort((i, i1) => (i < i1 ? 1 : (i > i1 ? -1 : 0)));
            var terms = new List<Entity>();
            foreach (var index in keys)
            {
                var pair = new KeyValuePair<int, Entity>(index, monomialsByPower[index]);
                Entity px;
                if (pair.Key == 0)
                {
                    terms.Add(pair.Value.Simplify());
                    continue;
                }

                if (pair.Key == 1)
                    px = variable;
                else
                    px = MathS.Pow(variable, pair.Key);
                if (pair.Value == 1)
                {
                    terms.Add(px);
                    continue;
                }
                else
                    terms.Add(pair.Value.Simplify() * px);
            }

            if (terms.Count == 0)
                return false;
            dst = terms[0];
            for (int i = 1; i < terms.Count; i++)
                if (terms[i].Name == "mulf" &&
                    terms[i].Children[0].entType == Entity.EntType.NUMBER &&
                    terms[i].Children[0].GetValue().IsReal() && terms[i].Children[0].GetValue().Re < 0)
                    dst -= ((-1) * terms[i].Children[0].GetValue()) * terms[i].Children[1];
                else
                    dst += terms[i];
            dst = dst.InnerSimplify();
            return true;
        }
    }
}
