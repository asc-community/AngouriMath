//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
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
        private static IEnumerable<Entity> PairwiseGrouping(Entity num, Entity den, TreeAnalyzer.SortLevel level)
        {
            var numFactors = Mulf.LinearChildren(num);
            var denFactors = Mulf.LinearChildren(den);
            var factors = new Dictionary<string, Entity>();
            foreach (var numFactor in numFactors)
            {
                var sorted = numFactor.SortHash(level);
                if (!factors.ContainsKey(sorted))
                    factors[sorted] = 1;
                factors[sorted] = (factors[sorted] * numFactor).InnerSimplified;
            }
            foreach (var denFactor in denFactors)
            {
                var sorted = denFactor.SortHash(level);
                if (!factors.ContainsKey(sorted))
                    factors[sorted] = 1;
                factors[sorted] = (factors[sorted] / denFactor).InnerSimplified;
            }
            return factors.Values;
        }

        internal static Entity FractionCommonDenominatorRules(Entity expr, TreeAnalyzer.SortLevel level)
            => expr switch
            {
                Sumf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen)) 
                    => SumOfFractions(expr, leftNum, leftDen, rightNum, rightDen),
                Minusf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen))
                    => SumOfFractions(expr, leftNum, leftDen, -rightNum, rightDen),
                Divf(var num, var den) when num.Vars.Any() && den.Vars.Any()
                    => PairwiseGrouping(num, den, level).Select(PowerRules).MultiplyAll().InnerSimplified.Replace(CollapseMultipleFractions),
                _ => expr
            };

        internal static Entity CollapseMultipleFractions(Entity expr)
            => expr switch
            {
                Powf(Divf(var a, var b), Integer { IsPositive: true } c) => a.Pow(c) / b.Pow(c),
                Powf(Mulf(var a, var b), Integer { IsPositive: true } c) => a.Pow(c) * b.Pow(c),

                Mulf(Divf(var a, var b), Divf(var c, var d)) => (a * c) / (b * d),
                Mulf(var a, Divf(var b, var c)) => (a * b) / c,
                Mulf(Divf(var a, var b), var c) => (a * c) / b,

                Divf(Divf(var a, var b), Divf(var c, var d)) => (a * d) / (b * c),
                Divf(Divf(var a, var b), var c) => a / (b * c),
                Divf(var a, Divf(var b, var c)) => (a * c) / b,

                _ => expr
            };
    }
}
