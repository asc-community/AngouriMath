
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
                    () => RealNumber.NaN(),
                    () => RealNumber.NaN(),
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
        public static implicit operator RealNumber(double value)
            => new RealNumber(value);
        public static implicit operator RealNumber(decimal value)
            => new RealNumber(value);
        public static implicit operator RealNumber(int value)
            => new RealNumber(value);
        public static implicit operator RealNumber(long value)
            => new RealNumber(value);
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
        {
            if (a.State == UndefinedState.NAN || b.State == UndefinedState.NAN)
                return true; // if something is undefined, a < b is undefined, hence true
            return !(a >= b);
        }

        public static bool operator <=(RealNumber a, RealNumber b)
            => a < b || a == b;

        internal static bool AreEqual(RealNumber a, RealNumber b)
            => a.IsDefinite() && b.IsDefinite() && Math.Abs(a.Value - b.Value) < MathS.Settings.PrecisionErrorCommon ||
               a.State == b.State && !a.IsDefinite();

        public static RealNumber operator -(RealNumber a)
            => (-1 * a).AsRealNumber();
    }
}


namespace AngouriMath.Core.Numerix
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