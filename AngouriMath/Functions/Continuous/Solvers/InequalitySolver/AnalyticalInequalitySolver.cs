using System;
using System.Collections.Generic;
using System.Text;
using static AngouriMath.Entity;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalInequalitySolver
    {
        /// <summary>
        /// Considers expr > 0
        /// </summary>
        internal static SetNode Solve(Entity expr, Variable x)
        {
            if (expr is Minusf(var v, var c) && v == x && !c.Contains(x))
                return new Set(MathS.Sets.Interval(c, Number.Real.PositiveInfinity));
            throw new NotImplementedException("Inequalities are not implemented yet");
        }
    }
}
