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
            Entity GetPower(Entity expr)
            {
                if (expr.entType != Entity.EntType.OPERATOR || expr.Name != "powf")
                    return 1;
                else
                    return expr.Children[1] * GetPower(expr.Children[0]);
            }

            (Entity numerator, List<(Entity den, RealNumber pow)> denominatorMultipliers) oneInfo;
            oneInfo.numerator = 1; // Once InnerSimplify is called, we get rid of 1 *
            oneInfo.denominatorMultipliers = new List<(Entity den, RealNumber pow)>();

            var multipliers = TreeAnalyzer.LinearChildren(term, "mulf", "divf", Const.FuncIfMul);
            var res = new FractionInfoList();
            foreach (var multiplyer in multipliers)
            {
                if (multiplyer.FindSubtree(x) == null)
                {
                    oneInfo.numerator *= multiplyer;
                    continue;
                }
                var power = GetPower(multiplyer);
                if (!MathS.CanBeEvaluated(power))
                {
                    oneInfo.numerator *= multiplyer;
                    continue;
                }
                var preciseValue = power.Eval();
                if (preciseValue.IsImaginary())
                {
                    oneInfo.numerator *= multiplyer;
                    continue;
                }
                var realPart = preciseValue as RealNumber;
                if (realPart > 0)
                {
                    oneInfo.numerator *= multiplyer;
                    continue;
                }
                oneInfo.denominatorMultipliers.Add((multiplyer, realPart));
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
        internal static Set Solve(Entity expr, VariableEntity x)
            => FindCD(expr, x).SolveEquation(x);

        internal static Entity FindCD(Entity expr, VariableEntity x)
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
                    denominators[name] = (den, denominators[name].pow + pow);
                }
                fracs.Add(oneInfo);
            }

            if (denominators.Count == 0)
                return null; // If there's no denominators or it's equal to 1, then we don't have to try to solve yet anymore

            Dictionary<string, (Entity den, RealNumber pow)> ToDict(List<(Entity den, RealNumber pow)> list)
            {
                var res = new Dictionary<string, (Entity den, RealNumber pow)>();
                foreach (var el in list)
                    res[el.den.ToString()] = el;
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
