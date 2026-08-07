//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Multithreading;
using AngouriMath.Extensions;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    internal static class TrigonometricSolver
    {
        /// <summary>
        /// Rewrites an equation that mixes <c>sin(u)</c> and <c>cos(u)</c> into one written
        /// in a single function, by way of <c>cos(u)^2 = 1 - sin(u)^2</c> or its mirror.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Without this the equation reaches the exponential solver, which writes both
        /// functions in terms of <c>e^(i u)</c> -- so <c>cos(a x)^2 + sin(a x) + c = 0</c>,
        /// which is a quadratic in <c>sin(a x)</c>, becomes a *quartic* in <c>e^(i a x)</c>
        /// and is answered with pages of nested radicals where there are two arcsines.
        /// https://github.com/asc-community/AngouriMath/issues/270
        /// </para>
        /// <para>
        /// Nothing new is needed to solve what comes out: the replacement machinery in
        /// <see cref="AnalyticalEquationSolver"/> already reduces a polynomial in
        /// <c>sin(u)</c> to a polynomial in a fresh variable and inverts the arcsine
        /// afterwards, which is why <c>sin(x)^2 + sin(x) = 0</c> has always answered well.
        /// The mixed form simply never became one.
        /// </para>
        /// <para>
        /// Only whole even powers are rewritten, and the rewrite is kept only if it removes
        /// the other function entirely. An odd power leaves a square root of a square behind
        /// -- <c>sin(x) + cos(x)</c> is not a polynomial in either function -- so those are
        /// declined here and left to the solvers that already handle them.
        /// </para>
        /// </remarks>
        internal static bool TryRewriteInOneFunction(Entity expr, Variable x, out Entity rewritten)
        {
            rewritten = expr;
            static bool Mentions<T>(Entity where, Variable x) where T : Entity
                => where.Nodes.Any(node => node is T && node.DirectChildren[0].ContainsNode(x));

            if (!Mentions<Sinf>(expr, x) || !Mentions<Cosf>(expr, x))
                return false;

            var asSine = expr.Replace(node => node switch
            {
                Powf(Cosf(var arg), Integer { IsPositive: true } power)
                    when power.EInteger.IsEven && arg.ContainsNode(x)
                    => MathS.Pow(1 - MathS.Pow(MathS.Sin(arg), 2),
                                 Integer.Create(power.EInteger.Divide(2))),
                _ => node
            });
            if (!Mentions<Cosf>(asSine, x))
            {
                // Folded, because the halved power is often 1 and `(1 - sin(x)^2)^1` is not
                // recognised as a polynomial in sin(x) by the replacement machinery that has
                // to solve it -- the rewrite fired and then bought nothing.
                rewritten = asSine.InnerSimplified;
                return true;
            }

            var asCosine = expr.Replace(node => node switch
            {
                Powf(Sinf(var arg), Integer { IsPositive: true } power)
                    when power.EInteger.IsEven && arg.ContainsNode(x)
                    => MathS.Pow(1 - MathS.Pow(MathS.Cos(arg), 2),
                                 Integer.Create(power.EInteger.Divide(2))),
                _ => node
            });
            if (!Mentions<Sinf>(asCosine, x))
            {
                rewritten = asCosine.InnerSimplified;
                return true;
            }
            return false;
        }

        // solves equation f(sin(x), cos(x), tan(x), cot(x)) for x
        internal static bool TrySolveLinear(Entity expr, Variable variable, out Set res)
        {
            res = Empty;
            var replacement = Variable.CreateTemp(expr.Vars);
            expr = expr.Replace(Patterns.NormalTrigonometricForm);
            expr = expr.Replace(Patterns.TrigonometricToExponentialRules(variable, replacement));
            MultithreadingFunctional.ExitIfCancelled();
            // if there is still original variable after replacements,
            // equation is not in a form f(sin(x), cos(x), tan(x), cot(x))
            if (expr.ContainsNode(variable))
                return false;

            if (AnalyticalEquationSolver.Solve(expr, replacement) is FiniteSet els)
            {
                MultithreadingFunctional.ExitIfCancelled();
                res = (Set)els.Select(sol => MathS.Pow(MathS.e, MathS.i * variable).Invert(sol, variable).ToSet()).Unite().InnerSimplified;
                return true;
            }
            else
                return false;
        }
    }
}