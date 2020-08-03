using AngouriMath.Core.Numerix;
using AngouriMath.Core.TreeAnalysis;
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AngouriMath.Limits
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
                if (!limit.Real.IsFinite)
                    return limit.Real; // TODO: sometimes we get { oo + value * i } so we assume it is just infinity
                if (limit == IntegerNumber.Zero) return limit;

                return res;
            }
            return null;
        }

        internal static Entity? SolveAsPolynomial(Entity expr, VariableEntity x)
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
                else if (MathS.CanBeEvaluated(mono[maxPower]))
                {
                    return Infinity * mono[maxPower].Eval();
                }
                else
                { 
                    return Infinity * mono[maxPower];
                }
            }
            else return null;
        }


        private static Pattern polynomialDivisionPattern = Patterns.any1 / Patterns.any2;
        internal static Entity? SolvePolynomialDivision(Entity expr, VariableEntity x)
        {
            if(polynomialDivisionPattern.Match(expr))
            {
                var P = expr.GetChild(0);
                var Q = expr.GetChild(1);
                
                var monoP = TreeAnalyzer.ParseAsPolynomial<EDecimal>(P, x);
                var monoQ = TreeAnalyzer.ParseAsPolynomial<EDecimal>(Q, x);

                
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
                            var result = Infinity * term.Eval();
                            if (result == RealNumber.NaN)
                                return null;
                            else
                                return result;
                        }
                        else return Infinity * term;
                    }
                    else if (maxPowerP.CompareTo(maxPowerQ) == 0)
                    {
                        var termPSimplified = maxTermP.InnerSimplify();
                        var termQSimplified = maxTermQ.InnerSimplify();
                        return termPSimplified / termQSimplified;
                    } 
                    else return 0;
                }
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

                if (logBase.SubtreeIsFound(x))
                {
                    return SolveAsLogarithmDivision(MathS.Ln(logArgument) / MathS.Ln(logBase), x);
                }
                else
                {
                    var innerLimit = Limits.ComputeLimit(logArgument, x, RealNumber.PositiveInfinity);
                    if (innerLimit is null) return null;
                    // do same as wolframalpha: https://www.wolframalpha.com/input/?i=ln%28-inf%29
                    if (innerLimit == RealNumber.NegativeInfinity) return RealNumber.PositiveInfinity;
                    if (innerLimit == RealNumber.PositiveInfinity) return RealNumber.PositiveInfinity;
                    if (innerLimit == IntegerNumber.Zero) return RealNumber.NegativeInfinity;
                    return MathS.Log(logBase, innerLimit);
                }
            }
            else return null;
        }

        private static Pattern LogarithmDivisionPattern = Logf.PHang(Patterns.any1, Patterns.any2) / Logf.PHang(Patterns.any3, Patterns.any2);
        internal static Entity? SolveAsLogarithmDivision(Entity expr, VariableEntity x)
        {
            if (LogarithmDivisionPattern.Match(expr))
            {
                var upperLogArgument = expr.GetChild(0).GetChild(1);
                var lowerLogArgument = expr.GetChild(1).GetChild(1);
                var upperLogBase = expr.GetChild(0).GetChild(0);
                var lowerLogBase = expr.GetChild(1).GetChild(0);
                if (lowerLogBase.SubtreeIsFound(x) || upperLogBase.SubtreeIsFound(x)) return null;

                var upperLogLimit = Limits.ComputeLimit(upperLogArgument, x, RealNumber.PositiveInfinity);
                var lowerLogLimit = Limits.ComputeLimit(lowerLogArgument, x, RealNumber.PositiveInfinity);
                if (upperLogLimit is null || lowerLogLimit is null) return null;

                if ((upperLogLimit.SubtreeIsFound(RealNumber.PositiveInfinity) ||
                     upperLogLimit.SubtreeIsFound(RealNumber.NegativeInfinity)  ||
                     upperLogLimit == IntegerNumber.Zero)
                    &&
                    (lowerLogLimit.SubtreeIsFound(RealNumber.PositiveInfinity) ||
                     lowerLogLimit.SubtreeIsFound(RealNumber.NegativeInfinity)  ||
                     lowerLogLimit == IntegerNumber.Zero))
                {
                    // apply L'Hôpital's rule for lim(x -> +oo) log(f(x), g(x))
                    var p = upperLogArgument.Derive(x) / upperLogArgument;
                    var q = lowerLogArgument.Derive(x) / lowerLogArgument;
                    return Limits.ComputeLimit(p / q, x, RealNumber.PositiveInfinity);
                }
                else
                {
                    var limit = MathS.Ln(upperLogLimit) / MathS.Ln(lowerLogLimit);
                    if(MathS.CanBeEvaluated(limit))
                    {
                        var res = limit.Eval();
                        if (res == RealNumber.NaN) return null;
                        if (!res.IsFinite || res == IntegerNumber.Zero) return res;
                        return limit;
                    }
                    return upperLogLimit / lowerLogLimit;
                }
            }

            return null;
        }
    }
}
