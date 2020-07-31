using AngouriMath.Core.Numerix;
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
        private static (Entity? result, bool isFinite) ComputeLimitToInfinityForwarder(Entity expr, VariableEntity x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }
        internal static (Entity? result, bool isFinite) ComputeLimitToInfinity(Entity expr, VariableEntity x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult.result is {}) return substitutionResult;

            var logarithmResult = LimitSolvers.SolveAsLogarithm(expr, x);
            if (logarithmResult.result is {}) return logarithmResult;

            var polynomialResult = LimitSolvers.SolveAsPolynome(expr, x);
            if (polynomialResult.result is {}) return polynomialResult;

            var polynomialDivisionResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialDivisionResult.result is {}) return polynomialDivisionResult;
            
            var logarithmDivisionResult = LimitSolvers.SolveAsLogarithmDivision(expr, x);
            if (logarithmDivisionResult.result is {}) return logarithmDivisionResult;

            return (null, false);
        }

        public static Entity? ComputeLimit(Entity expr, VariableEntity x, Entity dist, ApproachFrom side)
        {
            expr = expr.DeepCopy();
            return _ComputeLimitSwitcher(expr, x, dist, side).result;
        }

        internal static (Entity? result, bool isFinite) _ComputeLimitSwitcher(Entity expr, VariableEntity x,
            Entity dist, ApproachFrom side)
        {
            var lim = _ComputeLimit(expr, x, dist, side);
            if (!(lim.result is null))
                return lim;

            for (int i = 0; i < expr.ChildrenCount; i++)
            {
                var child = expr.GetChild(i);
                var (limit, isFinite) = _ComputeLimitSwitcher(child, x, dist, side);
                if (limit is {} && isFinite)
                    expr.SetChild(i, limit);
            }

            return _ComputeLimit(expr, x, dist, side);
        }

        internal static (Entity? result, bool isFinite) _ComputeLimit(Entity expr, VariableEntity x, Entity dist, ApproachFrom side)
        {
            if (!expr.SubtreeIsFound(x))
                return (expr, true);

            if(dist is NumberEntity number)
            {
                if (number.Value is RealNumber real)
                {
                    if (!real.IsFinite)
                    { 
                        // avoid NaN values as non finite numbers
                        if (real == RealNumber.NaN)
                        {
                            return (RealNumber.NaN, false);
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
