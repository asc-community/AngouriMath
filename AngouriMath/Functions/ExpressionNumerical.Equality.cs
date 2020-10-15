using AngouriMath.Extensions;
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

            var expr1Compiled = expr1.Compile(vars1);
            var expr2Compiled = expr2.Compile(vars2);

            Dictionary<Variable, int> order1 = new();
            for (int i = 0; i < vars1.Length; i++)
                order1[vars1[i]] = i;
            Dictionary<Variable, int> order2 = new();
            for (int i = 0; i < vars2.Length; i++)
                order2[vars2[i]] = i;

            var toSub1 = new Complex[vars1.Length];
            var toSub2 = new Complex[vars2.Length];

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
                foreach (Variable var in vars)
                {
                    var idToTakeFrom = TrueRemainder(var.GetHashCode() + offset, checkPoints.Length);
                    if (order1.ContainsKey(var))
                        toSub1[order1[var]] = (Complex)checkPoints[idToTakeFrom].EvalNumerical();
                    if (order2.ContainsKey(var))
                        toSub2[order2[var]] = (Complex)checkPoints[idToTakeFrom].EvalNumerical();
                }
                var val1 = expr1Compiled.Call(toSub1);
                var val2 = expr2Compiled.Call(toSub2);
                if (val1 != val2)
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
