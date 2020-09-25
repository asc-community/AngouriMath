
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

using AngouriMath.Core.Exceptions;
using System;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public bool EvaluableNumerical => Evaled is Complex;

        /// <summary>
        /// Evaluates a given expression to one number or throws exception
        /// </summary>
        /// <returns>
        /// <see cref="Complex"/> since new version
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this entity cannot be represented as a simple number.
        /// <see cref="EvaluableNumerical"/> should be used to check beforehand.
        /// </exception>
        public Complex EvalNumerical() =>
            Evaled is Complex value ? value :
                throw new CannotEvalException
                    ($"Result cannot be represented as a simple number! Use {nameof(EvaluableNumerical)} to check beforehand.");
    }
}