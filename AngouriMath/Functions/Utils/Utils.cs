
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngouriMath.Core;
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

        /// <summary>
        /// Rounds numbers to the given precision
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        internal static Number CutoffImprecision(Number num)
            => new Number(num.Re - num.Re % MathS.Utils.EQUALITY_THRESHOLD,
                num.Im - num.Im % MathS.Utils.EQUALITY_THRESHOLD);


        internal static (string prefix, int num) ParseIndex(string name)
        {
            int pos_ = name.IndexOf('_');
            if (pos_ != -1)
            {
                string varName = name.Substring(0, pos_);
                int num;
                if (Int32.TryParse(name.Substring(pos_ + 1), out num))
                {
                    return (varName, num);
                }
            }
            return (null, 0);
        }

        /// <summary>
        /// Finds next var index name starting with 1, e. g.
        /// x + n_0 + n_a + n_3 + n_1
        /// will find n_2
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        internal static VariableEntity FindNextIndex(Entity expr, string prefix)
        {
            var indices = new HashSet<int>();
            foreach (var var in MathS.Utils.GetUniqueVariables(expr).FiniteSet())
            {
                var index = ParseIndex(var.Name);
                if (index.prefix == prefix)
                    indices.Add(index.num);
            }
            int i = 1;
            while (indices.Contains(i))
                i++;
            return new VariableEntity(prefix + "_" + i);
        }
    }
}
