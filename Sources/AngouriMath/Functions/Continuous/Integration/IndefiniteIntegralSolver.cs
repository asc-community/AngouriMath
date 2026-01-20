//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//
using HonkSharp.Fluency;
using static AngouriMath.Entity;
using System.Linq;
using System.Collections.Generic;

namespace AngouriMath.Functions.Algebra
{
    internal static class IndefiniteIntegralSolver
    {
        internal static Entity? SolveBySplittingSum(Entity expr, Entity.Variable x)
        {
            var splitted = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(expr, e => e.ContainsNode(x));
            if (splitted is null || splitted.Count < 2) return null; // nothing to do, let other solvers do the work
            return splitted.Select(e => Integration.ComputeIndefiniteIntegral(e, x)).Aggregate((e1, e2) => (e1, e2) switch {
                (null, _) or (_, null) => null,
                (var int1, var int2) => int1 + int2
            });
        }

        internal static Entity? SolveAsPolynomialTerm(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Mulf(var m1, var m2) =>
                !m1.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(m2, x)?.Pipe(i => m1 * i) :
                !m2.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(m1, x)?.Pipe(i => m2 * i) :
                null,

            Entity.Divf(var div, var over) =>
                !div.ContainsNode(x) ?
                    over is Entity.Powf(var @base, var power) ?
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(@base, -power), x)?.Pipe(i => div * i) :
                        Integration.ComputeIndefiniteIntegral(MathS.Pow(over, -1), x)?.Pipe(i => div * i) :
                !over.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(div, x)?.Pipe(i => i / over) :
                null,

            Entity.Powf(var @base, var power) =>
                !power.ContainsNode(x) && @base == x ?
                    power == -1 ?
                        MathS.Ln(MathS.Abs(@base)) :
                        MathS.Pow(x, power + 1) / (power + 1) :
                    null,

            Entity.Variable v =>
                v == x ? MathS.Pow(x, 2) / 2 : v * x,

            _ => null
        };

        internal static Entity? SolveIntegratingByParts(Entity expr, Entity.Variable x)
        {
            static Entity? IntegrateByParts(Entity v, Entity u, Entity.Variable x, int currentRecursion = 0)
            {
                if (v == 0) return 0;
                if (currentRecursion == MathS.Settings.MaxExpansionTermCount) return null;

                var integral = Integration.ComputeIndefiniteIntegral(u, x);
                if (integral is null) return null;
                var differential = v.Differentiate(x);
                var result = IntegrateByParts(differential, integral, x, currentRecursion + 1);
                return (result is null) ? null : v * integral - result;
            }

            if (expr is Entity.Mulf(var f, var g))
            {
                if (MathS.TryPolynomial(f, x, out var fPoly))
                {
                    return IntegrateByParts(fPoly, g, x);
                }
                if (MathS.TryPolynomial(g, x, out var gPoly))
                {
                    return IntegrateByParts(gPoly, f, x);
                }
                else return null;
            }
            else return null;
        }

        internal static Entity? SolveLogarithmic(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Logf(var @base, var arg) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Ln(arg) / MathS.Ln(@base), x) :
                arg is Entity.Powf(var y, var pow) ? // log(b, y^p) = ln(y^p) / ln(b) = ln(p) / ln(b) * ln(y)
                    Integration.ComputeIndefiniteIntegral(pow / MathS.Ln(@base) * MathS.Ln(y), x) :
                    null,

            _ => null
        };

        internal static Entity? SolveExponential(Entity expr, Entity.Variable x) => expr switch
        {
            Entity.Powf(var @base, var pow) =>
                @base.ContainsNode(x) ?
                    Integration.ComputeIndefiniteIntegral(MathS.Pow(MathS.e, MathS.Ln(@base) * pow), x) :
                    null,

            _ => null
        };

        /// <summary>
        /// Attempts to solve an integral using u-substitution.
        /// Looks for patterns where f(g(x)) * g'(x) can be integrated as F(g(x)).
        /// </summary>
        internal static Entity? SolveBySubstitution(Entity expr, Entity.Variable x)
        {
            // Try to find a suitable substitution u = g(x)
            // We need to identify a composite function and check if du/dx appears in the integrand
            var candidates = FindSubstitutionCandidates(expr, x);
            foreach (var u in candidates)
            {
                var duDx = u.Differentiate(x).InnerSimplified;

                // Try to express expr as h(u) * du/dx
                // If successful, integral becomes ∫h(u)du
                var uSub = Variable.CreateUnique(expr, "u_sub");

                // Try to divide expr by duDx and check if result is independent of x
                // Replace all occurrences of u's expression with a temporary variable
                var integrandInU = (expr / duDx).Substitute(u, uSub).Simplify(1);
                if (integrandInU is Providedf(var innerExpr, _)) integrandInU = innerExpr; // ignore singularities

                // If the result doesn't contain x anymore, we found a valid substitution
                // and we can integrate with respect to u (treating u as a variable)
                if (!integrandInU.ContainsNode(x) && Integration.ComputeIndefiniteIntegral(integrandInU, uSub) is { } resultInU)
                    // Substitute back: replace u with g(x)
                    return resultInU.Substitute(uSub, u);
            }

            return null;
        }

        /// <summary>
        /// Finds potential substitution candidates u = g(x) from the expression.
        /// For example, common patterns to try:
        /// 1. f(ax + b) * a  ->  u = ax + b
        /// 2. f(x^n) * x^(n-1)  ->  u = x^n
        /// 3. f(g(x)) * g'(x)  ->  u = g(x)
        /// </summary>
        private static IEnumerable<Entity> FindSubstitutionCandidates(Entity expr, Entity.Variable x)
        {
            var candidates = new List<Entity>();
            foreach (var node in expr.Nodes) // Look for composite functions (functions of functions)
                switch (node)
                {
                    case TrigonometricFunction:
                        candidates.Add(node); // Trigonometric function itself (for cases like sin(x)*cos(x))
                        if (node.DirectChildren[0] != x && node.DirectChildren[0].ContainsNode(x))
                            candidates.Add(node.DirectChildren[0]); // Trigonometric functions with non-trivial arguments
                        break;
                    case Powf(var @base, var exp):
                        if (@base == x) candidates.Add(node); // Power expressions x^n
                        // Exponential with non-trivial argument
                        if (@base != x && @base.ContainsNode(x)) candidates.Add(@base);
                        if (exp != x && exp.ContainsNode(x)) candidates.Add(exp);
                        break;
                    case Logf(_, var antilog):
                        candidates.Add(node); // Logarithm itself (for cases like 1/(x*ln(x)))
                        if (antilog != x && antilog.ContainsNode(x)) candidates.Add(antilog); // Also add the argument if it's not just x
                        break;
                    case Sumf(var aug, var add):
                        if (aug.ContainsNode(x) || add.ContainsNode(x)) candidates.Add(node); // Linear expressions ax + b
                        break;
                }
            // Sort by complexity - try simpler substitutions first
            return candidates.OrderBy(c => c.Complexity).Distinct();
        }
    }
}