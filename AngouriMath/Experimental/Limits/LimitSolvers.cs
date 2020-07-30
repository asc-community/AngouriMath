using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Experimental.Limits
{
    class LimitSolvers
    {
        private static RealNumber Infinity = RealNumber.PositiveInfinity;
        internal static Entity? SolveBySubstitution(Entity expr, VariableEntity x)
        {
            var res = expr.Substitute(x, Infinity);
            if(MathS.CanBeEvaluated(res))
            {
                var limit = res.Eval();
                if (limit != RealNumber.NaN)
                    return limit;
            }
            return null;
        }

        internal static Entity? SolvePolynomialDivision(Entity expr, VariableEntity x)
        {
            var pattern = Patterns.any1 / Patterns.any2;
            if(pattern.Match(expr))
            {
                var P = expr.GetChild(0);
                var Q = expr.GetChild(1);
                
                var monoP = TreeAnalyzer.ParseAsPolynomial<EDecimal>(P, x);
                var monoQ = TreeAnalyzer.ParseAsPolynomial<EDecimal>(Q, x);

                Entity? result = null;
                if(monoP is { } && monoQ is { })
                {
                    var maxPowerP = monoP.Keys.Max();
                    var maxPowerQ = monoQ.Keys.Max();

                    var maxTermP = monoP[maxPowerP];
                    var maxTermQ = monoQ[maxPowerQ];
                    if (maxPowerP.CompareTo(maxPowerQ) > 0)
                    {
                        var term = maxTermP / maxTermQ;
                        if (MathS.CanBeEvaluated(term))
                        {
                            result = Infinity * term.Eval();
                        }
                        else
                            result = Infinity * term;
                    }
                    else if (maxPowerP.CompareTo(maxPowerQ) == 0)
                    {
                        var termPSimplified = maxTermP.InnerSimplify();
                        var termQSimplified = maxTermQ.InnerSimplify();
                        return termPSimplified / termQSimplified;
                    }
                    else
                    {
                        return 0;
                    }
                    if (result == RealNumber.NaN) result = null; // avoid returning NaN
                }
                return result;
            }
            else
            {
                var pol = TreeAnalyzer.ParseAsPolynomial<EDecimal>(expr, x);
                if (pol is null)
                    return null;
                var maxPower = pol.Keys.Max();
                if (maxPower.CompareTo(EDecimal.Zero) > 0)
                    return pol[maxPower] * Infinity;
                else if (maxPower.CompareTo(EDecimal.Zero) == 0)
                    return pol[maxPower];
                else
                    return 0;
            }
            return null;
        }
    }
}
