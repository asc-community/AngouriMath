using AngouriMath.Core.Exceptions;
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
            if (expr is Minusf(var v, var c) && v == x && !c.ContainsNode(x))
                return new Set(MathS.Sets.Interval(c, Number.Real.PositiveInfinity).SetLeftClosed(false).SetRightClosed(false));
            if (expr is Minusf(var c1, var v1) && v1 == x && !c1.ContainsNode(x))
                return new Set(MathS.Sets.Interval(Number.Real.NegativeInfinity, c1).SetLeftClosed(false).SetRightClosed(false));
            throw FutureReleaseException.Raised("Inequalities are not implemented yet", "1.2.1");
        }
    }
}
