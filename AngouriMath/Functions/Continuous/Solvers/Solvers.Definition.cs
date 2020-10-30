/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Functions.Algebra.AnalyticalSolving;
using System;
using static AngouriMath.Entity.Set;

namespace AngouriMath
{
    public abstract partial record Entity
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
            throw new InvalidOperationException("There should be statement to be true (e. g. equality, inequality, or some other predicate)");
        }
    }
}
