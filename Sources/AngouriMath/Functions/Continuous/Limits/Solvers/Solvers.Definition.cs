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

            // In front of both paths, not only the two-sided one. Each of these is guarded on
            // a two-sided limit, and a two-sided limit that exists is also the limit from
            // either side, so none of them says anything one-sided that it did not already say
            // two-sided. Skipping them is what made (1 + x)^(1/x) at 0+ answer 1: without the
            // second remarkable limit the descent reads it as 1^(+oo) and is definite about
            // it, where the same expression approached from both sides gives e.
            expr = expr.Replace(a => TrivialTrigonometricReplacement(a, x));
            expr = ApplyTrivialTransformations(expr, x, dest, (_, exprLim) => exprLim);
            expr = ApplyFirstRemarkable(expr, x, dest);
            expr = expr.Replace(c => ApplySecondRemarkable(c, x, dest));
            MultithreadingFunctional.ExitIfCancelled();

            if (side is ApproachFrom.Left or ApproachFrom.Right)
            {
                var oneSided = expr.ComputeLimitDivideEtImpera(x, dest, side);
                if (oneSided is { } found && (acceptNaN || found.Evaled != MathS.NaN))
                    return oneSided;
                // The same rule the two-sided path falls through to, asked with the side it was
                // given. l'Hopital's rule is stated one-sidedly to begin with -- the two-sided
                // case is the two one-sided ones agreeing -- so this is not a weaker reading of
                // it than the one above. Where the descent has nothing to say about a quotient,
                // it is what answers (1 - cos(x)) / x^2 at 0+ with 1/2 rather than NaN, and
                // csc(x) * x with 1: the csc rewrite leaves a product, which the descent does
                // not take apart but which the rule reads back as the quotient x / sin(x).
                if (ApplylHopitalRule(expr, x, dest, side) is { } lhopital && lhopital.Evaled != MathS.NaN)
                    return lhopital;
                // The one-sided path is the only one with nothing behind it, and the descent
                // can make an indeterminate form definite on the way down: for x * ln(x) at 0+
                // it substitutes the first factor's own limit and then asks for 0 * -oo, which
                // is NaN. NaN is the claim that the limit does not exist, and here it is 0.
                //
                // Moving x out to infinity is what ComputeLimitImpl does for a finite
                // destination in any case, so this asks the same question in the place where
                // l'Hopital's rule and the rest of the machinery for infinity live -- none of
                // which this path can otherwise reach. Only where nothing above answered, and
                // whatever was found before is kept if this finds nothing better.
                // Simplified, and not only for tidiness: the substitution leaves the
                // destination behind as a term, and it is exactly that leftover which makes
                // the answer NaN. x * ln(x) at 0+ becomes (0 + 1/x) * ln(0 + 1/x), whose limit
                // at infinity comes back NaN, where the same expression written as
                // (1/x) * ln(1/x) comes back 0.
                if (dest.IsFinite
                    && ComputeLimit(expr.Substitute(x, side is ApproachFrom.Left ? dest - 1 / x : dest + 1 / x)
                                        .InnerSimplified,
                                    x, Real.PositiveInfinity) is { } byInfinity
                    && byInfinity.Evaled != MathS.NaN)
                    return byInfinity;
                return oneSided;
            }
            if (side is ApproachFrom.BothSides)
            {
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
                    // The rewrites are worth their cost only where there is no answer without
                    // them, and each of them costs an expansion or a simplification of the
                    // whole expression. The descent visits every part of every expression, so
                    // rather than pay for them at each of those parts on every limit ever
                    // taken, the descent is walked a second time with them turned on, and only
                    // for an expression that has just been found to have no answer at all.
                    if (SolveByRewriting(expr, x, dest) is { } rewritten && rewritten.Evaled != MathS.NaN)
                        return rewritten;
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