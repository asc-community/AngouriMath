
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



﻿using System;
 using System.Collections.Generic;
 using System.Linq;
 using AngouriMath.Core.Exceptions;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Functions.DiscreteMath;
 using PeterO.Numbers;

namespace AngouriMath.Core.TreeAnalysis
{
    internal static partial class TreeAnalyzer
    {
        // TODO: realize all methods
        
        /// <summary>
        /// Counts all combinations of roots, for example
        /// 3 ^ 0.5 + 4 ^ 0.25 will return a set of 8 different numbers
        /// </summary>
        /// <param name="expr"></param>
        /// <returns></returns>
        internal static Set EvalAll(Entity expr)
        {
            throw new NotImplementedException();
        }
        
        internal static void EvalCombs(Entity expr, Set set)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Finds out how many terms we get after expansion via binomial coefficients, e. g
        /// (a + b) ^ 3 -> 2, 3 -> 4
        /// </summary>
        /// <param name="numberOfTerms"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static EInteger EstimateTermCount(EInteger numberOfTerms, EInteger power)
            => Combinatorics.C(power + numberOfTerms - 1, power);

        /// <summary>
        /// Returns a list of linear children over sum
        /// where at most one term doesn't contain x, e. g.
        /// x2 + x + a + b + x
        /// =>
        /// [x2, x, a + b, x]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        internal static List<Entity>? GatherLinearChildrenOverAndExpand(Entity expr, Func<Entity, bool> conditionForUniqueTerms)
        {
            if (expr.Name != "sumf" && expr.Name != "minusf")
                return SmartExpandOver(expr, conditionForUniqueTerms);
            var res = new List<Entity>();
            Entity freeTerm = 0;
            foreach (var child in TreeAnalyzer.LinearChildrenOverSum(expr))
                if (conditionForUniqueTerms(child))
                {
                    var expanded = SmartExpandOver(child, conditionForUniqueTerms);
                    if (expanded is null)
                        return null;
                    res.AddRange(expanded);
                }
                else
                    freeTerm += child;
            res.Add(freeTerm);
            return res;
        }

        /// <summary>
        /// expr is NEITHER + NOR -
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        internal static List<Entity>? SmartExpandOver(Entity expr, Func<Entity, bool> conditionForUniqueTerms)
        {
            var keepResult = new List<Entity> { expr };
            if (!(expr is OperatorEntity || expr is FunctionEntity))
                return keepResult;
            var newChildren = new List<Entity>();
            var result = new List<Entity>();
            switch (expr.Name)
            {
                case "sumf":
                case "minusf":
                    throw new SysException("SmartExpandOver must be only called of non-sum expression");
                case "divf":
                    ExpandFactorialDivisions(ref expr);
                    if (expr.Name != "divf") return SmartExpandOver(expr, conditionForUniqueTerms);
                    var numChildren = GatherLinearChildrenOverAndExpand(expr.GetChild(0), conditionForUniqueTerms);
                    if (numChildren is null)
                        return null;
                    if (numChildren.Count > MathS.Settings.MaxExpansionTermCount)
                        return null;
                    return numChildren.Select(c => c / expr.GetChild(1)).ToList();
                case "mulf":
                    var oneChildren = GatherLinearChildrenOverAndExpand(expr.GetChild(0), conditionForUniqueTerms);
                    var twoChildren = GatherLinearChildrenOverAndExpand(expr.GetChild(1), conditionForUniqueTerms);
                    if (oneChildren is null || twoChildren is null)
                        return null;
                    if (oneChildren.Count * twoChildren.Count > MathS.Settings.MaxExpansionTermCount)
                        return null;
                    foreach (var one in oneChildren)
                    foreach (var two in twoChildren)
                        newChildren.Add(one * two);
                    return newChildren;
                case "powf":
                    if (!(expr.GetChild(1) is NumberEntity { Value:IntegerNumber { Value: var power } } && power >= 1))
                        return keepResult;
                    var linBaseChildren = GatherLinearChildrenOverAndExpand(expr.GetChild(0), conditionForUniqueTerms);
                    if (linBaseChildren is null)
                        return null;
                    if (linBaseChildren.Count == 1)
                    {
                        var baseChild = linBaseChildren[0];
                        if (!(baseChild is OperatorEntity))
                            return new List<Entity> { expr };
                        if (baseChild.Name != "divf" && baseChild.Name != "mulf")
                            return new List<Entity> { expr };
                        // (a / b)^2 = a^2 / b^2
                        baseChild.SetChild(0, baseChild.GetChild(0).Pow(expr.GetChild(1)));
                        baseChild.SetChild(1, baseChild.GetChild(1).Pow(expr.GetChild(1)));
                        return new List<Entity> {baseChild};
                    }
                    if (power > 20 && linBaseChildren.Count > 1 ||
                        EstimateTermCount(linBaseChildren.Count, power) >
                        EInteger.FromInt32(MathS.Settings.MaxExpansionTermCount))
                        return null;
                    foreach (var powerListForTerm in Combinatorics.CombinateSums(linBaseChildren.Count, power))
                    {
                        EInteger biCoef = 1;
                        EInteger sumPow = power;
                        foreach (var pow in powerListForTerm)
                        {
                            biCoef *= Combinatorics.C(sumPow, pow);
                            sumPow -= pow;
                        }
                        Entity term = IntegerNumber.Create(biCoef);
                        for (int i = 0; i < powerListForTerm.Count; i++)
                            if (powerListForTerm[i].Equals(1))
                                term *= linBaseChildren[i];
                            else if (powerListForTerm[i] > 1)
                                term *= MathS.Pow(linBaseChildren[i], IntegerNumber.Create(powerListForTerm[i]));
                        newChildren.AddRange(SmartExpandOver(term, conditionForUniqueTerms));
                    }
                    return newChildren;
            }

            return new List<Entity> { expr };
        }
    }
}
