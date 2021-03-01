/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    partial class Patterns
    {
        private static Entity SumOfFractions(Entity expr,
            Entity leftNum, Entity leftDen, Entity rightNum, Entity rightDen)
        {
            // var leftInt = (leftNum.Vars, leftDen.Vars).IntersectSequences().Any();
            // var rightInt = (rightNum.Vars, rightDen.Vars).IntersectSequences().Any();
            var twoInt = ((leftNum + leftDen).Vars, (rightNum + rightDen).Vars).IntersectSequences().Any();
            if (twoInt)
                return (leftNum * rightDen + rightNum * leftDen).Simplify() / (rightDen * leftDen).Simplify();
            else
                return expr;
        }


        internal static Entity FractionCommonDenominatorRules(Entity expr)
            => expr switch
            {
                Sumf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen)) 
                    => SumOfFractions(expr, leftNum, leftDen, rightNum, rightDen),
                Minusf(Divf(var leftNum, var leftDen), Divf(var rightNum, var rightDen))
                    => SumOfFractions(expr, leftNum, leftDen, -rightNum, rightDen),
                _ => expr
            };
    }
}
