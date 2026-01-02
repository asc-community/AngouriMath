//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Multithreading;
using PeterO.Numbers;

namespace AngouriMath.Functions.Algebra
{
    using static Entity;
    using static Entity.Number;
    internal static class LimitSolvers
    {
        internal static Dictionary<EDecimal, Entity>? ParseAsPolynomial(Entity expr, Variable x)
        {
            var children = TreeAnalyzer.GatherLinearChildrenOverSumAndExpand(
                 expr, entity => entity.ContainsNode(x)
            );

            if (children is null)
                return null;

            var monomials = AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
                <EDecimal, TreeAnalyzer.PrimitiveDecimal>(children, x);
            if (monomials is null) return null;
            var filteredDictionary = new Dictionary<EDecimal, Entity>();
            foreach (var monomial in monomials)
            {
                var simplified = monomial.Value.InnerSimplified;
                if (simplified != Integer.Zero)
                {
                    filteredDictionary.Add(monomial.Key, simplified);
                }
            }
            return filteredDictionary;
        }
        [ConstantField] private static readonly Real Infinity = Real.PositiveInfinity;
        internal static Entity? SolveBySubstitution(Entity expr, Variable x)
        {
            // For now we remove all provideds from an expression
            // Is it a correct thing to do?
            var res = expr.Replace(e => e is Providedf(var expr, _) ? expr : e).Substitute(x, Infinity);
            if (res.Evaled is Complex limit)
            {
                MultithreadingFunctional.ExitIfCancelled();
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
            if (ParseAsPolynomial(expr, x) is { } mono)
            {
                MultithreadingFunctional.ExitIfCancelled();
                var maxPower = mono.Keys.Max() ?? throw new AngouriBugException("No null expected");
                return
                    maxPower.IsZero
                    ? mono[maxPower]
                    : maxPower.IsNegative
                    ? 0
                    : mono[maxPower].Evaled is Complex power
                    ? Infinity * power
                    : Infinity * mono[maxPower];
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
                    var maxPowerP = monoP.Keys.Max() ?? throw new AngouriBugException("No null expected");
                    var maxPowerQ = monoQ.Keys.Max() ?? throw new AngouriBugException("No null expected");
                    MultithreadingFunctional.ExitIfCancelled();
                    var maxTermP = monoP[maxPowerP];
                    var maxTermQ = monoQ[maxPowerQ];
                    if (maxPowerP.CompareTo(maxPowerQ) > 0)
                    {
                        var term = maxTermP / maxTermQ;
                        if (term.Evaled is Number eval)
                        {
                            var result = Infinity * eval;
                            return result == Real.NaN ? null : (Entity)result;
                        }
                        else return Infinity * term;
                    }
                    else if (maxPowerP.CompareTo(maxPowerQ) == 0)
                    {
                        MultithreadingFunctional.ExitIfCancelled();
                        var termPSimplified = maxTermP.InnerSimplified;
                        var termQSimplified = maxTermQ.InnerSimplified;
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
                if (logBase.ContainsNode(x))
                    return SolveAsLogarithmDivision(MathS.Ln(logArgument) / MathS.Ln(logBase), x);
                else
                {
                    MultithreadingFunctional.ExitIfCancelled();
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
                if (lowerLogBase.ContainsNode(x) || upperLogBase.ContainsNode(x)) return null;
                MultithreadingFunctional.ExitIfCancelled();
                var upperLogLimit = LimitFunctional.ComputeLimit(upperLogArgument, x, Real.PositiveInfinity);
                var lowerLogLimit = LimitFunctional.ComputeLimit(lowerLogArgument, x, Real.PositiveInfinity);
                if (upperLogLimit is null || lowerLogLimit is null) return null;
                MultithreadingFunctional.ExitIfCancelled();
                if ((upperLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || upperLogLimit == Integer.Zero)
                    && (lowerLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || lowerLogLimit == Integer.Zero))
                {
                    var p = (upperLogArgument.Differentiate(x) / upperLogArgument).InnerSimplified;
                    var q = (lowerLogArgument.Differentiate(x) / lowerLogArgument).InnerSimplified;
                    return LimitFunctional.ComputeLimit(p / q, x, Real.PositiveInfinity);
                }
                else

                {
                    var div = (MathS.Ln(upperLogLimit) / MathS.Ln(lowerLogLimit));
                    var divEvaled = div.Evaled;
                    return divEvaled switch
                    {
                        { Evaled: Complex { IsNaN: true } } => null,
                        { } res when res.ContainsNode("+oo") || res.ContainsNode("-oo") => div.InnerSimplified,
                        { Evaled: Complex } limit => limit,
                        _ => upperLogLimit / lowerLogLimit,
                    };
                }
            }

            return null;
        }
    }
}
