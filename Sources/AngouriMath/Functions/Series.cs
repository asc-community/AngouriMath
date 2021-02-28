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
    }
}
