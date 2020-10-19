/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Functions.Continuous.Solvers.SetSolver;
using static AngouriMath.Entity;
using static AngouriMath.Entity.Set;

namespace AngouriMath.Functions.Algebra.AnalyticalSolving
{
    internal static class StatementSolver
    {
        private static Entity Minus(Entity left, Entity right)
        {
            if (left.Evaled == 0)
                return -right;
            if (right.Evaled == 0)
                return left;
            return left - right;
        }

        internal static Set Solve(Entity expr, Variable x)
            => expr switch
            {
                Equalsf(var left, var right) when left is Set || right is Set
                    => AnalyticalSetSolver.Solve(left, right, x),

                Equalsf(var left, var right) when left is not Set && right is not Set
                    => AnalyticalEquationSolver.Solve(left - right, x),

                Equalsf => Empty,

                Andf(var left, var right) => 
                    MathS.Intersection(Solve(left, x), Solve(right, x)),
                Orf(var left, var right) => 
                    MathS.Union(Solve(left, x), Solve(right, x)),
                Impliesf(var left, var right) => 
                    MathS.Union(MathS.SetSubtraction(expr.Codomain, Solve(left, x)), Solve(right, x)),

                // TODO: there should be universal set to subtract from when inverting
                Greaterf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(left, right), x),
                LessOrEqualf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x)
                    .Unite(AnalyticalEquationSolver.Solve(Minus(left, right), x)),
                GreaterOrEqualf(var left, var right) => MathS.Union(AnalyticalInequalitySolver.Solve(Minus(left, right), x), AnalyticalEquationSolver.Solve(Minus(left, right), x)),

                Lessf(var left, var right) => 
                    AnalyticalInequalitySolver.Solve(Minus(right, left), x),

                Variable when expr == x => new FiniteSet(true),

                // TODO: Although piecewise needed?
                _ => Set.Empty
            };
    }
}
