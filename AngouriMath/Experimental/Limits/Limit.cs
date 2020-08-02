using AngouriMath.Core.Numerix;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace AngouriMath.Experimental.Limits
{
    public enum ApproachFrom
    {
        Left,
        Right,
    }
    public static class Limits
    {
        private static Entity? SimplifyAndComputeLimitToInfinity(Entity expr, VariableEntity x)
        {
            expr = expr.Simplify();
            return ComputeLimitToInfinity(expr, x);
        }

        private static Entity? ComputeLimitToInfinity(Entity expr, VariableEntity x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult is {}) return substitutionResult;

            var logarithmResult = LimitSolvers.SolveAsLogarithm(expr, x);
            if (logarithmResult is {}) return logarithmResult;

            var polynomialResult = LimitSolvers.SolveAsPolynomial(expr, x);
            if (polynomialResult is {}) return polynomialResult;

            var polynomialDivisionResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialDivisionResult is {}) return polynomialDivisionResult;
            
            var logarithmDivisionResult = LimitSolvers.SolveAsLogarithmDivision(expr, x);
            if (logarithmDivisionResult is {}) return logarithmDivisionResult;

            return null;
        }

        public static Entity? ComputeLimit(Entity expr, VariableEntity x, Entity dist, ApproachFrom side)
        {
            expr = expr.DeepCopy(); // create a copy as algorithm below affects entity
            return ComputeLimitDivideEtEmpera(expr, x, dist, side);
        }

        private static Entity? ComputeLimitDivideEtEmpera(Entity expr, VariableEntity x,
            Entity dist, ApproachFrom side)
        {
            // here we try to compute limit for each children and then merge them into limit of whole expression
            var lim = ComputeLimitImpl(expr, x, dist, side);
            if (lim is {}) return lim;
            // theoretically, for cases such limit (x -> -1) 1 / (x + 1)
            // this method will return NaN, but thanks to replacement of x to an non-definite expression,
            // it is somehow compensated

            for (int i = 0; i < expr.ChildrenCount; i++)
            {
                var child = expr.GetChild(i);
                var limit = ComputeLimitDivideEtEmpera(child, x, dist, side);
                if (limit is {} && limit.IsFinite())
                    expr.SetChild(i, limit);
            }

            return ComputeLimitImpl(expr, x, dist, side);
        }

        private static Entity? ComputeLimitImpl(Entity expr, VariableEntity x, Entity dist, ApproachFrom side)
        {
            if (!expr.SubtreeIsFound(x)) return expr;

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
                        return SimplifyAndComputeLimitToInfinity(expr, x);
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
            return SimplifyAndComputeLimitToInfinity(expr, x);
        }

        public static Entity? ComputeLimit(Entity expr, VariableEntity x, Entity dist)
        {
            if(dist is NumberEntity num && !num.Value.IsFinite)
            {
                // just compute limit with no check for left/right equality
                // here approach left will be ignored anyways, as dist is infinite number
                return ComputeLimit(expr, x, dist, ApproachFrom.Left);
            }

            var fromLeft = ComputeLimit(expr, x, dist, ApproachFrom.Left);
            var fromRight = ComputeLimit(expr, x, dist, ApproachFrom.Right);
            if (fromLeft is null || fromRight is null) return null;
            if (fromLeft == fromRight) return fromLeft;
            else return RealNumber.NaN;
        }
    }
}
