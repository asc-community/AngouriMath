

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

using System;
using System.Numerics;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.Numerix;
using AngouriMath.Functions;
using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        public abstract partial record Num
        {
            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(RealNumber num)
            {
                if (!num.IsFinite)
                    return false;
                return Functional.IsZero(num.Value);
            }

            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(EDecimal num)
            {
                return num.Abs().LessThan(MathS.Settings.PrecisionErrorZeroRange);
            }

            /// <summary>
            /// Checks whether a number is zero
            /// </summary>
            /// <param name="num"></param>
            /// <returns></returns>
            public static bool IsZero(ComplexNumber num)
                => IsZero(num.Real) && IsZero(num.Imaginary);

            /// <summary>
            /// This class is developed for some additional functions
            /// Some functions are public
            /// </summary>
            public static class Functional
            {
                internal static bool IsZero(EDecimal value)
                {
                    return value.Abs().LessThanOrEquals(MathS.Settings.PrecisionErrorZeroRange);
                }
            }
        }
    }
}