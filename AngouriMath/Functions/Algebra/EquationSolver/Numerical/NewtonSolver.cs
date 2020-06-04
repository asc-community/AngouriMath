
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



 using AngouriMath.Core;
using AngouriMath.Functions.Algebra.NumbericalSolving;
using System;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Core.Sys.Interfaces;

namespace AngouriMath.Functions.Algebra.NumbericalSolving
{
    internal static class NumericalEquationSolver
    {
        /// <summary>
        /// Perform one iteration of searching for a root with Newton-Raphson method
        /// </summary>
        /// <param name="f"></param>
        /// <param name="df"></param>
        /// <param name="value"></param>
        /// <param name="precision"></param>
        /// <returns></returns>
        private static ComplexNumber NewtonIter(FastExpression f, FastExpression df, ComplexNumber value, int precision)
        {
            ComplexNumber prev = value;
            int minCheckIters = (int)Math.Sqrt(precision);
            for (int i = 0; i < precision; i++)
            {
                if (i == precision - 1)
                    prev = value.Copy();
                try // TODO: remove try catch in for
                {
                    value -= (f.Substitute(value) / df.Substitute(value)) as ComplexNumber;
                }
                catch (MathSException)
                {
                    throw new MathSException("Two or more variables in SolveNt is forbidden");
                }
                if (i > minCheckIters && prev == value)
                    return value;
            }
            if (Number.Abs(prev - value) > MathS.Settings.PrecisionErrorCommon)
                return RealNumber.NaN();
            else
                return value;
        }

        /// <summary>
        /// Performs a grid search with each iteration done by NewtonIter
        /// </summary>
        /// <param name="expr">
        /// The equation with one variable to be solved
        /// </param>
        /// <param name="v">
        /// The variable to solve over
        /// </param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stepCount">
        /// A complex number, thus, if stepCount.Im == 0, no operations will be performed at all. If you
        /// need to iterate over real numbers only, set it to 1, i. e. new Number(your_number, 1)
        /// </param>
        /// <param name="precision">
        /// How many approximations we need to do before we reach the most precise result.
        /// </param>
        /// <returns></returns>
        internal static Set SolveNt(Entity expr, VariableEntity v, 
            (decimal Re, decimal Im) from, 
            (decimal Re, decimal Im) to, 
            (int Re, int Im) stepCount, int precision)
        {
            MathS.Settings.FloatToRationalIterCount.Set(0);
            var res = new Set();
            var df = expr.Derive(v).Simplify().Compile(v);
            var f = expr.Simplify().Compile(v);
            for (int x = 0; x < stepCount.Re; x++)
                for (int y = 0; y < stepCount.Im; y++)
                {
                    var xShare = ((decimal)x) / stepCount.Re;
                    var yShare = ((decimal)y) / stepCount.Im;
                    var value = Number.Create(from.Re * xShare + to.Re * (1 - xShare),
                                           from.Im * yShare + to.Im * (1 - yShare));
                    var root = NewtonIter(f, df, value, precision);
                    if (root.IsDefinite() && f.Call(root).Abs() < MathS.Settings.PrecisionErrorCommon)
                        res.Add(root);
                }
            MathS.Settings.FloatToRationalIterCount.Unset();
            return res;
        }
    }
}

namespace AngouriMath
{
    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// To get Number from NumberEntity (in case of need a concrete number)
        /// </summary>
        /// <returns></returns>
        public ComplexNumber GetValue()
        {
            if (this.entType == EntType.NUMBER)
                return (this as NumberEntity).Value;
            else
                throw new MathSException("Cannot get number from expression");
        }
        
        public Set SolveNt(VariableEntity v, int precision = 30)
            => SolveNt(v, (-10, -10), (10, 10), (10, 10), precision: precision);
        public Set SolveNt(VariableEntity v, (decimal Re, decimal Im) from, (decimal Re, decimal Im) to, int precision = 30)
            => SolveNt(v, from, to, (10, 10),  precision: precision);

        /// <summary>
        /// Searches for numerical solutions via Newton's method https://en.wikipedia.org/wiki/Newton%27s_method
        /// </summary>
        /// <param name="v">
        /// Variable to solve over
        /// </param>
        /// <param name="from">
        /// Re(from) - down bound of search in real numbers, Im(from) - that in imaginary. 
        /// For example, from: new Number(-10, -10)
        /// </param>
        /// <param name="to">
        /// Re(to) - up bound of search in real numbers, Im(to) - that in imaginary.
        /// For exmaple, to: new Number(10, 10)
        /// </param>
        /// <param name="stepCount">
        /// Re(stepCount) - number of steps over real numbers, Im(stepCount) - number of steps over imaginary numbers.
        /// For example, stepCount: new Number(10, 10)
        /// </param>
        /// <param name="precision">
        /// If you get very similar roots that you think are equal, increase precision (but it will slower the algorithm)
        /// </param>
        /// <returns></returns>
        public Set SolveNt(VariableEntity v, (decimal Re, decimal Im) from, (decimal Re, decimal Im) to, (int Re, int Im) stepCount, int precision = 30)
        => NumericalEquationSolver.SolveNt(this, v, from, to, stepCount, precision);
    }
}
