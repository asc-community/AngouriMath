using System;
using System.Collections.Generic;
using System.Text;

namespace AngouriMath.Core.Numerix
{
    public partial class RationalNumber : RealNumber
    {
        public static RationalNumber operator +(RationalNumber a, RationalNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.RATIONAL))
                return Number.OpSum(a, b) as RationalNumber;
            var num = a.Numerator * b.Denominator + b.Numerator * a.Denominator;
            var den = a.Denominator * b.Denominator;
            return Number.Functional.Downcast(new RationalNumber(num, den)) as RationalNumber;
        }

        public static RationalNumber operator -(RationalNumber a, RationalNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.RATIONAL))
                return Number.OpSub(a, b) as RationalNumber;
            var num = a.Numerator * b.Denominator - b.Numerator * a.Denominator;
            var den = a.Denominator * b.Denominator;
            return Number.Functional.Downcast(new RationalNumber(num, den)) as RationalNumber;
        }

        public static RationalNumber operator *(RationalNumber a, RationalNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.RATIONAL))
                return Number.OpMul(a, b) as RationalNumber;
            var num = a.Numerator * b.Numerator;
            var den = a.Denominator * b.Denominator;
            return Number.Functional.Downcast(new RationalNumber(num, den)) as RationalNumber;
        }

        public static RationalNumber operator /(RationalNumber a, RationalNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.RATIONAL))
                return Number.OpDiv(a, b) as RationalNumber;
            var num = a.Numerator * b.Denominator;
            var den = a.Denominator * b.Numerator;
            return Number.Functional.Downcast(new RationalNumber(num, den)) as RationalNumber;
        }

        internal static bool AreEqual(RationalNumber a, RationalNumber b)
            => a.Numerator == b.Numerator && a.Denominator == b.Denominator && a.State == b.State;

        public static RationalNumber operator -(RationalNumber a)
            => (-1 * a).AsRationalNumber();
    }
}
