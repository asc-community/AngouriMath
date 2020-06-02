

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
using AngouriMath.Functions;

namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number
    {
        public static ComplexNumber Create(Complex value)
            => Number.Functional.Downcast(new ComplexNumber(value)) as ComplexNumber;
        public static IntegerNumber Create(long value)
            => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;
        public static IntegerNumber Create(BigInteger value)
            => Number.Functional.Downcast(new IntegerNumber(value)) as IntegerNumber;
        public static IntegerNumber Create(int value) 
            => Number.Functional.Downcast(new IntegerNumber((BigInteger)value)) as IntegerNumber;
        public static RationalNumber CreateRational(IntegerNumber numerator, IntegerNumber denominator)
            => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;
        public static RealNumber Create(decimal value)
            => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;
        public static RealNumber Create(double value)
            => Number.Functional.Downcast(new RealNumber(value)) as RealNumber;
        public static ComplexNumber Create(RealNumber re, RealNumber im)
            => Number.Functional.Downcast(new ComplexNumber(re, im)) as ComplexNumber;
        public Number Copy()
            => SuperSwitch(
                num => new IntegerNumber(num[0] as Number),
                num => new RationalNumber(num[0]),
                num => new RealNumber(num[0] as Number),
                num => new ComplexNumber(num[0]),
                Type,
                this
            );

        public static bool IsZero(RealNumber num)
        {
            if (!num.IsDefinite())
                return false;
            return Functional.IsZero(num.Value);
        }

        public static bool IsZero(ComplexNumber num)
            => IsZero(num.Real) && IsZero(num.Imaginary);

        public static class Functional
        {
            /// <summary>
            /// Can be only called if num is in type's set of numbers, e. g.
            /// we can upcast longeger to real, but we cannot upcast complex to real
            /// </summary>
            /// <param name="num"></param>
            /// <param name="type"></param>
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

            internal static (Number a, Number b, HierarchyLevel type) MakeEqual(Number a, Number b)
            {
                var maxLevel = Math.Max((long)a.Type, (long)b.Type);
                var newType = (HierarchyLevel) maxLevel;
                return (UpCastTo(a, newType), UpCastTo(b, newType), newType);
            }

            private static bool TryCastToInt(decimal value, out BigInteger res)
            {
                var intPart = Math.Round(value);
                var rest = value - intPart;
                if (IsZero(rest))
                {
                    res = (BigInteger)intPart;
                    return true;
                }
                else
                {
                    return false;
                }
            }

            internal static Number Downcast(Number a)
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
                            return (Result: complnum, Continue: false);
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

            public static RationalNumber FindRational(decimal num)
                => FindRational(num, 15);

            public static RationalNumber FindRational(decimal num, int iterCount)
            {
                if (iterCount <= 0)
                    return null;
                long sign = num > 0 ? 1 : -1;
                num *= sign;
                var intPart = (IntegerNumber)((BigInteger)Math.Floor(num));
                if (intPart > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                    return null;
                decimal rest = num - (decimal)intPart.Value;
                if (IsZero(rest))
                    return (sign * intPart).AsRationalNumber();
                else
                {
                    var inv = 1 / rest;
                    var rat = FindRational(inv, iterCount - 1);
                    if (rat is null)
                        return null;
                    return intPart * sign + Number.Create(sign) / rat;
                }
            }

            public static bool BothAreEqual(Number a, Number b, HierarchyLevel type)
            {
                return a.Type == type && b.Type == type;
            }
        }
    }
}
