//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath.Functions.Algebra
{
    using AngouriMath.Core.Exceptions;
    using AngouriMath.Core.Multithreading;
    using Core;
    using static Entity;
    using static Entity.Number;
    internal static partial class LimitFunctional
    {
        private static Entity? SimplifyAndComputeLimitToInfinity(Entity expr, Variable x)
        {
            expr = expr.Simplify();
            if (expr is Providedf(var expression, _)) expr = expression; // limits operate assuming a continuous expression even though some points may be undefined.

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

        public static Entity? ComputeLimit(Entity expr, Variable x, Entity dest, ApproachFrom side = ApproachFrom.BothSides, bool acceptNaN = false)
        {
            if (expr is not ContinuousNode and not Variable)
                return null;
            expr = expr.Replace(ExpandLogarithm);

            if (side is ApproachFrom.Left or ApproachFrom.Right)
                return expr.ComputeLimitDivideEtImpera(x, dest, side);
            if (side is ApproachFrom.BothSides)
            {
                expr = expr.Replace(a => TrivialTrigonometricReplacement(a, x));
                expr = ApplyTrivialTransformations(expr, x, dest, (_, exprLim) => exprLim);
                expr = ApplyFirstRemarkable(expr, x, dest);
                // expr = ApplySecondRemarkable(expr, x, dest);
                expr = expr.Replace(c => ApplySecondRemarkable(c, x, dest));

                MultithreadingFunctional.ExitIfCancelled();
                if (!dest.IsFinite)
                {
                    // just compute limit with no check for left/right equality
                    // here approach left will be ignored anyways, as dest is infinite number
                    var atInfinity = expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Left);
                    if (atInfinity is { } found && found.Evaled != MathS.NaN)
                        return found;
                    // l'Hopital's rule was only ever reached for a finite destination, so the
                    // textbook oo/oo cases at infinity -- ln(x) / x, x / e^x, ln(x) / sqrt(x) --
                    // were left with nothing to catch them. The rule is only allowed to improve
                    // on what is already there: sqrt(x) / sqrt(x + 1) merely turns into its own
                    // reciprocal, and a NaN from it would claim the limit does not exist.
                    if (ApplylHopitalRule(expr, x, dest) is { } lhopital && lhopital.Evaled != MathS.NaN)
                        return lhopital;
                    // Last, because it is the most expensive and everything above is cheaper
                    // for the cases it answers. What it adds are the ones nothing above has
                    // any reading of, including the differences whose terms cancel to every
                    // order: e^(x + e^(-x)) - e^x is 1, and no amount of differentiating both
                    // parts of it arrives there.
                    //
                    // Only where the destination is itself infinite, which is what the
                    // algorithm is stated for. A limit at a finite point reaches infinity too,
                    // by substituting for x, but there it would be replacing answers the rules
                    // above already give rather than adding ones they do not -- a change worth
                    // making on its own evidence and not as a side effect of this.
                    if (Gruntz.LimitToPositiveInfinity(
                            dest.Evaled is Real { IsNegative: true } ? expr.Substitute(x, -x) : expr, x)
                        is { } byGruntz && byGruntz.Evaled != MathS.NaN)
                        return byGruntz;
                    return atInfinity;
                }
                else if (expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Left) is { } fromLeft
                  && expr.ComputeLimitDivideEtImpera(x, dest, ApproachFrom.Right) is { } fromRight)
                {
                    if (fromLeft == fromRight && (acceptNaN || fromLeft.Evaled != MathS.NaN))
                        return fromLeft;
                    if (ExpressionNumerical.AreEqual(fromLeft, fromRight) && (acceptNaN || fromLeft.Evaled != MathS.NaN))
                        return fromLeft;
                    var lhopital = ApplylHopitalRule(expr, x, dest);
                    if (lhopital != null) return ComputeLimit(lhopital, x, dest, acceptNaN: true);
                    else return MathS.NaN; // A two-sided limit cannot exist if the limit from left and right don't match.
                }
                else
                {
                    var lhopital = ApplylHopitalRule(expr, x, dest);
                    if (lhopital != null) return ComputeLimit(lhopital, x, dest);
                    else return null;
                }
            }
            throw new AngouriBugException($"Unresolved enum parameter {side}");
        }

        internal static Entity? ComputeLimitImpl(Entity expr, Variable x, Entity dist, ApproachFrom side) => dist switch
        {
            _ when !expr.ContainsNode(x) => expr,
            // avoid NaN values as non finite numbers
            { IsNaN: true } => MathS.NaN,
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