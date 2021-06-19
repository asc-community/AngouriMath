/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
using System.Linq;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static class Series
    {
        internal static IEnumerable<Entity> TaylorExpansionTerms(Entity expr, Variable exprVariable, Variable polyVariable, Entity point)
        {
            var currExpr = expr;
            var i = 0;
            while (true)
            {
                var num = (currExpr.Substitute(exprVariable, point) * (polyVariable - point).Pow(i)).InnerSimplified;
                if (num != Number.Integer.Zero)
                    if (i > 1)
                        yield return num / ((Entity)i).Factorial();
                    else
                        yield return num;
                else
                    yield return Number.Integer.Zero;
                currExpr = currExpr.Differentiate(exprVariable);
                i++;
            }
        }

        internal static Entity TaylorExpansion(Entity expr, Variable exprVariable, Variable polyVariable, Entity point, int termCount)
        {
            if (termCount < 0)
                throw new InvalidNumberException($"{nameof(termCount)} is supposed to be at least 0");
            var terms = new List<Entity>();
            foreach (var term in TaylorExpansionTerms(expr, exprVariable, polyVariable, point))
            {
                if (terms.Count >= termCount)
                    return TreeAnalyzer.MultiHangBinary(terms, (a, b) => a + b);
                terms.Add(term);
            }
            throw new AngouriBugException($"We cannot get here, as {nameof(TaylorExpansionTerms)} is supposed to be endless");
        }

        internal static IEnumerable<Entity> MultivariableTaylorExpansionTerms(Entity expr, (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
        {
            // This structure is just a deconstruction of the terms that are added together in a multi taylor polynomial.
            // Like n (x - a)^s (y - b)^t f'(x,y), where n is the coefficient, s and t are pointCoefficientDegrees, and then the partialDerivative
            var lastTerms = new List<(int coefficient, int[] pointCoefficientDegrees, Entity partialDerivative)>();
            var order = 0;
            var variables = exprToPolyVars.Length;

            while (true)
            {
                var newTerms = new List<(int coefficient, int[] pointCoefficientDegrees, Entity partialDerivative)>();

                if (order > 0) {

                    foreach (var lastTerm in lastTerms)
                    {
                        // We must take the partial derivative with respect to each component.
                        for (int variableIndex = 0; variableIndex < exprToPolyVars.Length; variableIndex++)
                        {
                            var triple = exprToPolyVars[variableIndex];
                            var newPointCoeffs = new int[variables];
                            lastTerm.pointCoefficientDegrees.CopyTo(newPointCoeffs,0);
                            newPointCoeffs[variableIndex] += 1;

                            bool foundRepeatFlag = false;

                            for (int newTermIndex = 0; newTermIndex < newTerms.Count; newTermIndex++)
                            {
                                var newTerm = newTerms[newTermIndex];

                                if (Enumerable.SequenceEqual(newTerm.pointCoefficientDegrees, newPointCoeffs))
                                {
                                    newTerm.coefficient += lastTerm.coefficient;
                                    foundRepeatFlag = true;
                                    break;
                                }
                            }

                            // If the partial derivative has already been computed, we just add to it's coefficient and skip this.
                            if (!foundRepeatFlag)
                            {
                                var newPartialDerivative = lastTerm.partialDerivative.Differentiate(triple.exprVariable);
                                newTerms.Add((lastTerm.coefficient, newPointCoeffs, newPartialDerivative));
                            }
                           
                        }
                    }

                }
                else newTerms.Add((1, new int[variables], expr));

                Entity fullExpr = 0;

                foreach (var newTerm in newTerms) {
                    
                    Entity pointCoefficients = 1;
                    for (int variableIndex = 0; variableIndex < exprToPolyVars.Length; variableIndex++)
                    {
                        var triple = exprToPolyVars[variableIndex];
                        pointCoefficients *= (triple.polyVariable - triple.point).Pow(newTerm.pointCoefficientDegrees[variableIndex]);
                    }

                    fullExpr += newTerm.partialDerivative * newTerm.coefficient * pointCoefficients;
                }

                foreach (var variable in exprToPolyVars)
                    fullExpr = fullExpr.Substitute(variable.exprVariable, variable.point).InnerSimplified;

                if (order > 1)
                    fullExpr /= ((Entity)order).Factorial();

                yield return fullExpr;

                lastTerms = newTerms;
                order++;
            }
        }

        internal static Entity MultivariableTaylorExpansion(Entity expr, (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars, int termCount)
        {
            if (termCount < 0)
                throw new InvalidNumberException($"{nameof(termCount)} is supposed to be at least 0");
            var terms = new List<Entity>();
            foreach (var term in MultivariableTaylorExpansionTerms(expr, exprToPolyVars))
            {
                if (terms.Count >= termCount)
                    return TreeAnalyzer.MultiHangBinary(terms, (a, b) => a + b);
                terms.Add(term);
            }
            throw new AngouriBugException($"We cannot get here, as {nameof(TaylorExpansionTerms)} is supposed to be endless");
        }
    }
}
