
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
using AngouriMath.Core;
using PeterO.Numbers;

namespace AngouriMath.Functions
{
    using static Entity;
    using static Entity.Number;
    internal static class Simplificator
    {
        /// <summary>See more details in <see cref="Entity.Simplify(int)"/></summary>
        internal static Entity Simplify(Entity expr, int level) => Alternate(expr, level).First().InnerSimplify();

        /// <summary>Finds all alternative forms of an expression</summary>
        internal static IEnumerable<Entity> Alternate(Entity src, int level)
        {
            if (src is Number || src is Variable)
                return new[] { src };
            var stage1 = src.InnerSimplify();
            if (stage1 is Number)
                return new[] { stage1 };

            // List of criteria for expr's complexity
            var history = new SortedDictionary<int, HashSet<Entity>>();
            void AddHistory(Entity expr)
            {
                void __IterAddHistory(Entity expr)
                {
                    static int CountExpressionComplexity(Entity expr) => MathS.Settings.ComplexityCriteria.Value(expr);
                    var refexpr = expr.Replace(Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL)).InnerSimplify();
                    var compl1 = CountExpressionComplexity(refexpr);
                    var compl2 = CountExpressionComplexity(expr);
                    var n = compl1 > compl2 ? expr : refexpr;
                    var ncompl = Math.Min(compl2, compl1);
                    if (history.TryGetValue(ncompl, out var ncomplList))
                        ncomplList.Add(n);
                    else history[ncompl] = new HashSet<Entity> { n };
                }
                __IterAddHistory(expr);
                __IterAddHistory(expr.Replace(Patterns.InvertNegativePowers));
            }

            AddHistory(stage1);
            var res = stage1;

            for (int i = 0; i < Math.Abs(level); i++)
            {
                res = res.Replace(Patterns.SortRules(i switch
                {
                    1 => TreeAnalyzer.SortLevel.MIDDLE_LEVEL,
                    2 => TreeAnalyzer.SortLevel.LOW_LEVEL,
                    _ => TreeAnalyzer.SortLevel.HIGH_LEVEL
                })).InnerSimplify();
                if (res.Nodes.Any(child => child is Powf))
                    AddHistory(res = res.Replace(Patterns.PowerRules).InnerSimplify());

                AddHistory(res = res.Replace(
                    Patterns.InvertNegativePowers,
                    Patterns.InvertNegativeMultipliers,
                    Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL)).InnerSimplify());
                AddHistory(res = res.Replace(Patterns.CommonRules).InnerSimplify());

                AddHistory(res = res.Replace(Patterns.InvertNegativePowers, Patterns.DivisionPreparingRules).InnerSimplify());
                AddHistory(res = res.Replace(Patterns.PolynomialLongDivision).InnerSimplify());

                if (res.Nodes.Any(child => child is
                    Sinf or Cosf or Tanf or Cotanf or Arcsinf or Arccosf or Arctanf or Arccotanf))
                {
                    var res1 = res.Replace(Patterns.ExpandTrigonometricRules).InnerSimplify();
                    AddHistory(res = res.Replace(Patterns.TrigonometricRules, Patterns.CommonRules).InnerSimplify());
                    AddHistory(res1);
                    res = res.Complexity > res1.Complexity ? res1 : res;
                }

                if (res.Nodes.Any(child => child is BooleanNode))
                {
                    AddHistory(res = res.Replace(Patterns.BooleanRules).InnerSimplify());
                }

                if (res.Nodes.Any(child => child is Factorialf))
                {
                    AddHistory(res = res.Replace(Patterns.ExpandFactorialDivisions).InnerSimplify());
                    AddHistory(res = res.Replace(Patterns.FactorizeFactorialMultiplications).InnerSimplify());
                }
                if (res.Nodes.Any(child => child is Powf))
                    AddHistory(res = res.Replace(Patterns.PowerRules).InnerSimplify());

                Entity? possiblePoly = null;
                foreach (var var in res.Vars)
                    if (TryPolynomial(res, var, out var resPoly)
                        && (possiblePoly is null || resPoly.Complexity < possiblePoly.Complexity))
                        AddHistory(possiblePoly = resPoly);
                if (possiblePoly is { } && possiblePoly.Complexity < res.Complexity)
                    res = possiblePoly;
                /*
                This was intended to simplify expressions as polynomials over nodes, some kind of
                greatest common node and simplifying over it. However, the current algorithm does
                not solve this issue completely and yet too slow to be accepted.

                AddHistory(res = TreeAnalyzer.Factorize(res));
                */

                res = history[history.Keys.Min()].First();
            }
            if (level > 0) // if level < 0 we don't check whether expanded version is better
            {
                AddHistory(res.Expand().Simplify(-level));
                AddHistory(res.Factorize().Simplify(-level));
            }

            return history.Values.SelectMany(x => x);
        }

        /// <summary>
        /// Sorts an expression into a polynomial.
        /// See more at <see cref="MathS.Utils.TryPolynomial"/>
        /// </summary>
        internal static bool TryPolynomial(Entity expr, Variable variable,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)]
            out Entity? dst)
        {
            dst = null;
            var children = Sumf.LinearChildren(expr.Expand());
            var monomialsByPower = Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                <EInteger, TreeAnalyzer.PrimitiveInteger>(children, variable);
            if (monomialsByPower == null)
                return false;
            var res = BuildPoly(monomialsByPower, variable);
            if (res is null)
                return false;
            dst = res;
            return true;
        }
        internal static Entity? BuildPoly(Dictionary<EInteger, Entity> monomialsByPower, Variable x)
        {
            var terms = new List<Entity>();
            foreach (var index in monomialsByPower.Keys.OrderByDescending(x => x))
            {
                var pair = new KeyValuePair<EInteger, Entity>(index, monomialsByPower[index]);
                if (pair.Key.IsZero)
                {
                    terms.Add(pair.Value.InnerSimplify());
                    continue;
                }

                var px = pair.Key.Equals(EInteger.One) ? x : MathS.Pow(x, pair.Key);
                terms.Add(pair.Value == 1 ? px : pair.Value.InnerSimplify() * px);
            }

            if (terms.Count == 0)
                return null;
            var dst = terms[0];
            for (int i = 1; i < terms.Count; i++)
                if (terms[i] is Mulf(Real { IsNegative: true } r, var m))
                    dst -= -r * m;
                else
                    dst += terms[i];
            return dst.InnerSimplify();
        }
    }
}