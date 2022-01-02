//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using static AngouriMath.Entity;

namespace AngouriMath.Functions
{
    internal static class Series
    {
        internal static Entity TaylorExpansion(Entity expr, int termCount, params (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
        {
            if (exprToPolyVars.Length == 1)
                return SingleTaylorExpansion(expr, termCount, exprToPolyVars[0].exprVariable, exprToPolyVars[0].polyVariable, exprToPolyVars[0].point);
            else
                return MultivariableTaylorExpansion(expr, termCount, exprToPolyVars);
        }

        internal static Entity SingleTaylorExpansion(Entity expr, int termCount, Variable exprVariable, Variable polyVariable, Entity point)
        {
            if (termCount < 0)
                throw new InvalidNumberException($"{nameof(termCount)} is supposed to be at least 0");
            var terms = new List<Entity>();
            foreach (var term in SingleTaylorExpansionTerms(expr, exprVariable, polyVariable, point))
            {
                if (terms.Count >= termCount)
                    return TreeAnalyzer.MultiHangBinary(terms, (a, b) => a + b);
                terms.Add(term);
            }
            throw new AngouriBugException($"We cannot get here, as {nameof(SingleTaylorExpansionTerms)} is supposed to be endless");
        }

        internal static IEnumerable<Entity> SingleTaylorExpansionTerms(Entity expr, Variable exprVariable, Variable polyVariable, Entity point)
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

        internal static Entity MultivariableTaylorExpansion(Entity expr, int termCount, params (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
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
            throw new AngouriBugException($"We cannot get here, as {nameof(MultivariableTaylorExpansionTerms)} is supposed to be endless");
        }

        internal static IEnumerable<Entity> MultivariableTaylorExpansionTerms(Entity expr, params (Variable exprVariable, Variable polyVariable, Entity point)[] exprToPolyVars)
        {
            // This structure is just a deconstruction of the terms that are added together in a multi taylor polynomial.
            // Like n (x - a)^s (y - b)^t f'(x,y), where n is the coefficient, s and t are pointCoefficientDegrees, and then the partialDerivative
            // Many of these need to be computed and added together per computed expansion term.
            var lastTerms = new List<(int coefficient, int[] pointCoefficientDegrees, Entity partialDerivative)>();
            var order = 0;
            var variables = exprToPolyVars.Length;

            while (true)
            {
                var newTerms = new List<(int coefficient, int[] pointCoefficientDegrees, Entity partialDerivative)>();

                // Base case: initial term is just the given expression.
                if (order == 0)
                    newTerms.Add((1, new int[variables], expr));
                else
                {
                    // Iterative step: take the partial derivative of each of the previous terms with respect to each of the variables.
                    foreach (var (coefficient, pointCoefficientDegrees, partialDerivative) in lastTerms)
                    {
                        for (int variableIndex = 0; variableIndex < exprToPolyVars.Length; variableIndex++)
                        {
                            // Compute first what our point coefficient degrees will be, so that we don't repeat the computation of
                            // any partial derivatives, like f_xy vs f_yx.
                            var (exprVariable, polyVariable, point) = exprToPolyVars[variableIndex];
                            var newPointCoeffs = new int[variables];
                            // The new degree is the same as the parent term's, times an extra (x - a) or whichever variable.
                            pointCoefficientDegrees.CopyTo(newPointCoeffs, 0);
                            newPointCoeffs[variableIndex] += 1;

                            // The term is a repeat if it's coefficients match one previously computed.
                            bool foundRepeatFlag = false;
                            for (int newTermIndex = 0; newTermIndex < newTerms.Count; newTermIndex++)
                            {
                                var newTerm = newTerms[newTermIndex];

                                if (Enumerable.SequenceEqual(newPointCoeffs, newTerm.pointCoefficientDegrees))
                                {
                                    // If the match is found, instead of creating a whole new repeat term, we just add
                                    // to the match's coefficient. This ends up making a neat binomial coefficient pattern.
                                    newTerm.coefficient += coefficient;
                                    newTerms[newTermIndex] = newTerm;
                                    foundRepeatFlag = true;
                                    break;
                                }
                            }

                            // If no match is found, we add it to the new term list.
                            if (!foundRepeatFlag)
                            {
                                var newPartialDerivative = partialDerivative.Differentiate(exprVariable);
                                newTerms.Add((coefficient, newPointCoeffs, newPartialDerivative));
                            }

                        }
                    }

                }


                // From the list of deconstructed terms, we put assemble the term and sum them all up.
                var fullExpressionTerms = new List<Entity>();
                foreach (var (coefficient, pointCoefficientDegrees, partialDerivative) in newTerms)
                {
                    // pointCoefficients are transformed into their proper (x - a)^t... forms.
                    Entity pointCoefficients = 1;
                    for (int variableIndex = 0; variableIndex < exprToPolyVars.Length; variableIndex++)
                    {
                        var (exprVariable, polyVariable, point) = exprToPolyVars[variableIndex];
                        pointCoefficients *= (polyVariable - point).Pow(pointCoefficientDegrees[variableIndex]);
                    }

                    // Solve the partial derivative at the given point,
                    var solvedPartialDerivative = partialDerivative;
                    foreach (var (exprVariable, polyVariable, point) in exprToPolyVars)
                        solvedPartialDerivative = solvedPartialDerivative.Substitute(exprVariable, point);

                    var fullTerm = solvedPartialDerivative * coefficient * pointCoefficients;

                    // tack on the coefficient information,
                    fullExpressionTerms.Add(fullTerm);
                }

                Entity fullExpr = TreeAnalyzer.MultiHangBinary(fullExpressionTerms, (a, b) => a + b);
                fullExpr = fullExpr.InnerSimplified;

                if (fullExpr.Equals(Number.Integer.Zero))
                    yield return Number.Integer.Zero;
                else
                {
                    // and divide that all by the proper factorial.
                    if (order > 1)
                        fullExpr /= ((Entity)order).Factorial();

                    yield return fullExpr;
                }

                lastTerms = newTerms;
                order++;
            }
        }
    }
}
