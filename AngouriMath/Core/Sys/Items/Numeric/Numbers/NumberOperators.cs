using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;


namespace AngouriMath.Core
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
        => Functional.Downcast(
            Complex.Pow(@base.AsComplex(), power.AsComplex())
            );

        /// <summary>
        /// e. g. Log(2, 32) = 5
        /// </summary>
        /// <param name="base"></param>
        /// <param name="powered"></param>
        /// <returns></returns>
        public static Number Log(RealNumber @base, Number powered)
            => Functional.Downcast(
                Complex.Log(powered.AsDouble(), @base.AsDouble())
            );

        public static Number Sin(Number num)
            => Functional.Downcast(Complex.Sin(num.AsComplex()));
        public static Number Cos(Number num)
            => Functional.Downcast(Complex.Cos(num.AsComplex()));
        public static Number Tan(Number num)
            => Functional.Downcast(Complex.Tan(num.AsComplex()));
        public static Number Cotan(Number num)
            => Functional.Downcast(1 / Complex.Tan(num.AsComplex()));
        public static Number Arcsin(Number num)
            => Functional.Downcast(Complex.Asin(num.AsComplex()));
        public static Number Arccos(Number num)
            => Functional.Downcast(Complex.Acos(num.AsComplex()));
        public static Number Arctan(Number num)
            => Functional.Downcast(Complex.Atan(num.AsComplex()));
        public static Number Arccotan(Number num)
            => Functional.Downcast(Complex.Atan(1 / num.AsComplex()));

    }
}
