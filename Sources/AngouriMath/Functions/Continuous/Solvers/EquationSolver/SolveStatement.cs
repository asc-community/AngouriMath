//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Extensions;
using AngouriMath.Functions.Continuous.Solvers.SetSolver;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class StatementSolver
    {
        private static Entity Minus(Entity left, Entity right)
        {
            if (left.Evaled == 0)
                return -right;
            if (right.Evaled == 0)
                return left;
            return left - right;
        }

        /// <summary>
        /// Substitutes each root back into the equation it came from and drops the ones
        /// that do not satisfy it.
        /// </summary>
        /// <remarks>
        /// Several of the rewrites the solvers rely on widen the domain of the equation.
        /// <c>ln(a) + ln(b) = ln(a * b)</c> is the plainest: solving
        /// <c>ln(x) + ln(x+1) = 0</c> goes through <c>x^2 + x - 1 = 0</c> and hands back
        /// both of its roots, but at -1.618... the original is 2*pi*i, not 0. The
        /// individual rewrites cannot always carry a condition that survives the chain of
        /// substitutions that follows, so the answers are checked once, here, against the
        /// equation as the caller wrote it.
        /// </remarks>
        private static Set WithoutSpuriousRoots(Set roots, Entity equation, Variable x)
            => roots is FiniteSet finite && finite.Any(root => IsSpurious(equation, x, root))
                ? finite.Where(root => !IsSpurious(equation, x, root)).ToSet()
                : roots;

        /// <summary>
        /// Whether a root demonstrably fails the equation. Anything that cannot be
        /// evaluated to a number -- a root still carrying a parameter, say
        /// <c>pi + 2 * pi * n_1</c>, or one whose residual is not finite -- is kept, so
        /// that a root is only ever dropped on positive evidence against it.
        /// </summary>
        private static bool IsSpurious(Entity equation, Variable x, Entity root)
        {
            if (root.Vars.Any())
                return false;
            try
            {
                return equation.Substitute(x, root).Evaled is Number.Complex residual
                    && residual.IsFinite
                    && !residual.Abs().EDecimal.LessThan(MathS.Settings.PrecisionErrorCommon);
            }
            catch (Core.Exceptions.AngouriBugException) { throw; }
            catch (System.Exception) { return false; }
        }

        internal static Set Solve(Entity expr, Variable x)
            => expr switch
            {
                Equalsf(var left, var right) when left is Set || right is Set
                    => AnalyticalSetSolver.Solve(left, right, x),

                Equalsf(var left, var right) when left is not Set && right is not Set
                    => WithoutSpuriousRoots(AnalyticalEquationSolver.Solve(left - right, x), left - right, x),

                Equalsf => Empty,

                Andf(var left, var right) => 
                    MathS.Intersection(Solve(left, x), Solve(right, x)),
                Orf(var left, var right) => 
                    MathS.Union(Solve(left, x), Solve(right, x)),
                Impliesf(var left, var right) => 
                    MathS.Union(MathS.SetSubtraction(expr.Codomain, Solve(left, x)), Solve(right, x)),

                // TODO: there should be universal set to subtract from when inverting
                Greaterf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(left, right), x),
                LessOrEqualf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x)
                    .Unite(AnalyticalEquationSolver.Solve(Minus(left, right), x)),
                GreaterOrEqualf(var left, var right) => MathS.Union(AnalyticalInequalitySolver.Solve(Minus(left, right), x), AnalyticalEquationSolver.Solve(Minus(left, right), x)),

                Lessf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x),

                Variable when expr == x => new FiniteSet(true),

                Inf(var var, Set set) when var == x => set,
                
                Providedf(var e, var predicate) => Solve(e, x).Filter(predicate, x),
                Piecewise p => EquationSolver.SolvePiecewise(p, x, Solve),

                // TODO: Although piecewise needed?
                _ => Set.Empty
            };
    }
}
