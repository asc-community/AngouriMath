//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using PeterO.Numbers;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static class ExpressionNumerical
    {
        /// <summary>
        /// Checks if two expressions are equivalent if 
        /// <see cref="Entity.Simplify"/> does not give the
        /// expected response
        /// </summary>
        internal static bool AreEqual(Entity expr1, Entity expr2, Entity[] checkPoints)
        {
            var vars1 = expr1.Vars.OrderBy(v => v.Name).ToArray();
            var vars2 = expr2.Vars.OrderBy(v => v.Name).ToArray();
            var vars = expr1.Vars.Concat(expr2.Vars).ToSet();

            static int TrueRemainder(int left, int right)
            {
                var rem = left % right;
                if (rem < 0)
                    rem += right;
                return rem;
            }

            for (var offset = 0; offset < checkPoints.Length; offset++)
            {
                var expr1Subs = expr1;
                var expr2Subs = expr2;
                foreach (Variable var in vars)
                {
                    var hash = var.GetHashCode();
                    var idToTakeFrom = TrueRemainder(hash + offset, checkPoints.Length);
                    var evaled = checkPoints[idToTakeFrom];
                    expr1Subs = expr1Subs.Substitute(var, evaled);
                    expr2Subs = expr2Subs.Substitute(var, evaled);
                }
                var evaled1 = expr1Subs.EvalNumerical();
                var evaled2 = expr2Subs.EvalNumerical();

                // TODO: should we consider NaN = {} to be true?
                if (evaled1 != evaled2 && evaled1 != MathS.NaN && evaled2 != MathS.NaN && !AgreeToHalfThePrecision(evaled1, evaled2))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Whether two values differ by no more than half the evaluation's digits, relative to the larger
        /// of them or to one. Two routes to one value differ in their last digits: the derivative of
        /// <c>x ln(x)</c>'s antiderivative, <c>2 x (-1/4 + ln(x)/2) + 1/x * 2/4 * x^2</c>, is <c>x ln(x)</c>,
        /// and compared exactly at -100 and 100 the two read as unequal. A value that is not finite agrees
        /// only with itself.
        /// https://github.com/asc-community/AngouriMath/issues/1674
        /// </summary>
        private static bool AgreeToHalfThePrecision(Number first, Number second)
        {
            if (first is not Number.Complex a || second is not Number.Complex b || !a.IsFinite || !b.IsFinite)
                return false;
            var digits = MathS.Settings.DecimalPrecisionContext.Value.Precision;
            var tolerance = EDecimal.FromString("1E-" + digits.Divide(2));
            var scale = EDecimal.Max(EDecimal.One, EDecimal.Max(a.Abs().EDecimal, b.Abs().EDecimal));
            return ((Number.Complex)(a - b)).Abs().EDecimal.CompareTo(scale.Multiply(tolerance)) <= 0;
        }

        /// <summary>
        /// Checks if two expressions are equivalent if 
        /// <see cref="Entity.Simplify"/> does not give the
        /// expected response
        /// </summary>
        internal static bool AreEqual(Entity expr1, Entity expr2)
            => AreEqual(expr1, expr2, MathS.UnsafeAndInternal.CheckPoints);
    }
}
