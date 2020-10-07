using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    class ExponentialSolver
    {
        internal static Entity.SetNode? SolveLinear(Entity expr, Entity.Variable x)
        {
            var linearized = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.Contains(x));
            // TODO: exponential solver in future releases
            return null;
        }
    }
}
