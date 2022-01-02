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
                if (evaled1 != evaled2 && evaled1 != MathS.NaN && evaled2 != MathS.NaN)
                    return false;
            }

            return true;
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
