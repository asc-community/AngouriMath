using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalInequalitySolver
    {
        /// <summary>
        /// Considers expr > 0
        /// </summary>
        internal static Set Solve(Entity expr, Variable x)
        {
            if (expr is Minusf(var v, var c) && v == x && !c.ContainsNode(x))
                return new Interval(c, false, Number.Real.PositiveInfinity, false);
            if (expr is Minusf(var c1, var v1) && v1 == x && !c1.ContainsNode(x))
                return new Interval(Number.Real.NegativeInfinity, false, c1, false);
            throw FutureReleaseException.Raised("Inequalities are not implemented yet", "1.2.1");
        }
    }
}
