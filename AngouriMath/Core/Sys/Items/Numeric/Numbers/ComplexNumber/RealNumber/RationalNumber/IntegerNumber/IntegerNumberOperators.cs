using System;

namespace AngouriMath.Core
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
            => val.Value;

        public static bool operator ==(IntegerNumber a, IntegerNumber b)
            => a.Value == b.Value;

        public static bool operator !=(IntegerNumber a, IntegerNumber b)
            => !(a == b);

        public static IntegerNumber operator -(IntegerNumber a)
            => (-1 * a).AsIntegerNumber();
    }
}
