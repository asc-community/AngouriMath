
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
        public bool EvaluableBoolean => Evaled is Boolean;

        /// <summary>
        /// Evaluates a given expression to one boolean or throws exception
        /// </summary>
        /// <returns>
        /// <see cref="Boolean"/>
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this entity cannot be represented as a simple boolean.
        /// <see cref="EvalBoolean"/> should be used to check beforehand.
        /// </exception>
        public Boolean EvalBoolean() =>
            Evaled is Boolean value ? value :
                throw new CannotEvalException
                    ($"Result cannot be represented as a simple boolean! Use {nameof(EvaluableBoolean)} to check beforehand.");
    }
}