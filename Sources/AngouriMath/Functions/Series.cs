/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Core.Exceptions;
using System.Collections.Generic;
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
            // pointCoefficient is the sequence of (x-a)(x-b)...
            var lastTerms = new List<(int coefficient, Entity pointCoefficients, Entity partialDerivative)>();
            var i = 0;

            while (true)
            {
                var newTerms = new List<(int coefficient, Entity pointCoefficients, Entity partialDerivative)>();
                var factorialCoeff = i > 1 ? 1 / ((Entity)i).Factorial() : 1;

                if (i > 0) {

                    foreach (var lastTerm in lastTerms)
                    {
                        // Splits into a version for every variable
                        foreach (var triple in exprToPolyVars)
                        {
                            var newPointCoeffs = (lastTerm.pointCoefficients * (triple.polyVariable - triple.point)).InnerSimplified;
                            bool foundRepeatFlag = false;

                            for (int j = 0; j < newTerms.Count; j++)
                            {
                                var newTerm = newTerms[j];

                                if (newTerm.pointCoefficients.Equals(newPointCoeffs))
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
                else newTerms.Add((1, 1, expr));

                Entity fullExpr = 0;

                foreach (var newPartialDerivative in newTerms)
                    fullExpr += newPartialDerivative.coefficient * newPartialDerivative.pointCoefficients * newPartialDerivative.partialDerivative;

                fullExpr *= factorialCoeff;

                foreach (var triples in exprToPolyVars)
                    fullExpr = fullExpr.Substitute(triples.exprVariable, triples.point).InnerSimplified;

                yield return fullExpr.InnerSimplified;

                lastTerms = newTerms;
                i++;
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
