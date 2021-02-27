/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using static AngouriMath.Entity.Set;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        /// <summary>
        /// Solves a <see cref="Statement"/>
        /// Statement is an Entity such that its value is true for
        /// any x in X, where X is the result of this method.
        /// See more about <see cref="Set"/>
        /// </summary>
        /// <param name="var">Over which variable to solve</param>
        public Set Solve(Variable var)
        {
            if (this is Statement)
            {
                var res = StatementSolver.Solve(this, var);
                return (Set)res.InnerSimplified;
            }
            if (this == var)
                return new FiniteSet(Boolean.True);
            throw new SolveRequiresStatementException();
        }
    }
}
