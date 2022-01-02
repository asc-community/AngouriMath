//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Continuous.Solvers.SetSolver
{
    internal static class AnalyticalSetSolver
    {
        internal static Set Solve(Entity left, Entity right, Variable x)
        {
            left = left.Replace(Patterns.SetOperatorRules);
            right = right.Replace(Patterns.SetOperatorRules);
            if (left.DirectChildren.Count<Entity>(c => c == x) 
                +
                right.DirectChildren.Count<Entity>(c => c == x) != 1)
                return Empty;
            if (left.ContainsNode(x))
                return left.Invert(right, x).ToSet();
            else
                return right.Invert(left, x).ToSet();
        }
    }
}
