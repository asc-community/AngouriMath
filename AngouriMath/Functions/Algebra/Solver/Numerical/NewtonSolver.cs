
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



﻿using AngouriMath.Core;
using AngouriMath.Functions.Algebra.NumbericalSolving;
using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Functions.Algebra.NumbericalSolving
{
    internal static class NumericalSolver
    {
        private static Number NewtonIter(FastExpression f, FastExpression df, Number value, int precision)
        {
            Number prev = value;
            int minCheckIters = (int)Math.Sqrt(precision);
            for (int i = 0; i < precision; i++)
            {
                if (i == precision - 1)
                    prev = value.Copy();
                try
                {
                    value -= f.Substitute(value) / df.Substitute(value);
                }
                catch (MathSException)
                {
                    throw new MathSException("Two or more variables in SolveNt is forbidden");
                }
                if (i > minCheckIters && prev == value)
                    return value;
            }
            if (Number.Abs(prev - value) > 0.01)
                return Number.Null;
            else
                return value;
        }


        internal static NumberSet SolveNt(Entity expr, VariableEntity v, Number from, Number to, Number stepCount, int precision = 30)
        {
            var res = new NumberSet();
            if (from == null)
                from = new Number(-10, -10);
            if (to == null)
                to = new Number(10, 10);
            int xStep = (int)stepCount.Re;
            int yStep = (int)stepCount.Im;
            var df = expr.Derive(v).Simplify().Compile(v);
            var f = expr.Simplify().Compile(v);
            for (int x = 0; x < xStep; x++)
                for (int y = 0; y < yStep; y++)
                {
                    double xShare = ((double)x) / xStep;
                    double yShare = ((double)y) / yStep;
                    var value = new Number(from.Re * xShare + to.Re * (1 - xShare),
                                           from.Im * yShare + to.Im * (1 - yShare));
                    var root = NewtonIter(f, df, value, precision);
                    if (!root.IsNull)
                        res.Include(root);
                }
            return res;
        }
    }
}

namespace AngouriMath
{
    public abstract partial class Entity
    {
        /// <summary>
        /// To get Number from NumberEntity (in case of need a concrete number)
        /// </summary>
        /// <returns></returns>
        public Number GetValue()
        {
            if (this.entType == EntType.NUMBER)
                return (this as NumberEntity).Value;
            else
                throw new MathSException("Cannot get number from expression");
        }
        
        public NumberSet SolveNt(VariableEntity v, int precision = 30)
            => SolveNt(v, new Number(-10, -10), new Number(10, 10), new Number(10, 10), precision: precision);
        public NumberSet SolveNt(VariableEntity v, Number from, Number to, int precision = 30)
            => SolveNt(v, from, to, new Number(10, 10),  precision: precision);

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
        public NumberSet SolveNt(VariableEntity v, Number from, Number to, Number stepCount, int precision = 30)
        => NumericalSolver.SolveNt(this, v, from, to, stepCount, precision);
    }
}
