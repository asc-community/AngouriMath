using AngouriMath.Functions.Boolean.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Text;

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
            => throw new NotImplementedException("Piecewise required");
    }
}
