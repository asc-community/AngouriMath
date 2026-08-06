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
            switch (expr)
            {
                case Providedf(var e, var predicate): return Solve(e, x).Filter(predicate, x);
                case Piecewise p: return EquationSolver.SolvePiecewise(p, x, Solve);
            }
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
                    var (root1, root2) = AscendingEndpoints(roots.First(), roots.Last());
                    if (a is Real { IsNegative: true })
                        return new Interval(root1, false, root2, false);
                    return new Interval(Real.NegativeInfinity, false, root1, false)
                        .Unite(new Interval(root2, false, Real.PositiveInfinity, false));
                }
            }
            throw FutureReleaseException.Raised("Inequalities are not implemented yet", "1.2.1");
        }

        /// <summary>
        /// The two roots as (smaller, larger).
        /// </summary>
        /// <remarks>
        /// <see cref="TreeAnalyzer.SortRealsAndNonReals"/> orders the roots it can compare
        /// and leaves the rest in the order the quadratic formula produced them, which is
        /// <c>(-b - sqrt(D))/(2a)</c> before <c>(-b + sqrt(D))/(2a)</c> -- ascending only
        /// while <c>a</c> is positive. Solving <c>expr &lt;= 0</c> negates the expression and
        /// so arrives here with a negative leading coefficient, which is how
        /// <c>(x - a)(x + a) &lt;= 0</c> came to be answered with an interval running from
        /// <c>|a|</c> to <c>-|a|</c>: empty, with the whole solution set lost.
        /// <para/>
        /// Written with <c>abs</c> rather than as a case split on the sign of the symbol,
        /// because the ordering has a closed form -- <c>min(p, q)</c> is
        /// <c>(p + q - |p - q|)/2</c> and <c>max(p, q)</c> is <c>(p + q + |p - q|)/2</c>. So
        /// the answer is the single interval <c>[-|a|; |a|]</c>, which is right for either
        /// sign of <c>a</c> and for <c>a = 0</c>, rather than three branches each holding on
        /// part of the line. It also keeps the result a <see cref="Set"/>: a piecewise is an
        /// <see cref="Entity"/> and not one, so returning it here is not open in any case.
        /// <para/>
        /// Where both roots are real numbers the sort above has already ordered them, and
        /// they are handed back untouched so that nothing a concrete coefficient produces
        /// changes shape.
        /// <a href="https://github.com/asc-community/AngouriMath/issues/757">#757</a>
        /// </remarks>
        private static (Entity Lower, Entity Upper) AscendingEndpoints(Entity first, Entity second)
        {
            if (first is Real && second is Real)
                return (first, second);
            var spread = MathS.Abs(first - second);
            return (Tidied((first + second - spread) / 2), Tidied((first + second + spread) / 2));
        }

        /// <summary>
        /// An endpoint in the shortest form that is still an endpoint.
        /// </summary>
        /// <remarks>
        /// <see cref="Entity.Simplify"/> is what turns the arithmetic above back into something
        /// readable, but it may cancel a symbol against itself on the way -- which holds only
        /// away from zero, and so comes back as a <see cref="Providedf"/>. A condition where an
        /// endpoint belongs makes the interval an <see cref="Entity"/> rather than a
        /// <see cref="Set"/>, and threw on the cast for <c>a*x^2 - 1 &lt;= 0</c>. The
        /// unsimplified form is the same number and assumes nothing, so it is what is kept
        /// there. Those are the expressions whose *leading* coefficient is symbolic, which this
        /// does not claim to answer correctly in any case -- whether the solution lies inside
        /// the roots or outside them turns on the sign of that coefficient, and that is a
        /// second question from the one here.
        /// </remarks>
        private static Entity Tidied(Entity endpoint)
            => endpoint.Simplify() is var simplified && simplified is not Providedf
                ? simplified
                : endpoint.InnerSimplified;
    }
}
