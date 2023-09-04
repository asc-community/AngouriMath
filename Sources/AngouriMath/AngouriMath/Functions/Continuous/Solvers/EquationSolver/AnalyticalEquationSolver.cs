//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using static AngouriMath.Entity;
using AngouriMath.Functions.Algebra;
using AngouriMath.Extensions;
using static AngouriMath.Entity.Set;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Multithreading;

namespace AngouriMath
{
    partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Attempt to find analytical roots of a custom equation.
        /// It solves the given expression assuming that it is
        /// equal to zero. No need to make it equal to 0 yourself;
        /// however, if you prefer so, consider using the .Solve()
        /// method instead
        /// </summary>
        /// <param name="x">
        /// The variable over which to solve the equation
        /// </param>
        /// <example>
        /// <code>
        /// Entity expr = "x + 8 - 4";
        /// Console.WriteLine(expr.SolveEquation("x"));
        /// </code>
        /// Will print "{ -4 }"
        /// </example>
        /// <returns>
        /// Returns <see cref="Set"/>
        /// </returns>
        public Set SolveEquation(Variable x) => EquationSolver.Solve(this, x);
    }
}

namespace AngouriMath.Functions
{
    internal static partial class TreeAnalyzer
    {
        /// <summary>
        /// Searches for a subtree containing `ent` and being minimal possible size.
        /// For example, for expr = MathS.Sqr(x) + 2 * (MathS.Sqr(x) + 3) the result
        /// will be MathS.Sqr(x) while for MathS.Sqr(x) + x the minimum subtree is x.
        /// Further, it will be used for solving with variable replacing, for example,
        /// there's no pattern for solving equation like sin(x)^2 + sin(x) + 1 = 0,
        /// but we can first solve t^2 + t + 1 = 0, and then root = sin(x).
        /// </summary>
        public static Entity GetMinimumSubtree(Entity expr, Variable x)
        {
            if (!expr.ContainsNode(x))
                throw new AngouriBugException($"{nameof(expr)} must contain {nameof(x)}");

            // The idea is the following:
            // We must get a subtree that has more occurances than 1,
            // But at the same time it should cover all references to `ent`
            var xs = expr.Nodes.Count(child => child == x);
            return
                expr.Nodes
                .TakeWhile(e => e != x) // Requires Entity enumeration to be depth-first!!
                .Where(e => e.ContainsNode(x)) // e.g. when expr is sin((x+1)^2)+3, this step results in [sin((x+1)^2)+3, sin((x+1)^2), (x+1)^2, x+1]
                .LastOrDefault(sub => expr.Nodes.Count(child => child == sub) * sub.Nodes.Count(child => child == x) == xs)
                // if `expr` contains 2 `sub`s and `sub` contains 3 `x`s, then there should be 6 `x`s in `expr` (6 == `xs`)
                ?? x;
        }
    }
}

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class AnalyticalEquationSolver
    {
        private static Set PiecewiseIntoIndependentEquations(Piecewise leftPart, Variable x, Entity rightPart)
        {
            Entity cond = true;
            var res = new List<Set>();
            foreach (var c in leftPart.Cases)
            {
                res.Add(Solve(c.Expression - rightPart, x).Filter(c.Predicate & cond, x));
                cond &= !c.Predicate;
            }
            return res.Unite();
        }

        /// <summary>Equation solver</summary>
        /// <param name="compensateSolving">
        /// Compensate solving is needed when you formatted an equation to (something - const)
        /// and compensateSolving "compensates" this by applying expression inverter,
        /// aka compensating the equation formed by the previous solver
        /// </param>
        /// <param name="expr">Expression</param>
        /// <param name="x">Variable to solve over</param>
        internal static Set Solve(Entity expr, Variable x, bool compensateSolving = false)
        {
            if (expr == x)
                return new Entity[] { 0 }.ToSet();

            // Applies an attempt to downcast roots
            static Entity TryDowncast(Entity equation, Variable x, Entity root)
            {
                if (root.Evaled is not Complex preciseValue)
                    return root;
                using var _ = MathS.Settings.FloatToRationalIterCount.Set(20);
                using var __ = MathS.Settings.PrecisionErrorZeroRange.Set(1e-7m);
                var downcasted = Complex.Create(preciseValue.RealPart, preciseValue.ImaginaryPart);
                if (equation.Substitute(x, downcasted).Evaled is not Complex error)
                    return root;
                return IsZero(error) && downcasted.RealPart is Rational && downcasted.ImaginaryPart is Rational
                       ? downcasted : root.InnerSimplified;
            }

            MultithreadingFunctional.ExitIfCancelled();

            switch (expr)
            {
                case Mulf(var multiplier, var multiplicand):
                    return MathS.Union(Solve(multiplier, x), Solve(multiplicand, x));
                case Divf(var dividend, var divisor):
                    return MathS.SetSubtraction(Solve(dividend, x), Solve(divisor, x));
                case Powf(var @base, _):
                    return Solve(@base, x);
                case Minusf(var subtrahend, var minuend) when !minuend.ContainsNode(x) && compensateSolving:
                    if (subtrahend == x)
                        return new[] { minuend }.ToSet();
                    if (subtrahend is Piecewise piecewise)
                        return PiecewiseIntoIndependentEquations(piecewise, x, minuend);
                    Entity? lastChild = null;
                    foreach (var child in subtrahend.DirectChildren)
                        if (child.ContainsNode(x))
                            if (lastChild is null)
                                lastChild = child;
                            else goto default;
                    if (lastChild is null)
                        goto default;
                    // TODO: optimize?
                    return subtrahend.Invert(minuend, lastChild).Select(result => Solve(lastChild - result, x, compensateSolving: true)).Unite();
                case Function:
                    return expr.Invert(0, x).Select(ent => TryDowncast(expr, x, ent)).ToSet();
                default:
                    break;
            }

            if (PolynomialSolver.SolveAsPolynomial(expr, x) is { } poly)
                return poly.Select(e => TryDowncast(expr, x, e.InnerSimplified)).ToSet();

            // If the replacement isn't one-variable one,
            // then solving over replacements is already useless,
            // so we skip this part and go to other solvers
            if (!compensateSolving)
            {
                var newVar = Variable.CreateTemp(expr.Vars);
                // Here we find all possible replacements and find one that has at least one solution
                foreach (var alt in expr.Alternate(4))
                {
                    MultithreadingFunctional.ExitIfCancelled();
                    if (!alt.ContainsNode(x))
                        return Set.Empty; // in this case there is either 0 or +oo solutions
                    var minimumSubtree = TreeAnalyzer.GetMinimumSubtree(alt, x);
                    if (minimumSubtree == x)
                        continue;
                    // Here we are trying to solve for this replacement
                    var solutionsSet = Solve(alt.Substitute(minimumSubtree, newVar), newVar).InnerSimplified;
                    if (solutionsSet is FiniteSet { IsSetEmpty: false } enums)
                    {
                        var solutions = enums.Select(solution => Solve(minimumSubtree - solution, x, compensateSolving: true)).Unite().InnerSimplified;
                        if (solutions is FiniteSet els)
                            return els.Select(ent => TryDowncast(expr, x, ent)).ToSet();
                        else if (solutions is Set { IsSetEmpty: false } set)
                            return set;
                    }
                }
                // // //
            }

            // if no replacement worked, try trigonometric solver
            if (TrigonometricSolver.TrySolveLinear(expr, x, out var trig) && trig is FiniteSet elsTrig)
                return (Set)elsTrig.Select(ent => TryDowncast(expr, x, ent)).ToSet().InnerSimplified;
            // // //

            MultithreadingFunctional.ExitIfCancelled();

            // if no trigonometric rules helped, try exponential-multiplicative solver
            if (ExponentialSolver.SolveMultiplicative(expr, x) is { } expMul && expMul is FiniteSet elsExpMul)
                return (Set)elsExpMul.Select(ent => TryDowncast(expr, x, ent)).ToSet().InnerSimplified;
            // // //

            // if no exponential-multiplicative rules helped, try exponential-linear solver
            if (ExponentialSolver.SolveLinear(expr, x) is { } expLin && expLin is FiniteSet elsExpLin)
                return (Set)elsExpLin.Select(ent => TryDowncast(expr, x, ent)).ToSet().InnerSimplified;
            // // //

            // if no exponential-linear rules helped, common denominator might help
            if (CommonDenominatorSolver.TrySolveGCD(expr, x, out var commonDenom) && commonDenom is FiniteSet elsCd)
                return (Set)elsCd.Select(ent => TryDowncast(expr, x, ent)).ToSet().InnerSimplified;
            // // //

            // if we have fractioned polynomials
            if (FractionedPolynoms.TrySolve(expr, x, out var fractioned) && fractioned is FiniteSet elsFracs)
                return (Set)elsFracs.Select(ent => TryDowncast(expr, x, ent)).ToSet().InnerSimplified;
            // // //

            // TODO: Solve factorials (Needs Lambert W function)
            // https://mathoverflow.net/a/28977

            // if nothing has been found so far
            if (MathS.Settings.AllowNewton && expr.Vars.Count == 1)
                return expr.SolveNt(x).Select(ent => TryDowncast(expr, x, ent)).ToSet();

            return Enumerable.Empty<Entity>().ToSet();
        }
    }
}
