

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
using AngouriMath.Functions;

namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number
    {
        /*
         *
         * This list represents the only possible way to explicitly create numeric instances
         * It will automatically downcast the result for you, so that 1.0 is an IntegerNumber
         * To avoid it, you may temporarily disable it
         *
         *   MathS.Settings.DowncastingEnabled.Set(false);
         *     var yourNum = Number.Create(1.0);
         *   MathS.Settings.DowncastingEnabled.Unset();
         *
         */

        /// <summary>
        /// Creates an instance of ComplexNumber from System.Numerics.Complex
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// ComplexNumber
        /// </returns>
        public static ComplexNumber Create(Complex value)
            => Number.Functional.Downcast(new ComplexNumber(value)) as ComplexNumber;

        /// <summary>
        /// Creates an instance of IntegerNumber from long
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// IntegerNumber
        /// </returns>
        public static IntegerNumber Create(long value)
            => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;

        /// <summary>
        /// Creates an instance of IntegerNumber from System.Numerics.BigInteger
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// IntegerNumber
        /// </returns>
        public static IntegerNumber Create(BigInteger value)
            => Number.Functional.Downcast(new IntegerNumber(value)) as IntegerNumber;

        /// <summary>
        /// Creates an instance of IntegerNumber from int
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// IntegerNumber
        /// </returns>
        public static IntegerNumber Create(int value) 
            => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;

        /// <summary>
        /// Creates an instance of RationalNumber of two IntegerNumbers
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        /// <returns>
        /// RationalNumber
        /// </returns>
        public static RationalNumber CreateRational(IntegerNumber numerator, IntegerNumber denominator)
            => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;

        /// <summary>
        /// Creates an instance of RealNumber from decimal
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// RealNumber
        /// </returns>
        public static RealNumber Create(decimal value)
            => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;

        /// <summary>
        /// Creates an instance of RealNumber from double
        /// </summary>
        /// <param name="value"></param>
        /// <returns>
        /// RealNumber
        /// </returns>
        public static RealNumber Create(double value)
            => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;

        /// <summary>
        /// Creates an instance of ComplexNumber from two RealNumbers
        /// </summary>
        /// <param name="re">
        /// Real part of a desired complex number
        /// </param>
        /// <param name="im">
        /// Imaginary part of a desired complex number
        /// </param>
        /// <returns>
        /// ComplexNumber
        /// </returns>
        public static ComplexNumber Create(RealNumber re, RealNumber im)
            => Number.Functional.Downcast(new ComplexNumber(re, im)) as ComplexNumber;

        /// <summary>
        /// If you need an indefinite value of a real number, use this
        /// Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY)
        /// Number.Create(RealNumber.UndefinedState.NEGATIVE_INFINITY)
        /// Number.Create(RealNumber.UndefinedState.NAN)
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public static RealNumber Create(RealNumber.UndefinedState state)
            => new RealNumber(state);

        /// <summary>
        /// If you need an indefinite value of a complex number, e. g.
        /// Number.Create(RealNumber.UndefinedState.POSITIVE_INFINITY, RealNumber.UndefinedState.NEGATIVE_INFINITY)
        /// -> +oo + -ooi
        /// </summary>
        /// <returns></returns>
        public static ComplexNumber Create(RealNumber.UndefinedState realState, RealNumber.UndefinedState imaginaryState)
            => Number.Create(Number.Create(realState), Number.Create(imaginaryState));

        /// <summary>
        /// Copies a Number with respect due to its hierarchy type, but without implicit downcasting
        /// </summary>
        /// <returns>
        /// Safely copied instance of Number
        /// </returns>
        public Number Copy()
            => Number.Copy(this);

        /// <summary>
        /// Checks whether a number is zero
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static bool IsZero(RealNumber num)
        {
            if (!num.IsDefinite())
                return false;
            return Functional.IsZero(num.Value);
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
            /// <summary>
            /// Can be only called if num is in type's set of numbers, e. g.
            /// we can upcast longeger to real, but we cannot upcast complex to real
            /// </summary>
            /// <param name="num">
            /// Number to upcast
            /// </param>
            /// <param name="type">
            /// The level the Number to upcast to
            /// </param>
            /// <returns></returns>
            internal static Number UpCastTo(Number num, Number.HierarchyLevel type)
            {
                if (num.Type == type)
                    return num;
                if (!num.Is(type))
                    throw new InvalidNumberCastException(num.Type, type);
                return Number.SuperSwitch(
                    num => new IntegerNumber(num[0] as Number),
                    num => new RationalNumber(num[0]),
                    num => new RealNumber(num[0] as Number),
                    num => new ComplexNumber(num[0]),
                    type,
                    num
                );
            }

            /// <summary>
            /// Upcasts both a and b to the same minimal level
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            internal static (Number a, Number b, HierarchyLevel type) MakeEqual(Number a, Number b)
            {
                var maxLevel = Math.Max((long)a.Type, (long)b.Type);
                var newType = (HierarchyLevel) maxLevel;
                return (UpCastTo(a, newType), UpCastTo(b, newType), newType);
            }

            /// <summary>
            /// If the difference between value & round(value) is zero (see Number.IsZero), we consider value as an integer
            /// </summary>
            /// <param name="value"></param>
            /// <param name="res"></param>
            /// <returns></returns>
            private static bool TryCastToInt(decimal value, out BigInteger res)
            {
                var intPart = Math.Round(value);
                var rest = value - intPart;
                if (Number.IsZero(rest))
                {
                    res = (BigInteger)intPart;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /// <summary>
            /// Performs so-called downcasting, an attempt to narrow a number's affiliation to a number set
            /// It attempts to cast ComplexNumber to RealNumber, RealNumber to RationalNumber, RationalNumber to IntegerNumber
            /// </summary>
            /// <param name="a">
            /// Number to downcast
            /// </param>
            /// <returns>
            /// Downcasted or kept Number
            /// </returns>
            public static Number Downcast(Number a)
            {
                if (!MathS.Settings.DowncastingEnabled)
                    return a;
                var res = SuperSwitch(
                    num => (Result: num[0], Continue: false),
                    (num) =>
                    {
                        if (!(a as RealNumber).IsDefinite())
                            return (Result: a, Continue: false);
                        var ratnum = num[0];
                        var gcd = Utils.GCD(ratnum.Denominator.Value, ratnum.Numerator.Value);
                        ratnum = new RationalNumber(
                            new IntegerNumber(ratnum.Numerator.Value / gcd),
                            new IntegerNumber(ratnum.Denominator.Value / gcd)
                        );
                        if (ratnum.Denominator.Value == 1)
                            return (Result: ratnum.Numerator, Continue: false);
                        else
                            return (Result: ratnum, Continue: false);
                    },
                    (num) =>
                    {
                        if (!(a as RealNumber).IsDefinite())
                            return (Result: a, Continue: false);
                        var realnum = num[0];

                        if (TryCastToInt(realnum.Value, out var intres))
                            return (Result: new IntegerNumber(intres), Continue: false);

                        var attempt = FindRational(realnum.Value, MathS.Settings.FloatToRationalIterCount);
                        if (attempt is null || 
                            Number.Abs(attempt.Numerator) > MathS.Settings.MaxAbsNumeratorOrDenominatorValue ||
                            Number.Abs(attempt.Denominator) > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                            return (Result: realnum, Continue: false);
                        else
                            return (Result: attempt, Continue: true);
                    },
                    (num) =>
                    {
                        var complnum = num[0];
                        if (complnum.Imaginary.IsDefinite() && Number.IsZero(complnum.Imaginary.Value)) // Momo's fix
                            return (Result: complnum.Real, Continue: true);
                        else
                        {
                            var reDowncasted = Downcast(complnum.Real) as RealNumber;
                            var imDowncasted = Downcast(complnum.Imaginary) as RealNumber;
                            return (Result: new ComplexNumber(reDowncasted, imDowncasted), Continue: false);
                        }
                    },
                    a.Type,
                    a
                    );
                if (res.Continue)
                    return Downcast(res.Result);
                else
                    return res.Result;
            }

            internal static bool IsZero(decimal value)
            {
                return Math.Abs(value) <= MathS.Settings.PrecisionErrorZeroRange;
            }

            /// <summary>
            /// Tries to find a pair of two IntegerNumbers (which is RationalNumber) so that its rational value is equal to num,
            /// to set some options for this function, you can use
            /// MathS.Settings.FloatToRationalIterCount.Set(20) to set a number of iterations allowed to be spent on searching for a rational
            /// and MathS.Settings.MaxAbsNumeratorOrDenominatorValue to limit the absolute value of both denominator and numerator
            /// </summary>
            /// <param name="num">
            /// e. g. 1.5m -> 3/2
            /// </param>
            /// <returns>
            /// RationalNumber if found,
            /// null otherwise
            /// </returns>
            public static RationalNumber FindRational(decimal num)
                => FindRational(num, 15);

            /// <summary>
            /// Tries to find a pair of two IntegerNumbers (which is RationalNumber) so that its rational value is equal to num,
            /// to set some options for this function, you can use
            /// MathS.Settings.MaxAbsNumeratorOrDenominatorValue to limit the absolute value of both denominator and numerator
            /// </summary>
            /// <param name="num">
            /// e. g. 1.5m -> 3/2
            /// </param>
            /// <param name="iterCount">
            /// number of iterations allowed to be spent for searching the rational, the more,
            /// the higher probability it will find a RationalNumber
            /// </param>
            /// <returns>
            /// RationalNumber if found,
            /// null otherwise
            /// </returns>
            public static RationalNumber FindRational(decimal num, int iterCount)
            {
                MathS.Settings.DowncastingEnabled.Set(false);
                var res = FindRational_(num, iterCount);
                MathS.Settings.DowncastingEnabled.Unset();
                return res;
            }

            public static RationalNumber FindRational_(decimal num, int iterCount)
            {
                if (iterCount <= 0)
                    return null;
                long sign = num > 0 ? 1 : -1;
                num *= sign;
                IntegerNumber intPart;
                intPart = (BigInteger) Math.Floor(num);
                if (intPart > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                    return null;
                decimal rest = num - (decimal)intPart.Value;
                if (Number.IsZero(rest))
                    return sign * intPart;
                else
                {
                    var inv = 1 / rest;
                    var rat = FindRational_(inv, iterCount - 1);
                    if (rat is null)
                        return null;
                    return intPart * sign + Number.Create(sign) / rat;
                }
            }

            internal static bool BothAreEqual(Number a, Number b, HierarchyLevel type)
            {
                return a.Type == type && b.Type == type;
            }

            public static ComplexNumber BinaryIntPow(ComplexNumber num, long val)
            {
                if (val == 0)
                    return 1;
                if (val == 1)
                    return num;
                if (val == -1)
                    return 1 / num;
                return BinaryIntPow(num, val / 2) * BinaryIntPow(num, val / 2) * BinaryIntPow(num, val % 2);
            }
        }
    }
}
