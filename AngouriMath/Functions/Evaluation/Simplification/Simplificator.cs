using AngouriMath.Core.TreeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Functions.Evaluation.Simplification
{
    internal static class Simplificator
    {
        internal static Entity Simplify(Entity expr) => Simplify(expr, 2);
        internal static Entity Simplify(Entity expr, int level) => Alternate(expr, level)[0].InnerSimplify();
        internal static EntitySet Alternate(Entity src, int level)
        {
            if (src.entType == Entity.EntType.NUMBER || src.entType == Entity.EntType.VARIABLE)
                return new EntitySet(false, src.Copy());
            var stage1 = src.InnerSimplify();
            if (stage1.entType == Entity.EntType.NUMBER)
                return new EntitySet(false, stage1);

            var history = new SortedDictionary<int, Entity>();

            void TryInnerSimplify(ref Entity expr)
            {
                TreeAnalyzer.Sort(ref expr, TreeAnalyzer.SortLevel.HIGH_LEVEL);
                expr = expr.InnerSimplify();
            }

            void __IterAddHistory(Entity expr)
            {
                Entity refexpr = expr.DeepCopy();
                TryInnerSimplify(ref refexpr);
                var n = refexpr.Complexity() > expr.Complexity() ? expr : refexpr;
                history[n.Complexity()] = n;
            }
            
            void AddHistory(Entity expr)
            {
                __IterAddHistory(expr);
                Entity _res = expr;
                TreeAnalyzer.InvertNegativePowers(ref _res);
                __IterAddHistory(_res);
            }

            AddHistory(stage1);
            Entity res = stage1;

            for (int i = 0; i < Math.Abs(level); i++)
            {
                if (i == 0 || i > 2)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.HIGH_LEVEL);
                else if (i == 1)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.MIDDLE_LEVEL);
                else if (i == 2)
                    TreeAnalyzer.Sort(ref res, TreeAnalyzer.SortLevel.LOW_LEVEL);
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
                res = history[history.Keys.Min()];
            }
            if (level > 0) // if level < 0 we don't check whether expanded version is better
            {
                var expanded = res.Expand().Simplify(-level);
                AddHistory(expanded);
                var collapsed = res.Collapse().Simplify(-level);
                AddHistory(collapsed);
            }
            return new EntitySet(history.Values);
        }
    }
}
