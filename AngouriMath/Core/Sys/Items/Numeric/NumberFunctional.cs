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
        public static RationalNumber CreateRational(int numerator, int denominator)
            => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;
        public static RationalNumber CreateRational(int numerator, long denominator)
            => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;
        public static RationalNumber CreateRational(long numerator, int denominator)
            => Number.Functional.Downcast(new RationalNumber(numerator, denominator)) as RationalNumber;
        public static RationalNumber CreateRational(long numerator, long denominator)
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
                        var attempt = FindRational(realnum.Value, MathS.Settings.FloatToRationalIterCount);
                        if (attempt is null || 
                            Math.Abs(attempt.Numerator) > MathS.Settings.MaxAbsNumeratorOrDenominatorValue ||
                            Math.Abs(attempt.Denominator) > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                            return (Result: realnum, Continue: false);
                        else
                            return (Result: attempt, Continue: true);
                    },
                    (num) =>
                    {
                        var complnum = num[0];
                        if (complnum.Imaginary.IsDefinite() && complnum.Imaginary.Value == 0)
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

            private static bool IsZero(decimal value)
            {
                return Math.Abs(value) < MathS.Settings.PrecisionError;
            }

            public static RationalNumber FindRational(decimal num)
                => FindRational(num, 15);

            public static RationalNumber FindRational(decimal num, int iterCount)
            {
                if (iterCount < 0)
                    return null;
                long sign = num > 0 ? 1 : -1;
                num *= sign;
                var intPart = (IntegerNumber)((long)Math.Floor(num));
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
