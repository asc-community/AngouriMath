using AngouriMath.Core.Exceptions;
using AngouriMath.Functions.Boolean.AnalyticalSolving;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Solves the given equation over <paramref name="x"/>
        /// symbolically
        /// </summary>
        public Set SolveBoolean(Variable x)
            => BooleanAnalyticalSolver.SolveBoolean(this, x);
    }
}

namespace AngouriMath.Functions.Boolean.AnalyticalSolving
{
    using static Entity;
    internal static class BooleanAnalyticalSolver
    {
        internal static Set SolveBoolean(Entity expr, Variable x)
            => throw FutureReleaseException.Raised("Piecewise", "1.2.1");
    }
}
