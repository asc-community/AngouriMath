/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core;
using PeterO.Numbers;
using System.Collections.Generic;
using System.Linq;

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

            var monomials = Algebra.AnalyticalSolving.PolynomialSolver.GatherMonomialInformation
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
            var res = expr.Substitute(x, Infinity);
            if (res.Evaled is Complex limit)
            {
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
                var maxPower = mono.Keys.Max();
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
                    var maxPowerP = monoP.Keys.Max();
                    var maxPowerQ = monoQ.Keys.Max();

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

                var upperLogLimit = LimitFunctional.ComputeLimit(upperLogArgument, x, Real.PositiveInfinity);
                var lowerLogLimit = LimitFunctional.ComputeLimit(lowerLogArgument, x, Real.PositiveInfinity);
                if (upperLogLimit is null || lowerLogLimit is null) return null;

                if ((upperLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || upperLogLimit == Integer.Zero)
                    && (lowerLogLimit.Nodes.Any(child => child == Real.PositiveInfinity || child == Real.NegativeInfinity)
                     || lowerLogLimit == Integer.Zero))
                {
                    // apply L'Hôpital's rule for lim(x -> +oo) log(f(x), g(x))
                    var p = (upperLogArgument.Differentiate(x) / upperLogArgument).InnerSimplified;
                    var q = (lowerLogArgument.Differentiate(x) / lowerLogArgument).InnerSimplified;
                    return LimitFunctional.ComputeLimit(p / q, x, Real.PositiveInfinity);
                }
                else
                    return (MathS.Ln(upperLogLimit) / MathS.Ln(lowerLogLimit)) switch
                    {
                        { Evaled: Complex { IsNaN: true } } => null,
                        { Evaled: (Complex { IsFinite: false } or Integer(0)) and var res } => res,
                        { Evaled: Complex } limit => limit,
                        _ => upperLogLimit / lowerLogLimit,
                    };
            }

            return null;
        }
    }
}
