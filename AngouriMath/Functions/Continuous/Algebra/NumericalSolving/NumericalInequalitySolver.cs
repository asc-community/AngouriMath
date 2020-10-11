/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System.Linq;
using AngouriMath.Core;
using AngouriMath.Core.Exceptions;
using AngouriMath.Extensions;

namespace AngouriMath.Functions.Algebra.NumericalSolving
{
    using static AngouriMath.Entity.Set;
    using static Entity;
    using static Entity.Number;
    internal static class NumericalInequalitySolver
    {
        /// <summary>
        /// <paramref name="expr"/> must contain only<see cref="Variable"/> <paramref name="x"/> as the variable
        /// </summary>
        /// <param name="expr">This must only contain one variable, which is <paramref name="x"/></param>
        /// <param name="x">The only variable</param>
        /// <param name="sign">The relation of the expression to zero.</param>
        internal static Set Solve(Entity expr, Variable x, MathS.Inequality sign)
        {
            if (expr.Vars.Single() != x)
                throw new MathSException($"{nameof(expr)} should only contain {nameof(Variable)} {nameof(x)}");
            bool Corresponds(Real val) => sign switch
            {
                MathS.Inequality.GreaterThan   => val >  0,
                MathS.Inequality.LessThan      => val <  0,
                MathS.Inequality.GreaterEquals => val >= 0,
                MathS.Inequality.LessEquals    => val <= 0,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(sign), (int)sign, typeof(MathS.Inequality))
            };

            var compiled = expr.Compile(x);
            var roots = expr.SolveNt(x);
            var realRoots = roots.OfType<Real>().OrderBy(x => x).ToList();
            var realRootsSet = realRoots.ToSet();
            if (realRoots.Count > 0)
            {
                realRoots.Insert(0, realRoots[0] - 5);
                realRoots.Insert(realRoots.Count, realRoots[realRoots.Count - 1] + 5);
            }
            var resultBuilder = new FiniteSetBuilder();
            for(int i = 0; i < realRoots.Count - 1; i++)
            {
                var left = realRoots[i];
                var right = realRoots[i + 1];
                var point = (left + right) / 2;
                var val = compiled.Call(point.ToNumerics());
                //if (Corresponds(val.AsRealNumber()))
                if (Corresponds(val.Real))
                    resultBuilder.Add(new Interval(left, false, right, false));
            }
            var result = resultBuilder.ToFiniteSet();
            return (sign & MathS.Inequality.EqualsFlag) != 0 ? MathS.Union(realRootsSet, result) : result;
        }
    }
}