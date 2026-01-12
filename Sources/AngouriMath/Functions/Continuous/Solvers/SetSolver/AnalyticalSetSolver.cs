//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using AngouriMath.Functions.Algebra;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Continuous.Solvers.SetSolver
{
    internal static class AnalyticalSetSolver
    {
        internal static Set Solve(Entity left, Entity right, Variable x)
        {
            switch (left, right)
            {
                case (Providedf(var l, var predicate), _): return Solve(l, right, x).Filter(predicate, x);
                case (_, Providedf(var r, var predicate)): return Solve(left, r, x).Filter(predicate, x);
                case (Piecewise p, _): return EquationSolver.SolvePiecewise(p, x, (e, x) => Solve(e, right, x));
                case (_, Piecewise p): return EquationSolver.SolvePiecewise(p, x, (e, x) => Solve(left, e, x));
            }

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
