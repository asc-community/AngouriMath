
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
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using FractionInfoList = List<(Entity numerator, List<Entity> denominatorMultipliers)>;
    internal static class CommonDenominatorSolver
    {
        

        /// <summary>
        /// All constants, no matter multiplied or divided, are numerator's coefficients:
        /// 2 * x / 3 => num: 2 / 3
        /// 
        /// All entities that contain x and have a real negative power are denominator's multipliers
        /// 2 / (x + 3) => den: [x + 3]
        /// 2 / ((x^(-1) + 3) * (x2 + 1)) => den: [x^(-1) + 3, x2 + 1]
        /// 
        /// All entities that have complex power are considered as product of an entity with a real power
        /// and an entity whole power's real part is 0
        /// x ^ (-1 + 2i) => num: x ^ (2i), den: [x]
        /// x ^ (3 - 2i) => num: x ^ (3 - 2i), den: []
        /// 
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        internal static (Entity numerator, List<(Entity den, RealNumber pow)> denominatorMultipliers) FindFractions(Entity term, VariableEntity x)
        {
            // TODO: consider cases where we should NOT gather all powers in row
            static Entity GetPower(Entity expr)
            {
                if (!(expr is OperatorEntity { Name: "powf" }))
                    return 1;
                else
                    return expr.GetChild(1) * GetPower(expr.GetChild(0));
            }

            static Entity GetBase(Entity expr)
            {
                if (!(expr is OperatorEntity { Name: "powf" }))
                    return expr;
                else
                    return GetBase(expr.GetChild(0));
            }

            (Entity numerator, List<(Entity den, RealNumber pow)> denominatorMultipliers) oneInfo;
            oneInfo.numerator = 1; // Once InnerSimplify is called, we get rid of 1 *
            oneInfo.denominatorMultipliers = new List<(Entity den, RealNumber pow)>();

            var multipliers = TreeAnalyzer.LinearChildren(term, "mulf", "divf", Const.FuncIfMul);
            var res = new FractionInfoList();
            foreach (var multiplier in multipliers)
            {
                if (!multiplier.SubtreeIsFound(x))
                {
                    oneInfo.numerator *= multiplier;
                    continue;
                }
                var power = GetPower(multiplier);
                if (!MathS.CanBeEvaluated(power))
                {
                    oneInfo.numerator *= multiplier;
                    continue;
                }
                var preciseValue = power.Eval();
                if (!(preciseValue is RealNumber realPart))
                {
                    oneInfo.numerator *= multiplier;
                    continue;
                }
                if (realPart > 0)
                {
                    oneInfo.numerator *= multiplier;
                    continue;
                }
                oneInfo.denominatorMultipliers.Add((GetBase(multiplier), realPart));
            }

            return oneInfo;
        }

        /// <summary>
        /// Finds the best common denominator, multiplies the whole expression by that, and
        /// tries solving if the found denominator is not 1
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        internal static Set? Solve(Entity expr, VariableEntity x)
        {
            var res = FindCD(expr, x);
            return res?.SolveEquation(x);
        }

        internal static Entity? FindCD(Entity expr, VariableEntity x)
        {
            var terms = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);
            var denominators = new Dictionary<string, (Entity den, RealNumber pow)>();
            var fracs = new List<(Entity numerator, List<(Entity den, RealNumber pow)> denominatorMultipliers)>();
            foreach (var term in terms)
            {
                var oneInfo = FindFractions(term, x);
                foreach (var denMp in oneInfo.denominatorMultipliers)
                {
                    var (den, pow) = denMp;
                    var name = den.ToString(); // TODO: Replace with faster hashing
                    if (!denominators.ContainsKey(name))
                        denominators[name] = (den, 0);
                    denominators[name] = (den, Number.Max(denominators[name].pow, -pow));
                }
                fracs.Add(oneInfo);
            }

            if (denominators.Count == 0)
                return null; // If there's no denominators or it's equal to 1, then we don't have to try to solve yet anymore

            static Dictionary<string, (Entity den, RealNumber pow)> ToDict(List<(Entity den, RealNumber pow)> list)
            {
                var res = new Dictionary<string, (Entity den, RealNumber pow)>();
                foreach (var (den, pow) in list)
                    res[den.ToString()] = (den, -pow);
                return res;
            }

            var newTerms = new List<Entity>();
            foreach (var frac in fracs)
            {
                var (num, dens) = frac;
                var denDict = ToDict(dens);
                Entity invertDenominator = 1;
                foreach (var mp in denominators)
                {
                    if (denDict.ContainsKey(mp.Key))
                        invertDenominator *= MathS.Pow(mp.Value.den, denominators[mp.Key].pow - denDict[mp.Key].pow);
                    else
                        invertDenominator *= MathS.Pow(mp.Value.den, denominators[mp.Key].pow);
                }
                newTerms.Add(invertDenominator * num);
            }

            return TreeAnalyzer.MultiHangBinary(newTerms, "sumf", Const.PRIOR_SUM).InnerSimplify();
        }
    }
}
