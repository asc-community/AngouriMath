
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
 using System.Numerics;
 using AngouriMath.Core.Numerix;
 using AngouriMath.Core.Sys.Interfaces;
 using AngouriMath.Extensions;

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
        private static ComplexNumber NewtonIter(FastExpression f, FastExpression df, Complex value, int precision)
        {
            Complex prev = value;

            Complex ChooseGood()
            {
                if (Complex.Abs(prev - value) > (double)MathS.Settings.PrecisionErrorCommon.Value)
                    return double.NaN;
                else
                    return value;
            }

            int minCheckIters = (int)Math.Sqrt(precision);
            for (int i = 0; i < precision; i++)
            {
                if (i == precision - 1)
                    prev = value;//.Copy();
                try // TODO: remove try catch in for
                {
                    
                    var dfv = df.Substitute(value);
                    if (dfv == 0)
                        return ChooseGood();
                    value -= f.Substitute(value) / dfv;
                }
                catch (OverflowException)
                {
                    return ChooseGood();
                }
                if (i > minCheckIters && prev == value)
                    return value;
            }
            return ChooseGood();
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
            NewtonSetting settings)
        {
            if (MathS.Utils.GetUniqueVariables(expr).Count != 1)
                throw new MathSException("Two or more or less than one variables in SolveNt is prohibited");
            MathS.Settings.FloatToRationalIterCount.Set(0);
            var res = new Set();
            var df = expr.Derive(v).Simplify().Compile(v);
            var f = expr.Simplify().Compile(v);
            for (int x = 0; x < settings.StepCount.Re; x++)
                for (int y = 0; y < settings.StepCount.Im; y++)
                {
                    var xShare = ((decimal)x) / settings.StepCount.Re;
                    var yShare = ((decimal)y) / settings.StepCount.Im;
                    var value = Number.Create(settings.From.Re * xShare + settings.To.Re * (1 - xShare),
                                           settings.From.Im * yShare + settings.To.Im * (1 - yShare));
                    var root = NewtonIter(f, df, value.AsComplex(), settings.Precision);
                    if (root.IsDefinite() && f.Call(root.AsComplex()).ToComplexNumber().Abs() < MathS.Settings.PrecisionErrorCommon)
                        res.Add(root);
                }
            MathS.Settings.FloatToRationalIterCount.Unset();
            return res;
        }
    }
}

namespace AngouriMath
{
    public class NewtonSetting
    {
        public (decimal Re, decimal Im) From;
        public (decimal Re, decimal Im) To;
        public (int Re, int Im) StepCount;
        public int Precision;

        public NewtonSetting()
        {
            From = (-10, -10);
            To = (10, 10);
            StepCount = (10, 10);
            Precision = 30;
        }
    }

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

        /// <summary>
        /// Searches for numerical solutions via Newton's method https://en.wikipedia.org/wiki/Newton%27s_method
        /// To change parameters see MathS.Settings.NewtonSolver
        /// </summary>
        /// <returns></returns>
        public Set SolveNt(VariableEntity v)
        => NumericalEquationSolver.SolveNt(this, v, MathS.Settings.NewtonSolver);
    }
}
