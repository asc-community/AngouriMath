//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

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
            {
                if (MathS.Utils.TryGetPolyLinear(expr, x, out var a, out var b))
                {
                    a = a.InnerSimplified;
                    b = b.InnerSimplified;
                    var root = PolynomialSolver.SolveLinear(a, b).First();
                    if (root is Complex and not Real)
                        return Empty;
                    if (a is Real { IsNegative: true })
                        return new Interval(Real.NegativeInfinity, false, root, false);
                    return new Interval(root, false, Real.PositiveInfinity, false);
                }
            }
            {
                if (MathS.Utils.TryGetPolyQuadratic(expr, x, out var a, out var b, out var c))
                {
                    a = a.InnerSimplified;
                    b = b.InnerSimplified;
                    c = c.InnerSimplified;
                    var roots = PolynomialSolver.SolveQuadratic(a, b, c);
                    if (roots.Any(c => c is Complex and not Real))
                        return Empty;
                    roots = TreeAnalyzer.SortRealsAndNonReals(roots);
                    var root1 = roots.First();
                    var root2 = roots.Last();
                    if (a is Real { IsNegative: true })
                        return new Interval(root1, false, root2, false);
                    return new Interval(Real.NegativeInfinity, false, root1, false)
                        .Unite(new Interval(root2, false, Real.PositiveInfinity, false));
                }
            }
            throw FutureReleaseException.Raised("Inequalities are not implemented yet", "1.2.1");
        }
    }
}
