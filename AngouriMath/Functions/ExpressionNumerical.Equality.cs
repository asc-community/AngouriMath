using AngouriMath.Extensions;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
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
            var vars1 = expr1.Vars.ToArray();
            var vars2 = expr2.Vars.ToArray();
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
                offset++;
                var expr1Subs = expr1;
                var expr2Subs = expr2;
                foreach (Variable var in vars)
                {
                    var idToTakeFrom = TrueRemainder(var.GetHashCode() + offset, checkPoints.Length);
                    var evaled = checkPoints[idToTakeFrom].EvalNumerical();
                    expr1Subs = expr1Subs.Substitute(var, evaled);
                    expr2Subs = expr2Subs.Substitute(var, evaled);
                }
                if (expr1Subs.EvalNumerical() != expr2Subs.EvalNumerical())
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
            => AreEqual(expr1, expr2, MathS.Internal.CheckPoints);
    }
}
