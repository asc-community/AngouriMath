using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngouriMath.Experimental.Limits
{
    class LimitSolvers
    {
        private static RealNumber Infinity = RealNumber.PositiveInfinity;
        static internal Entity? SolveBySubstitution(Entity expr, VariableEntity x)
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

        static internal Entity? SolvePolynomialDivision(Entity expr, VariableEntity x)
        {
            var pattern = Patterns.any1 / Patterns.any2;
            if(pattern.Match(expr))
            {
                var P = expr.GetChild(0);
                var Q = expr.GetChild(1);
                var monoP = TreeAnalyzer.GatherAllPossiblePolynomials(P, replaceVars: false).monoInfo;
                var monoQ = TreeAnalyzer.GatherAllPossiblePolynomials(Q, replaceVars: false).monoInfo;

                if(monoP.ContainsKey(x.Name) && monoQ.ContainsKey(x.Name))
                {
                    var maxTermP = monoP[x.Name].First();
                    var maxTermQ = monoQ[x.Name].First();
                    if (maxTermP.Key.CompareTo(maxTermQ.Key) > 0)
                    {
                        if (MathS.CanBeEvaluated(maxTermP.Value))
                            return Infinity * maxTermP.Value.Eval();
                        else
                            return (Infinity * maxTermP.Value).Simplify();
                    }
                    else if (maxTermP.Key.CompareTo(maxTermQ.Key) < 0)
                    {
                        return 0;
                    }
                    else
                    {
                        var term = maxTermP.Value / maxTermQ.Value;
                        if (MathS.CanBeEvaluated(term))
                            return Infinity * term.Eval();
                        else
                            return (Infinity * term).Simplify();
                    }
                }   
            }
            return null;
        }
    }
}
