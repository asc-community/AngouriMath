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
        internal static Entity? ComputeLimitToInfinity(Entity expr, VariableEntity x)
        {
            var substitutionResult = LimitSolvers.SolveBySubstitution(expr, x);
            if (substitutionResult != null) return substitutionResult;

            var polynomialResult = LimitSolvers.SolvePolynomialDivision(expr, x);
            if (polynomialResult != null) return polynomialResult;

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
                            throw new MathSException("NaN value as limit approach point");
                        }
                        // if x -> -oo just make -x -> +oo
                        if (real == RealNumber.NegativeInfinity)
                        {
                            expr = expr.Substitute(x, -x);
                        }
                        // compute limit for x -> +oo
                        return ComputeLimitToInfinity(expr, x);
                    }
                }    
                else
                {
                    throw new SolvingException($"limits cannot be currently evaluated for complex numbers: lim({x} -> {dist}) {expr}");
                }
            }
            // handle side from which the limit is approached
            if (side == ApproachFrom.Left)
                expr = expr.Substitute(x, dist - 1 / x);
            else
                expr = expr.Substitute(x, dist + 1 / x);
            // lim(x -> 3-) x <=> lim(x -> 0+) 3 - x <=> lim(x -> +oo) 3 - 1 / x
            // lim(x -> 3+) x <=> lim(x -> 0+) 3 + x <=> lim(x -> +oo) 3 + 1 / x
            return ComputeLimitToInfinity(expr, x);
        }
    }
}
