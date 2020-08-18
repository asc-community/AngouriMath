
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

using AngouriMath.Extensions;
using PeterO.Numbers;

namespace AngouriMath
{
    using static Entity.Number;
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>
        /// Returns a value of a definite integral of a function. Only works for one-variable functions
        /// </summary>
        /// <param name="x">Variable to integrate over</param>
        /// <param name="from">The complex lower bound for integrating</param>
        /// <param name="to">The complex upper bound for integrating</param>
        public Complex DefiniteIntegral(Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to) =>
            Integration.Integrate(this, x, from, to, 100);

        /// <summary>
        /// Returns a value of a definite integral of a function. Only works for one-variable functions
        /// </summary>
        /// <param name="x">Variable to integrate over</param>
        /// <param name="from">The real lower bound for integrating</param>
        /// <param name="to">The real upper bound for integrating</param>
        public Complex DefiniteIntegral(Variable x, EDecimal from, EDecimal to) =>
            Integration.Integrate(this, x, (from, 0), (to, 0), 100);

        /// <summary>
        /// Returns a value of a definite integral of a function. Only works for one-variable functions
        /// </summary>
        /// <param name="x">Variable to integrate over</param>
        /// <param name="from">The complex lower bound for integrating</param>
        /// <param name="to">The complex upper bound for integrating</param>
        /// <param name="stepCount">Accuracy (initially, amount of iterations)</param>
        public Complex DefiniteIntegral(Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to, int stepCount) =>
            Integration.Integrate(this, x, from, to, stepCount);
    }
    internal static class Integration
    {
        /// <summary>Numerical definite integration, see more in <see cref="Entity.DefiniteIntegral(Entity.Variable, EDecimal, EDecimal)"/></summary>
        internal static Complex Integrate(Entity func, Entity.Variable x, (EDecimal Re, EDecimal Im) from, (EDecimal Re, EDecimal Im) to, int stepCount)
        {
            System.Numerics.Complex res = 0;
            var cfunc = func.Compile(x);
            for(int i = 0; i <= stepCount; i++)
            {
                var share = ((EDecimal)i) / stepCount;
                var tmp = Complex.Create(from.Re * share + to.Re * (1 - share), from.Im * share + to.Im * (1 - share));
                res += cfunc.Substitute(tmp.ToNumerics());
            }
            return res.ToNumber() / (stepCount + 1) * (Complex.Create(to.Re, to.Im) - Complex.Create(from.Re, from.Im));
        }
    }
}
