using AngouriMath.Functions.Algebra.AnalyticalSolving;
using PeterO.Numbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AngouriMath.Limits
{
    using static Entity;
    using static Entity.Number;
    class LimitSolvers
    {
        internal static Dictionary<EDecimal, Entity>? ParseAsPolynomial(Entity expr, Variable x)
        {
            var children = Core.TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(
                 expr, entity => entity.Vars.Contains(x)
            );

            if (children is null)
                return null;

            var monomials = PolynomialSolver.GatherMonomialInformation
                <EDecimal, Core.TreeAnalyzer.PrimitiveDecimal>(children, x);
            if (monomials is null) return null;
            var filteredDictionary = new Dictionary<EDecimal, Entity>();
            foreach (var monomial in monomials)
            {
                var simplified = monomial.Value.InnerSimplify();
                if (simplified != Integer.Zero)
                {
                    filteredDictionary.Add(monomial.Key, simplified);
                }
            }
            return filteredDictionary;
        }
        private static readonly Real Infinity = Real.PositiveInfinity;
        internal static Entity? SolveBySubstitution(Entity expr, Variable x)
        {
            var res = expr.Substitute(x, Infinity);
            if (MathS.CanBeEvaluated(res))
            {
                var limit = res.Eval();
                if (limit == Real.NaN) return null;
                if (!limit.RealPart.IsFinite)
                    return limit.RealPart; // TODO: sometimes we get { oo + value * i } so we assume it is just infinity
                if (limit == Integer.Zero) return limit;

                return res;
            }
            return null;
        }

        internal static Entity? SolveAsPolynomial(Entity expr, Variable x)
        {
            var mono = ParseAsPolynomial(expr, x);
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

        internal static Entity? SolvePolynomialDivision(Entity expr, Variable x)
        {
            if (expr is Divf(var P, var Q))
            {
                var monoP = ParseAsPolynomial(P, x);
                var monoQ = ParseAsPolynomial(Q, x);

                if (monoP is { } && monoQ is { })
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
                            if (result == Real.NaN)
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

        internal static Entity? SolveAsLogarithm(Entity expr, Variable x)
        {
            if (expr is Logf(var logBase, var logArgument))
            {
                if (logBase.Vars.Contains(x))
                {
                    return SolveAsLogarithmDivision(MathS.Ln(logArgument) / MathS.Ln(logBase), x);
                }
                else
                {
                    var innerLimit = LimitFunctional.ComputeLimit(logArgument, x, Real.PositiveInfinity);
                    if (innerLimit is null) return null;
                    // do same as wolframalpha: https://www.wolframalpha.com/input/?i=ln%28-inf%29
                    if (innerLimit == Real.NegativeInfinity) return Real.PositiveInfinity;
                    if (innerLimit == Real.PositiveInfinity) return Real.PositiveInfinity;
                    if (innerLimit == Integer.Zero) return Real.NegativeInfinity;
                    return MathS.Log(logBase, innerLimit);
                }
            }
            else return null;
        }

        internal static Entity? SolveAsLogarithmDivision(Entity expr, Variable x)
        {
            if (expr is Divf(Logf(var upperLogBase, var upperLogArgument), Logf(var lowerLogBase, var lowerLogArgument)))
            {
                if (lowerLogBase.Vars.Contains(x) || upperLogBase.Vars.Contains(x)) return null;

                var upperLogLimit = LimitFunctional.ComputeLimit(upperLogArgument, x, Real.PositiveInfinity);
                var lowerLogLimit = LimitFunctional.ComputeLimit(lowerLogArgument, x, Real.PositiveInfinity);
                if (upperLogLimit is null || lowerLogLimit is null) return null;

                if ((upperLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || upperLogLimit == Integer.Zero)
                    && (lowerLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || lowerLogLimit == Integer.Zero))
                {
                    // apply L'Hôpital's rule for lim(x -> +oo) log(f(x), g(x))
                    var p = upperLogArgument.Derive(x) / upperLogArgument;
                    var q = lowerLogArgument.Derive(x) / lowerLogArgument;
                    return LimitFunctional.ComputeLimit(p / q, x, Real.PositiveInfinity);
                }
                else
                {
                    var limit = MathS.Ln(upperLogLimit) / MathS.Ln(lowerLogLimit);
                    if (MathS.CanBeEvaluated(limit))
                    {
                        var res = limit.Eval();
                        if (res == Real.NaN) return null;
                        if (!res.IsFinite || res == Integer.Zero) return res;
                        return limit;
                    }
                    return upperLogLimit / lowerLogLimit;
                }
            }

            return null;
        }
    }
}
