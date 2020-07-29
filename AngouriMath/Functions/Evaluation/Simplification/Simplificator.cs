
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
        internal static Entity Simplify(Entity expr) => Simplify(expr, 2);

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
                return new Set(src.Copy());
            var stage1 = src.InnerSimplify();
            if (stage1 is NumberEntity)
                return new Set(stage1);

            var history = new SortedDictionary<int, List<Entity>>();

            static void TryInnerSimplify(ref Entity expr)
            {
                TreeAnalyzer.Sort(ref expr, TreeAnalyzer.SortLevel.HIGH_LEVEL);
                expr = expr.InnerSimplify();
            }

            // List of criterians of expr's complexity
            static int CountExpressionComplexity(Entity expr)
            => MathS.Settings.ComplexityCriteria.Value(expr);

            void __IterAddHistory(Entity expr)
            {
                Entity refexpr = expr.DeepCopy();
                TryInnerSimplify(ref refexpr);
                var compl1 = CountExpressionComplexity(refexpr);
                var compl2 = CountExpressionComplexity(expr);
                var n = compl1 > compl2 ? expr : refexpr;
                var ncompl = Math.Min(compl2, compl1);
                if (!history.ContainsKey(ncompl))
                    history[ncompl] = new List<Entity>();
                history[ncompl].Add(n);
            }
            
            void AddHistory(Entity expr)
            {
                __IterAddHistory(expr);
                Entity _res = expr;
                TreeAnalyzer.InvertNegativePowers(ref _res);
                __IterAddHistory(_res);
            }

            AddHistory(stage1);
            var res = stage1.DeepCopy();

            for (int i = 0; i < Math.Abs(level); i++)
            {
                if (i == 0 || i > 2)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.HIGH_LEVEL);
                else if (i == 1)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.MIDDLE_LEVEL);
                else if (i == 2)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.LOW_LEVEL);
                res = res.InnerSimplify();
                if (TreeAnalyzer.Optimization.ContainsPower(res))
                {
                    TreeAnalyzer.ReplaceInPlace(Patterns.PowerRules, ref res);
                    AddHistory(res);
                }

                {
                    TreeAnalyzer.InvertNegativePowers(ref res);
                    TreeAnalyzer.InvertNegativeMultipliers(ref res);
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.HIGH_LEVEL);
                    AddHistory(res);
                    res = res.InnerSimplify();
                    TreeAnalyzer.ReplaceInPlace(Patterns.CommonRules, ref res);
                    AddHistory(res);
                    TreeAnalyzer.InvertNegativePowers(ref res);
                }

                {
                    TreeAnalyzer.InvertNegativePowers(ref res);
                    TreeAnalyzer.ReplaceInPlace(Patterns.DivisionPreparingRules, ref res);
                    res = res.InnerSimplify();
                    TreeAnalyzer.FindDivisors(ref res, (num, denom) => !MathS.CanBeEvaluated(num) && !MathS.CanBeEvaluated(denom));
                }

                res = res.InnerSimplify();
                if (TreeAnalyzer.Optimization.ContainsTrigonometric(res))
                {
                    var res1 = res.DeepCopy();
                    TreeAnalyzer.ReplaceInPlace(Patterns.TrigonometricRules, ref res);
                    AddHistory(res);
                    TreeAnalyzer.ReplaceInPlace(Patterns.ExpandTrigonometricRules, ref res1);
                    AddHistory(res1);
                    res = res.Complexity() > res1.Complexity() ? res1 : res;
                }
                if (TreeAnalyzer.Optimization.ContainsPower(res))
                {
                    TreeAnalyzer.ReplaceInPlace(Patterns.PowerRules, ref res);
                    AddHistory(res);
                }
                AddHistory(res);
                res = history[history.Keys.Min()][0].DeepCopy();
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
