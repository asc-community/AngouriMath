
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
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number
    {
        internal static Number OpSum(Number a, Number b)
        {
            HierarchyLevel level;
            (a, b, level) = Number.Functional.MakeEqual(a, b);
            return SuperSwitch(
                num => num[0] + num[1],
                num => num[0] + num[1],
                num => num[0] + num[1],
                num => num[0] + num[1],
                level,
                a, b
            );
        }

        internal static Number OpSub(Number a, Number b)
        {
            HierarchyLevel level;
            (a, b, level) = Number.Functional.MakeEqual(a, b);
            return SuperSwitch(
                num => num[0] - num[1],
                num => num[0] - num[1],
                num => num[0] - num[1],
                num => num[0] - num[1],
                level,
                a, b
            );
        }

        internal static Number OpMul(Number a, Number b)
        {
            HierarchyLevel level;
            (a, b, level) = Number.Functional.MakeEqual(a, b);
            return SuperSwitch(
                num => num[0] * num[1],
                num => num[0] * num[1],
                num => num[0] * num[1],
                num => num[0] * num[1],
                level,
                a, b
            );
        }

        internal static Number OpDiv(Number a, Number b)
        {
            HierarchyLevel level;
            (a, b, level) = Number.Functional.MakeEqual(a, b);
            return SuperSwitch(
                num => num[0] / num[1],
                num => num[0] / num[1],
                num => num[0] / num[1],
                num => num[0] / num[1],
                level,
                a, b
            );
        }

        public static Number operator +(Number a, Number b)
            => OpSum(a, b);

        public static Number operator -(Number a, Number b)
            => OpSub(a, b);

        public static Number operator *(Number a, Number b)
            => OpMul(a, b);

        public static Number operator /(Number a, Number b)
            => OpDiv(a, b);

        public static Number operator -(Number a)
            => -1 * a;

        public static Number operator +(Number a)
            => a;

        public static bool operator ==(Number a, Number b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
            (a, b, _) = Functional.MakeEqual(a, b);
            if (a.IsReal() && b.IsReal())
            {
                var aAsReal = (a as RealNumber);
                var bAsReal = (b as RealNumber);
                if (!aAsReal.IsDefinite() && !bAsReal.IsDefinite())
                    return aAsReal.State == bAsReal.State;
                else if (!aAsReal.IsDefinite() || !bAsReal.IsDefinite())
                    return false;
                // else both are defined
            }
            if (a.Type != b.Type)
                return false;
            return SuperSwitch(
                num => IntegerNumber.AreEqual(num[0], num[1]),
                num => RationalNumber.AreEqual(num[0], num[1]),
                num => RealNumber.AreEqual(num[0], num[1]),
                num => ComplexNumber.AreEqual(num[0], num[1]),
                a.Type,
                a, b
            );
        }

        public static bool operator !=(Number a, Number b)
            => !(a == b);

        /// <summary>
        /// e. g. Pow(2, 5) = 32
        /// </summary>
        /// <param name="base">
        /// The base of the exponential, base^power
        /// </param>
        /// <param name="power">
        /// The power of the exponential, base^power
        /// </param>
        /// <returns></returns>
        public static ComplexNumber Pow(Number @base, Number power)
        {
            // TODO: make it more detailed (e. g. +oo ^ +oo = +oo)
            if (power.IsInteger())
                return Functional.BinaryIntPow(@base as ComplexNumber, power.AsInt());
            var baseCom = @base.AsComplexNumber();
            var powerCom = power.AsComplexNumber();
            if (baseCom.IsDefinite() && powerCom.IsDefinite())
            {
                try
                {
                    return Functional.Downcast(
                        Complex.Pow(baseCom.AsComplex(), powerCom.AsComplex())
                    ) as ComplexNumber;
                }
                catch (OverflowException)
                {
                    return RealNumber.NaN();
                }
            }
            else
                return ComplexNumber.Indefinite(RealNumber.UndefinedState.NAN);
        }

        /// <summary>
        /// e. g. Log(2, 32) = 5
        /// </summary>
        /// <param name="base">
        /// Log's base, log(base, x) is a number y such that base^y = x
        /// </param>
        /// <param name="x">
        /// The number of which we want to get its base power
        /// </param>
        /// <returns></returns>
        public static ComplexNumber Log(RealNumber @base, Number x)
        {
            var baseCom = @base.AsComplexNumber();
            var poweredCom = x.AsComplexNumber();
            if (baseCom.IsDefinite() && poweredCom.IsDefinite())
                return Functional.Downcast(
                Complex.Log(x.AsComplex(), @base.AsDouble())
            ) as ComplexNumber;
            else
                return ComplexNumber.Indefinite(RealNumber.UndefinedState.NAN);
        }

        /// <summary>
        /// Calculates the exact value of sine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Sin(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Sin(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of cosine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Cos(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Cos(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of tangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Tan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Tan(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of cotangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Cotan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(1 / Complex.Tan(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of arcsine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arcsin(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Asin(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of arccosine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arccos(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Acos(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of arctangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arctan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Atan(num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

        /// <summary>
        /// Calculates the exact value of arccotangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arccotan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Atan(1 / num.AsComplex())) as ComplexNumber
                : RealNumber.NaN();

    }
}
