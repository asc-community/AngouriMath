/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    partial class Patterns
    {
        private static Entity SumOfFractions(Entity expr,
            Entity leftNum, Entity leftDen, Entity rightNum, Entity rightDen)
        {
            var twoInt = ((leftNum + leftDen).Vars, (rightNum + rightDen).Vars).IntersectSequences().Any();
            if (twoInt)
                return (leftNum * rightDen + rightNum * leftDen).Simplify() / (rightDen * leftDen).Simplify();
            else
                return expr;
        }

        // a * b * c / (b * c * d)
        // =>
        // a * (b / b) * (c / c) * 1/d
        private static Entity PairwiseGrouping(Entity num, Entity den, TreeAnalyzer.SortLevel level)
        {
            var numFactors = Mulf.LinearChildren(num);
            var denFactors = Mulf.LinearChildren(den);
            var factors = new Dictionary<string, Entity>();
            foreach (var numFactor in numFactors)
                factors[numFactor.SortHash(level)] = numFactor;
            foreach (var denFactor in denFactors)
            {
                var sorted = denFactor.SortHash(level);
                if (!factors.ContainsKey(sorted))
                    factors[sorted] = 1;
                factors[sorted] /= denFactor;
            }
            return TreeAnalyzer.MultiHangBinary(factors.Values.ToArray(), (a, b) => a * b);
        }

        internal static Entity FractionCommonDenominatorRules(Entity expr, TreeAnalyzer.SortLevel level)
            => expr switch
            {
                Sumf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen)) 
                    => SumOfFractions(expr, leftNum, leftDen, rightNum, rightDen),
                Minusf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen))
                    => SumOfFractions(expr, leftNum, leftDen, -rightNum, rightDen),
                Divf(var num, var den) when num.Vars.Any() && den.Vars.Any()
                    => PairwiseGrouping(num, den, level),
                _ => expr
            };
    }
}
