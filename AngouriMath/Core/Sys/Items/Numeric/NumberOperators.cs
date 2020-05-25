using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace AngouriMath.Core.Numeric
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

        public static bool operator ==(Number a, Number b)
        {
            if (a is null && b is null)
                return true;
            if (a is null || b is null)
                return false;
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
                num => num[0] == num[1],
                num => num[0] == num[1],
                num => num[0] == num[1],
                num => num[0] == num[1],
                a.Type,
                a, b
            );
        }

        public static bool operator !=(Number a, Number b)
            => !(a == b);

        /// <summary>
        /// e. g. Pow(2, 5) = 32
        /// </summary>
        /// <param name="base"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static Number Pow(Number @base, Number power)
        {
            // TODO: make it more detailed (e. g. +oo ^ +oo = +oo)
            var baseCom = @base.AsComplexNumber();
            var powerCom = power.AsComplexNumber();
            if (baseCom.IsDefinite() && powerCom.IsDefinite())
                return Functional.Downcast(
                    Complex.Pow(baseCom.AsComplex(), powerCom.AsComplex())
                );
            else
                return ComplexNumber.Indefinite(RealNumber.UndefinedState.NAN);
        }

        /// <summary>
        /// e. g. Log(2, 32) = 5
        /// </summary>
        /// <param name="base"></param>
        /// <param name="powered"></param>
        /// <returns></returns>
        public static Number Log(RealNumber @base, Number powered)
        {
            var baseCom = @base.AsComplexNumber();
            var poweredCom = powered.AsComplexNumber();
            if (baseCom.IsDefinite() && poweredCom.IsDefinite())
                return Functional.Downcast(
                Complex.Log(powered.AsDouble(), @base.AsDouble())
            );
            else
                return ComplexNumber.Indefinite(RealNumber.UndefinedState.NAN);
        }

        public static Number Sin(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Sin(num.AsComplex()))
                : RealNumber.NaN();

        public static Number Cos(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Cos(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Tan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Tan(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Cotan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(1 / Complex.Tan(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Arcsin(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Asin(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Arccos(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Acos(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Arctan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Atan(num.AsComplex()))
                : RealNumber.NaN();
        public static Number Arccotan(Number num)
            => (num as ComplexNumber).IsDefinite()
                ? Functional.Downcast(Complex.Atan(1 / num.AsComplex()))
                : RealNumber.NaN();

    }
}
