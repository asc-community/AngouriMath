/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
namespace AngouriMath.Core
{
    /// <summary>
    /// Where to tend to the given number in limits
    /// </summary>
    public enum ApproachFrom
    {
        /// <summary>
        /// Means that the limit is considered valid if and only if
        /// Left-sided limit exists and Right-sided limit exists
        /// and they are equal
        /// </summary>
        BothSides,

        /// <summary>
        /// If x tends from the left, i. e. it is never greater than the destination
        /// </summary>
        Left,

        /// <summary>
        /// If x tends from the right, i. e. it is never less than the destination
        /// </summary>
        Right,
    }
}

namespace AngouriMath
{
    using Core;
    using static Functions.Algebra.LimitFunctional;
    partial record Entity
    {
        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo", ApproachFrom.BothSides)
        /// </param>
        /// <param name="side">
        /// From where to approach it: from the left, from the right,
        /// or BothSides, implying that if limits from either are not
        /// equal, there is no limit
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        public Entity Limit(Variable x, Entity destination, ApproachFrom side)
        { 
            var res = ComputeLimit(this, x, destination, side); 
            if (res is null || res == MathS.NaN)
                return new Limitf(this, x, destination, side);
            return res;
        }

        /// <summary>
        /// Finds the limit of the given expression over the given variable
        /// </summary>
        /// <param name="x">
        /// The variable to be approaching
        /// </param>
        /// <param name="destination">
        /// A value where the variable approaches. It might be a symbolic
        /// expression, a finite number, or an infinite number, for example,
        /// "sqrt(x2 + x) / (3x + 3)".Limit("x", "+oo")
        /// </param>
        /// <returns>
        /// A result or the <see cref="Limitf"/> node if the limit
        /// cannot be determined
        /// </returns>
        public Entity Limit(Variable x, Entity destination)
        {
            var res = ComputeLimit(this, x, destination, ApproachFrom.BothSides);
            if (res is null || res == MathS.NaN)
                return new Limitf(this, x, destination, ApproachFrom.BothSides);
            return res.InnerSimplified;
        }

        /// <summary>
        /// <a href="https://en.wikipedia.org/wiki/Divide_and_rule"/>
        /// Divide and rule (Latin: divide et impera), or divide and conquer,
        /// in politics and sociology is gaining and maintaining power by
        /// breaking up larger concentrations of power into pieces that
        /// individually have less power than the one implementing the strategy.
        /// 
        /// In computer science, divide and conquer is an algorithm design paradigm
        /// based on multi-branched recursion. A divide-and-conquer algorithm works
        /// by recursively breaking down a problem into two or more sub-problems of
        /// the same or related type, until these become simple enough to be solved
        /// directly. The solutions to the sub-problems are then combined to give a
        /// solution to the original problem.
        /// </summary>
        // here we try to compute limit for each children and then merge them into limit of whole expression
        // theoretically, for cases such limit (x -> -1) 1 / (x + 1)
        // this method will return NaN, but thanks to replacement of x to an non-definite expression,
        // it is somehow compensated
        internal virtual Entity? ComputeLimitDivideEtImpera(Variable x, Entity dist, ApproachFrom side)
            => new Limitf(this, x, dist, side);
    }
}


namespace AngouriMath.Functions.Algebra
{
    using AngouriMath.Core.Exceptions;
    using AngouriMath.Core.Multithreading;
    using Core;
    using System;
    using static Entity;
    using static Entity.Number;
    internal static class LimitFunctional
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

        private static Entity EquivalenceRules(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                Sinf or Tanf or Arcsinf or Arctanf => expr.DirectChildren[0],
                _ => expr
            };

        private static Entity ApplyFirstRemarkable(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                Divf(var a, var b) div
                    when a.Limit(x, dest).Evaled == 0 && b.Limit(x, dest).Evaled == 0
                        => div.New(EquivalenceRules(a, x, dest), EquivalenceRules(b, x, dest)),

                _ => expr
            };

        private static Entity ApplySecondRemarkable(Entity expr, Variable x, Entity dest)
            => expr switch
            {
                // f(x)^g(x) for f(x) -> 1, g(x) -> +oo
                // => (1 + (f(x) - 1)) ^ g(x) = ((1 - (f(x) - 1)) ^ (1 / (f(x) - 1))) ^ (g(x) (f(x) - 1))
                // e ^ (g(x) * (f(x) - 1))
                Powf(var xPlusOne, var xPower) when
                xPlusOne.ContainsNode(x) && xPower.ContainsNode(x) &&
                (xPlusOne - 1).Limit(x, dest).Evaled == 0 && IsInfiniteNode(xPower.Limit(x, dest)) =>
                MathS.e.Pow(xPower * (xPlusOne - 1)),

                _ => expr
            };

        private static bool IsInfiniteNode(Entity expr)
            => expr.ContainsNode("+oo") || expr.ContainsNode("-oo"); // TODO: is it correct?

        private static bool IsFiniteNode(Entity expr)
            => !IsInfiniteNode(expr) && expr != MathS.NaN;

        private static Entity ApplylHopitalRule(Entity expr, Variable x, Entity dest)
        {
            if (expr is Divf(var num, var den))
                if (num.Limit(x, dest).Evaled is var numLimit && den.Limit(x, dest).Evaled is var denLimit)
                    if (numLimit == 0 && denLimit == 0 ||
                            IsInfiniteNode(numLimit) && IsInfiniteNode(denLimit))
                        if (num is not Number && den is not Number)
                            if (num.ContainsNode(x) && den.ContainsNode(x))
                            {
                                var applied = num.Differentiate(x) / den.Differentiate(x);
                                if (ComputeLimit(applied, x, dest) is { } resLim)
                                    return resLim;
                            }
            return expr;
        }

        private static Entity ApplyTrivialTransformations(Entity expr, Variable x, Entity dest, Func<Entity, Entity, Entity> transformation)
            => expr switch
            {
                Sumf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) + transformation(b, bLim),
                Minusf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) - transformation(b, bLim),
                Mulf(var a, var b)
                    when ComputeLimit(a, x, dest) is { } aLim && ComputeLimit(b, x, dest) is { } bLim &&
                        IsFiniteNode(aLim.Evaled) && IsFiniteNode(bLim.Evaled)
                        => transformation(a, aLim) * transformation(b, bLim),
                _ => expr
            };

        private static Entity TrivialTrigonometricReplacement(Entity expr, Variable x)
            => expr switch
            {
                Secantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Cos(arg),
                Cosecantf(var arg) when arg.ContainsNode(x) => 1 / MathS.Sin(arg),
                _ => expr
            };

        public static Entity? ComputeLimit(Entity expr, Variable x, Entity dest, ApproachFrom side = ApproachFrom.BothSides)
        {
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