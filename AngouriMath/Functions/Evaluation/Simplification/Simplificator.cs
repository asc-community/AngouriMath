
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

using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Numerix;


namespace AngouriMath.Functions.Evaluation.Simplification
{
    internal static class Simplificator
    {
        /// <summary>
        /// See more details in Entity.Simplify
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        internal static Entity Simplify(Entity expr, int level) => ((Entity)Alternate(expr, level).Pieces[0]).InnerSimplify();

        /// <summary>
        /// Finds all alternative forms of an expression
        /// </summary>
        /// <param name="src"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        internal static Set Alternate(Entity src, int level)
        {
            if (src is NumberEntity || src is VariableEntity)
                return new Set(src);
            var stage1 = src.InnerSimplify();
            if (stage1 is NumberEntity)
                return new Set(stage1);

            var history = new SortedDictionary<int, List<Entity>>();

            // List of criterians of expr's complexity
            static int CountExpressionComplexity(Entity expr) => MathS.Settings.ComplexityCriteria.Value(expr);
            
            void AddHistory(Entity expr)
            {
                void __IterAddHistory(Entity expr)
                {
                    var refexpr = expr.Replace(Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL)).InnerSimplify();
                    var compl1 = CountExpressionComplexity(refexpr);
                    var compl2 = CountExpressionComplexity(expr);
                    var n = compl1 > compl2 ? expr : refexpr;
                    var ncompl = Math.Min(compl2, compl1);
                    if (history.TryGetValue(ncompl, out var ncomplList))
                        ncomplList.Add(n);
                    history[ncompl] = new List<Entity> { n };
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
                if (res.Any(child => child is Powf))
                {
                    res = res.Replace(Patterns.PowerRules);
                    AddHistory(res);
                }

                {
                    res = res.Replace(
                        Patterns.InvertNegativePowers,
                        Patterns.InvertNegativeMultipliers,
                        Patterns.SortRules(TreeAnalyzer.SortLevel.HIGH_LEVEL));
                    AddHistory(res);
                    res = res.InnerSimplify().Replace(Patterns.CommonRules);
                    AddHistory(res);
                    res = res.Replace(Patterns.InvertNegativePowers);
                }

                {
                    res = res.Replace(Patterns.InvertNegativePowers, Patterns.DivisionPreparingRules).InnerSimplify();
                    AddHistory(res);
                    TreeAnalyzer.FindDivisors(ref res, (num, denom) => !MathS.CanBeEvaluated(num) && !MathS.CanBeEvaluated(denom));
                    res = res.InnerSimplify();
                    AddHistory(res);
                }

                if (res.Any(child => child is
                    Sinf or Cosf or Tanf or Cotanf or Arcsinf or Arccosf or Arctanf or Arccotanf))
                {
                    var res1 = res.Replace(Patterns.ExpandTrigonometricRules);
                    res = res.Replace(Patterns.TrigonometricRules);
                    AddHistory(res);
                    AddHistory(res1);
                    res = res.Complexity > res1.Complexity ? res1 : res;
                }
                if (res.Any(child => child is Factorialf))
                {
                    res = res.Replace(Patterns.ExpandFactorialDivisions);
                    AddHistory(res);
                    res = res.Replace(Patterns.CollapseFactorialMultiplications);
                    AddHistory(res);
                }
                if (res.Any(child => child is Powf))
                {
                    res = res.Replace(Patterns.PowerRules);
                    AddHistory(res);
                }
                AddHistory(res);

                {
                    // It is quite slow at this point
                    var listOfPossiblePolys = new List<Entity>();
                    // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
                    foreach (var variableEntity in res.Vars)
                    {
                        if (MathS.Utils.TryPolynomial(res, variableEntity, out var resPoly))
                            listOfPossiblePolys.Add(resPoly);
                    }
                    if (listOfPossiblePolys.Count != 0)
                    {
                        var min = listOfPossiblePolys.Min(c => c.Complexity);
                        var minPoly = listOfPossiblePolys.First(c => c.Complexity == min);
                        AddHistory(minPoly);
                        if (min < res.Complexity)
                            res = minPoly;
                    }
                }

                res = history[history.Keys.Min()][0];
            }
            if (level > 0) // if level < 0 we don't check whether expanded version is better
            {
                var expandedRaw = res.Expand();
                var expanded = expandedRaw.Simplify(-level);
                AddHistory(expanded);
                var collapsed = res.Collapse().Simplify(-level);
                AddHistory(collapsed);
            }

            var result = new Set();
            
            foreach (var pair in history)
                foreach (var el in pair.Value)
                    result.Add(el);
            
            return result;
        }
    }
}
