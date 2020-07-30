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
                if (limit == RealNumber.NaN) return null;
                if (!limit.IsFinite) return limit;

                return res;
            }
            return null;
        }

        internal static Entity? SolveAsPolynome(Entity expr, VariableEntity x)
        {
            var mono = TreeAnalyzer.ParseAsPolynomial<EDecimal>(expr, x);
            if (mono is { })
            {
                var maxPower = mono.Keys.Max();
                if (maxPower.IsZero)
                {
                    return mono[maxPower];
                }
                else if (maxPower.IsNegative)
                {
                    return 0;
                }
                else
                {
                    if (MathS.CanBeEvaluated(mono[maxPower]))
                        return Infinity * mono[maxPower].Eval();
                    else
                        return Infinity * mono[maxPower];
                }
            }
            else return null;
        }


        private static Pattern polynomDivisionPattern = Patterns.any1 / Patterns.any2;
        internal static Entity? SolvePolynomialDivision(Entity expr, VariableEntity x)
        {
            if(polynomDivisionPattern.Match(expr))
            {
                var P = expr.GetChild(0);
                var Q = expr.GetChild(1);
                
                var monoP = TreeAnalyzer.ParseAsPolynomial<EDecimal>(P, x);
                var monoQ = TreeAnalyzer.ParseAsPolynomial<EDecimal>(Q, x);

                if (monoQ is null) monoQ = new Dictionary<EDecimal, Entity>() { { EDecimal.Zero, 1 } }; // P -> P / 1

                Entity? result = null;
                if(monoP is { })
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
            return null;
        }

        private static Pattern LogarithmPattern = Logf.PHang(Patterns.any1, Patterns.any2);
        internal static Entity? SolveAsLogarithm(Entity expr, VariableEntity x)
        {
            if (LogarithmPattern.Match(expr))
            {
                var logBase = expr.GetChild(0);
                var logArgument = expr.GetChild(1);

                if (logBase.ContainsName("x"))
                {
                    // apply L'Hôpital's rule for lim(x -> +oo) log(f(x), g(x))
                    var p = logArgument.Derive(x) / logArgument;
                    var q = logBase.Derive(x) / logBase;
                    return Limit.ComputeLimit(p / q, x, RealNumber.PositiveInfinity, ApproachFrom.Left);
                }
                else
                {
                    var innerLimit = Limit.ComputeLimitToInfinity(logArgument, x);
                    if (innerLimit is null) return null;
                    // do same as wolframalpha: https://www.wolframalpha.com/input/?i=ln%28-inf%29
                    if (innerLimit == RealNumber.NegativeInfinity) return RealNumber.PositiveInfinity;
                    if (innerLimit == RealNumber.PositiveInfinity) return RealNumber.PositiveInfinity;
                    return MathS.Log(logBase, innerLimit);
                }
            }
            else return null;
        }
    }
}
