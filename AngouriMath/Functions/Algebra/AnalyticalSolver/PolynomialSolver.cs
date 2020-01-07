using System;
using System.Collections.Generic;
using System.Text;
using AngouriMath.Core.TreeAnalysis;

namespace AngouriMath.Functions.Algebra.AnalyticalSolver
{
    internal static class PolynomialSolver
    {
        internal static bool IsPolynomial(Entity expr,
                                   Entity subtree /* e. g. x or cos(x)*/,
                                   VariableEntity x /* further we need to check if a coef already contains the var*/)
        {
            expr = expr.Expand(); // (x + 1) * x => x^2 + x
            var children = TreeAnalyzer.LinearChildren(expr, "sumf", "minusf", Const.FuncIfSum);

            // Check if all are like {1} * x^n
            foreach (var child in children)
                if (!IsVarPow(subtree, child))
                    return false;

            return true;
        }

        internal static bool IsVarPow(Entity subtree, Entity expr)
        {
            expr = expr.Simplify(); // x * x => x ^ 2
            return true; // Under word
        }
    }
}
