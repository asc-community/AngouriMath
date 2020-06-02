
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

namespace AngouriMath.Core.Numerix
{
    public partial class IntegerNumber : RationalNumber
    {
        public static IntegerNumber operator +(IntegerNumber a, IntegerNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.INTEGER))
                return Number.OpSum(a, b) as IntegerNumber;
            return new IntegerNumber(a.Value + b.Value);
        }

        public static IntegerNumber operator -(IntegerNumber a, IntegerNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.INTEGER))
                return Number.OpSub(a, b) as IntegerNumber;
            return new IntegerNumber(a.Value - b.Value);
        }

        public static IntegerNumber operator *(IntegerNumber a, IntegerNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.INTEGER))
                return Number.OpMul(a, b) as IntegerNumber;
            return new IntegerNumber(a.Value * b.Value);
        }

        public static RationalNumber operator /(IntegerNumber a, IntegerNumber b)
        {
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.INTEGER))
                return Number.OpDiv(a, b) as IntegerNumber;
            return Number.Functional.Downcast(new RationalNumber(a, b)) as RationalNumber;
        }
        public static implicit operator int(IntegerNumber val)
            => (int)val.Value;
        public static implicit operator long(IntegerNumber val)
            => (long)val.Value;
        public static implicit operator BigInteger(IntegerNumber val)
            => val.Value;

        public static bool AreEqual(IntegerNumber a, IntegerNumber b)
            => a.Value == b.Value;

        public static IntegerNumber operator -(IntegerNumber a)
            => (-1 * a).AsIntegerNumber();

        public static implicit operator IntegerNumber(int num)
            => new IntegerNumber((BigInteger)num);

        public static implicit operator IntegerNumber(long num)
            => new IntegerNumber((BigInteger)num);
        public static implicit operator IntegerNumber(BigInteger num)
            => new IntegerNumber(num);
    }
}
