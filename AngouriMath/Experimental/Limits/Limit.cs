﻿using AngouriMath.Core.Numerix;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Experimental.Limits
{
    public enum ApproachFrom
    {
        Left,
        Right,
    }
    public static class Limit
    {
        private static Entity? ComputeLimitToInfinityForwarder(Entity expr, VariableEntity x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }
        internal static Entity? ComputeLimitToInfinity(Entity expr, VariableEntity x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult != null) return substitutionResult;

            var logarithmResult = LimitSolvers.SolveAsLogarithm(expr, x);
            if (logarithmResult != null) return logarithmResult;

            var polynomialResult = LimitSolvers.SolveAsPolynome(expr, x);
            if (polynomialResult != null) return polynomialResult;

            var polynomialDivisionResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialDivisionResult != null) return polynomialDivisionResult;

            return null;
        }
        public static Entity? ComputeLimit(Entity expr, VariableEntity x, Entity dist, ApproachFrom side)
        {
            if(dist is NumberEntity number)
            {
                if (number.Value is RealNumber real)
                {
                    if (!real.IsFinite)
                    { 
                        // avoid NaN values as non finite numbers
                        if (real == RealNumber.NaN)
                        {
                            return RealNumber.NaN;
                        }
                        // if x -> -oo just make -x -> +oo
                        if (real == RealNumber.NegativeInfinity)
                        {
                            expr = expr.Substitute(x, -x);
                        }
                        // compute limit for x -> +oo
                        return ComputeLimitToInfinityForwarder(expr, x);
                    }
                }
                else if(!number.Value.IsFinite)
                {
                    throw new MathSException($"complex infinities are not supported in limits: lim({x} -> {dist}) {expr}");
                }
            }
            // handle side from which the limit is approached
            if (side == ApproachFrom.Left)
                expr = expr.Substitute(x, dist - 1 / x);
            else
                expr = expr.Substitute(x, dist + 1 / x);
            // lim(x -> 3-) x <=> lim(x -> 0+) 3 - x <=> lim(x -> +oo) 3 - 1 / x
            // lim(x -> 3+) x <=> lim(x -> 0+) 3 + x <=> lim(x -> +oo) 3 + 1 / x
            return ComputeLimitToInfinityForwarder(expr, x);
        }

        public static Entity? ComputeLimit(Entity expr, VariableEntity x, Entity dist)
        {
            var fromLeft = ComputeLimit(expr, x, dist, ApproachFrom.Left);
            var fromRight = ComputeLimit(expr, x, dist, ApproachFrom.Right);
            if (fromLeft is null || fromRight is null) return null;
            if (fromLeft == fromRight) return fromLeft;
            else return RealNumber.NaN;
        }
    }
}
