/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
namespace AngouriMath.Functions.Algebra
{
    using AngouriMath.Core.Exceptions;
    using AngouriMath.Core.Multithreading;
    using Core;
    using System;
    using static Entity;
    using static Entity.Number;
    internal static partial class LimitFunctional
    {
        private static Entity? SimplifyAndComputeLimitToInfinity(Entity expr, Variable x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }

        private static Entity? ComputeLimitToInfinity(Entity expr, Variable x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult is { }) return substitutionResult;

            var logarithmResult = LimitSolvers.SolveAsLogarithm(expr, x);
            if (logarithmResult is { }) return logarithmResult;

            var polynomialResult = LimitSolvers.SolveAsPolynomial(expr, x);
            if (polynomialResult is { }) return polynomialResult;

            var polynomialDivisionResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialDivisionResult is { }) return polynomialDivisionResult;

            var logarithmDivisionResult = LimitSolvers.SolveAsLogarithmDivision(expr, x);
            if (logarithmDivisionResult is { }) return logarithmDivisionResult;

            return null;
        }

        private static Entity ExpandLogarithm(Entity expr)
            => expr switch
            {
                Logf(var @base, var antilog) when @base != MathS.e => MathS.Ln(antilog) / MathS.Ln(@base),
                _ => expr
            };

        public static Entity? ComputeLimit(Entity expr, Variable x, Entity dest, ApproachFrom side = ApproachFrom.BothSides)
        {
            expr = expr.Replace(ExpandLogarithm).Simplify();

            if (side is ApproachFrom.Left or ApproachFrom.Right)
                return expr.ComputeLimitDivideEtImpera(x, dest, side);
            if (side is ApproachFrom.BothSides)
            {
                expr = expr.Replace(a => TrivialTrigonometricReplacement(a, x));
                expr = ApplyTrivialTransformations(expr, x, dest, (_, exprLim) => exprLim);
                expr = ApplyFirstRemarkable(expr, x, dest);
                expr = ApplySecondRemarkable(expr, x, dest);
                expr = ApplylHopitalRule(expr, x, dest);


                MultithreadingFunctional.ExitIfCancelled();
                if (!dest.IsFinite)
                    // just compute limit with no check for left/right equality
                    // here approach left will be ignored anyways, as dist is infinite number
                    return expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Left);
                else if (expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Left) is { } fromLeft
                  && expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Right) is { } fromRight)
                {
                    if (fromLeft == fromRight)
                        return fromLeft;
                    if (ExpressionNumerical.AreEqual(fromLeft, fromRight))
                        return fromLeft;
                    return Real.NaN;
                }
                else
                    return null;
            }
            throw new AngouriBugException($"Unresolved enum parameter {side}");
        }

        internal static Entity? ComputeLimitImpl(Entity expr, Variable x, Entity dist, ApproachFrom side) => dist switch
        {
            _ when !expr.ContainsNode(x) => expr,
            // avoid NaN values as non finite numbers
            Real { IsNaN: true } => Real.NaN,
            // if x -> -oo just make -x -> +oo
            Real { IsFinite: false, IsNegative: true } => SimplifyAndComputeLimitToInfinity(expr.Substitute(x, -x), x),
            // compute limit for x -> +oo
            Real { IsFinite: false, IsNegative: false } => SimplifyAndComputeLimitToInfinity(expr, x),
            Complex { IsFinite: false } =>
                throw new LimitOperationNotSupportedException($"Complex infinities are not supported in limits: lim({x} -> {dist}) {expr}"),
            _ => SimplifyAndComputeLimitToInfinity(side switch
            {
                // lim(x -> 3-) x <=> lim(x -> 0+) 3 - x <=> lim(x -> +oo) 3 - 1 / x
                ApproachFrom.Left => expr.Substitute(x, dist - 1 / x),
                // lim(x -> 3+) x <=> lim(x -> 0+) 3 + x <=> lim(x -> +oo) 3 + 1 / x
                ApproachFrom.Right => expr.Substitute(x, dist + 1 / x),
                _ => throw new System.ArgumentOutOfRangeException(nameof(side), side,
                    $"Only {ApproachFrom.Left} and {ApproachFrom.Right} are supported.")
            }, x)
        };
    }
}