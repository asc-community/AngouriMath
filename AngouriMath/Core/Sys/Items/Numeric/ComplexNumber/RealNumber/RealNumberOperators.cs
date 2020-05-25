using System;

namespace AngouriMath.Core.Numeric
{
    public partial class RealNumber
    {
        public static RealNumber operator +(RealNumber a, RealNumber b)
        {
            if (!a.IsDefinite() || !b.IsDefinite())
                return UndefinedStateSuperSwitch.Switch(
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    a, b);
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.REAL))
                return Number.OpSum(a, b) as RealNumber;
            return Number.Functional.Downcast(new RealNumber(a.Value + b.Value)) as RealNumber;
        }

        public static RealNumber operator -(RealNumber a, RealNumber b)
        {
            if (!a.IsDefinite() || !b.IsDefinite())
                return UndefinedStateSuperSwitch.Switch(
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    a, b);
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.REAL))
                return Number.OpSub(a, b) as RealNumber;
            return Number.Functional.Downcast(new RealNumber(a.Value - b.Value)) as RealNumber;
        }

        public static RealNumber operator *(RealNumber a, RealNumber b)
        {
            if (!a.IsDefinite() || !b.IsDefinite())
                return UndefinedStateSuperSwitch.Switch(
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => b.Value > 0 ? new RealNumber(UndefinedState.POSITIVE_INFINITY) : new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => b.Value > 0 ? new RealNumber(UndefinedState.NEGATIVE_INFINITY) : new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => a.Value > 0 ? new RealNumber(UndefinedState.POSITIVE_INFINITY) : new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => a.Value > 0 ? new RealNumber(UndefinedState.NEGATIVE_INFINITY) : new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    a, b);
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.REAL))
                return Number.OpMul(a, b) as RealNumber;
            return Number.Functional.Downcast(new RealNumber(a.Value * b.Value)) as RealNumber;
        }

        public static RealNumber operator /(RealNumber a, RealNumber b)
        {
            if (!a.IsDefinite() || !b.IsDefinite())
                return UndefinedStateSuperSwitch.Switch(
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NAN),
                    () => new RealNumber(UndefinedState.NAN),
                    () => b.Value >= 0 ? new RealNumber(UndefinedState.POSITIVE_INFINITY) : new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    () => b.Value >= 0 ? new RealNumber(UndefinedState.NEGATIVE_INFINITY) : new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    () => new IntegerNumber(0), 
                    () => new IntegerNumber(0),
                    a, b);
            if (!Functional.BothAreEqual(a, b, HierarchyLevel.REAL))
                return Number.OpDiv(a, b) as RealNumber;
            if (b.Value != 0)
                return Number.Functional.Downcast(new RealNumber(a.Value / b.Value)) as RealNumber;
            else
                return a.Value switch
                {
                    var x when x > 0 => new RealNumber(UndefinedState.POSITIVE_INFINITY),
                    var x when x < 0 => new RealNumber(UndefinedState.NEGATIVE_INFINITY),
                    _ => new RealNumber(UndefinedState.NAN)
                };
        }

        public static implicit operator float(RealNumber value)
            => (float) value.Value;
        public static implicit operator double(RealNumber value)
            => (double)value.Value;
        public static implicit operator decimal(RealNumber value)
            => value.Value;

        public static bool operator >(RealNumber a, RealNumber b)
        {
            if (a.State == UndefinedState.NAN || b.State == UndefinedState.NAN)
                return true; // if something is undefined, a > b is undefined, hence true
            if (a.State == UndefinedState.NEGATIVE_INFINITY)
                return false; // -oo is never greater than anything
            if (a.State == UndefinedState.POSITIVE_INFINITY)
                return true; // +oo is always greater than anything
            if (b.State == UndefinedState.NEGATIVE_INFINITY)
                return true; // anything is greater than -oo
            if (b.State == UndefinedState.POSITIVE_INFINITY)
                return false; // anything is never greater than +oo
            return a.Value > b.Value;
        }

        public static bool operator >=(RealNumber a, RealNumber b)
            => a > b || a == b;

        public static bool operator <(RealNumber a, RealNumber b)
            => !(a >= b);

        public static bool operator <=(RealNumber a, RealNumber b)
            => !(a > b);

        public static bool operator ==(RealNumber a, RealNumber b)
            => a.Value == b.Value && a.State == b.State;

        public static bool operator !=(RealNumber a, RealNumber b)
            => !(a == b);

        public static RealNumber operator -(RealNumber a)
            => (-1 * a).AsRealNumber();
    }
}


namespace AngouriMath.Core.Numeric
{
    public partial class RealNumber
    {
        private static class UndefinedStateSuperSwitch
        {
            internal delegate RealNumber Operation();
            internal static RealNumber Switch(
                Operation neg__neg,
                Operation pos__pos,
                Operation neg__pos,
                Operation pos__neg,
                Operation pos_,
                Operation neg_,
                Operation _pos,
                Operation _neg,
                RealNumber a, RealNumber b
            )
            {
                // NaN + [] = NaN
                if (a.State == UndefinedState.NAN || b.State == UndefinedState.NAN)
                    return new RealNumber(UndefinedState.NAN);
                // -oo ? -oo
                if (a.State == UndefinedState.NEGATIVE_INFINITY && b.State == UndefinedState.NEGATIVE_INFINITY)
                    return neg__neg();
                // +oo ? +oo
                if (a.State == UndefinedState.POSITIVE_INFINITY && b.State == UndefinedState.POSITIVE_INFINITY)
                    return pos__pos();
                // -oo ? +oo
                if (a.State == UndefinedState.NEGATIVE_INFINITY && b.State == UndefinedState.POSITIVE_INFINITY)
                    return neg__pos();
                // +oo ? -oo
                if (a.State == UndefinedState.POSITIVE_INFINITY && b.State == UndefinedState.NEGATIVE_INFINITY)
                    return pos__neg();
                // ELSE
                // +oo ? []
                if (a.State == UndefinedState.POSITIVE_INFINITY)
                    return pos_();
                // -oo ? []
                if (a.State == UndefinedState.NEGATIVE_INFINITY)
                    return neg_();
                // [] ? +oo
                if (b.State == UndefinedState.POSITIVE_INFINITY)
                    return _pos();
                // [] ? -oo
                if (b.State == UndefinedState.NEGATIVE_INFINITY)
                    return _neg();
                throw new NotSupportedException("Expected all cases to be considered");
            }
        }
    }
}