
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
