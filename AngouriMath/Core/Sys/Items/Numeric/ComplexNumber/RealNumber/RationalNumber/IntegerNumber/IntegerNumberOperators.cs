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
    }
}
