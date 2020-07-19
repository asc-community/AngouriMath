
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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PeterO.Numbers;

namespace AngouriMath.Core.Numerix
{
    public abstract partial class Number : IEquatable<Number>
    {
        internal static EInteger CtxAdd(EInteger a, EInteger b) => a.Add(b);
        internal static EInteger CtxSubtract(EInteger a, EInteger b) => a.Subtract(b);
        internal static EInteger CtxMultiply(EInteger a, EInteger b) => a.Multiply(b);
        internal static ERational CtxDivide(EInteger a, EInteger b) =>
            b.IsZero ? ERational.NaN : new ERational(a, b);
        internal static EInteger CtxMod(EInteger a, EInteger b) => a.Remainder(b);
        internal static EInteger CtxPow(EInteger a, EInteger b) => a.Pow(b);
        internal static ERational CtxAdd(ERational a, ERational b) => a.Add(b);
        internal static ERational CtxSubtract(ERational a, ERational b) => a.Subtract(b);
        internal static ERational CtxMultiply(ERational a, ERational b) => a.Multiply(b);
        internal static ERational CtxDivide(ERational a, ERational b) =>
            b.IsZero ? ERational.NaN : a.Divide(b);
        internal static ERational CtxMod(ERational a, ERational b) => a.Remainder(b);
        internal static EDecimal CtxAdd(EDecimal a, EDecimal b)
            => a.Add(b, MathS.Settings.DecimalPrecisionContext);
        internal static EDecimal CtxSubtract(EDecimal a, EDecimal b)
            => a.Subtract(b, MathS.Settings.DecimalPrecisionContext);
        internal static EDecimal CtxMultiply(EDecimal a, EDecimal b)
            => a.Multiply(b, MathS.Settings.DecimalPrecisionContext);
        internal static EDecimal CtxDivide(EDecimal a, EDecimal b)
            => a.DivideToExponent(b, -MathS.Settings.DecimalPrecisionContext.Value.Precision);
        internal static EDecimal CtxMod(EDecimal a, EDecimal b)
            => a.RemainderNoRoundAfterDivide(b, MathS.Settings.DecimalPrecisionContext);
        internal static EDecimal CtxPow(EDecimal a, EDecimal b)
            => a.Pow(b, MathS.Settings.DecimalPrecisionContext);

        public static RealNumber Max(params RealNumber[] nums)
            => nums.Length == 1 ? nums[0] : InternalMax(nums[0], Max(new ArraySegment<RealNumber>(nums, 1, nums.Length - 1).ToArray()));

        public static RealNumber Min(params RealNumber[] nums)
            => nums.Length == 1 ? nums[0] : InternalMin(nums[0], Min(new ArraySegment<RealNumber>(nums, 1, nums.Length - 1).ToArray()));

        private static RealNumber InternalMax(RealNumber a, RealNumber b)
            => a > b ? a : b;

        private static RealNumber InternalMin(RealNumber a, RealNumber b)
            => a < b ? a : b;

        internal static T OpSum<T>(T a, T b) where T : Number =>
            SuperSwitch(a, b,
                (a, b) => IntegerNumber.Create(CtxAdd(a.Value, b.Value)),
                (a, b) => RationalNumber.Create(CtxAdd(a.Value, b.Value)),
                (a, b) => RealNumber.Create(CtxAdd(a.Value, b.Value)),
                (a, b) => ComplexNumber.Create(CtxAdd(a.Real.Value, b.Real.Value), CtxAdd(a.Imaginary.Value, b.Imaginary.Value))
             );
        internal static T OpSub<T>(T a, T b) where T : Number =>
            SuperSwitch(a, b,
                (a, b) => IntegerNumber.Create(CtxSubtract(a.Value, b.Value)),
                (a, b) => RationalNumber.Create(CtxSubtract(a.Value, b.Value)),
                (a, b) => RealNumber.Create(CtxSubtract(a.Value, b.Value)),
                (a, b) => ComplexNumber.Create(CtxSubtract(a.Real.Value, b.Real.Value), CtxSubtract(a.Imaginary.Value, b.Imaginary.Value))
             );
        internal static T OpMul<T>(T a, T b) where T : Number =>
            SuperSwitch(a, b,
                (a, b) => IntegerNumber.Create(CtxMultiply(a.Value, b.Value)),
                (a, b) => RationalNumber.Create(CtxMultiply(a.Value, b.Value)),
                (a, b) => RealNumber.Create(CtxMultiply(a.Value, b.Value)),
                (a, b) =>
                {
                    // Define oo * i -> oo i and i * oo -> oo i
                    static EDecimal ModifiedMultiply(EDecimal a, EDecimal b) =>
                        a.IsInfinity() && b.IsZero ? EDecimal.Zero
                        : b.IsInfinity() && a.IsZero ? EDecimal.Zero
                        : CtxMultiply(a, b);
                    return ComplexNumber.Create(
                        CtxSubtract(ModifiedMultiply(a.Real.Value, b.Real.Value), ModifiedMultiply(a.Imaginary.Value, b.Imaginary.Value)),
                        CtxAdd(ModifiedMultiply(a.Real.Value, b.Imaginary.Value), ModifiedMultiply(a.Imaginary.Value, b.Real.Value)));
                }
             );
        internal static ComplexNumber OpDiv<T>(T a, T b) where T : Number =>
            SuperSwitch(a, b,
                (a, b) => RationalNumber.Create(CtxDivide(a.Value, b.Value)),
                (a, b) => RationalNumber.Create(CtxDivide(a.Value, b.Value)),
                (a, b) => RealNumber.Create(CtxDivide(a.Value, b.Value)),
                (a, b) =>
                {
                    /*
                     * (a + ib) / (c + id) = (a + ib) * (1 / (c + id))
                     * 1 / (c + id) = (c2 + d2) / (c + id) / (c2 + d2) = (c - id) / (c2 + d2)
                     * => ans = (a + ib) * (c - id) / (c2 + d2)
                     */
                    var conj = b.Conjugate();
                    var bAbs = b.Abs().Value;
                    var abs2 = CtxMultiply(bAbs, bAbs);
                    var Re = CtxDivide(conj.Real.Value, abs2);
                    var Im = CtxDivide(conj.Imaginary.Value, abs2);
                    var c = ComplexNumber.Create(Re, Im);
                    return a * c;
                }
             );
        internal static bool AreEqual<T>(T a, T b) where T : Number =>
            SuperSwitch(a, b,
                (a, b) => a.Value.Equals(b.Value),
                (a, b) => a.Value.Equals(b.Value),
                (a, b) => a.IsFinite && b.IsFinite && IsZero(CtxSubtract(a.Value, b.Value)) ||
                         !a.IsFinite && !b.IsFinite && a.Value.Equals(b.Value),
                (a, b) => AreEqual(a.Real, b.Real) && AreEqual(a.Imaginary, b.Imaginary)
             );
        public override bool Equals(object other) => other is Number n && Equals(n);
        public bool Equals(Number n) => AreEqual(this, n);
        public abstract override int GetHashCode();
        public static Number operator +(Number a, Number b) => OpSum(a, b);
        public static Number operator -(Number a, Number b) => OpSub(a, b);
        public static Number operator *(Number a, Number b) => OpMul(a, b);
        public static Number operator /(Number a, Number b) => OpDiv(a, b);
        public static Number operator +(Number a) => a;
        public static Number operator -(Number a) => OpMul(-1, a);
        public static bool operator ==(Number a, Number b) => AreEqual(a, b);
        public static bool operator !=(Number a, Number b) => !AreEqual(a, b);

        internal static ComplexNumber FindGoodRoot(ComplexNumber @base, IntegerNumber power)
        {
            var list = new List<ComplexNumber>();
            foreach (NumberEntity root in GetAllRoots(@base, power.Value).FiniteSet())
            {
                var downcasted = MathS.Settings.FloatToRationalIterCount.As(15, () =>
                    MathS.Settings.PrecisionErrorZeroRange.As(1e-6m, () =>
                    {
                        return ComplexNumber.Create(root.Value.Real, root.Value.Imaginary);
                    }));
                if (downcasted is RationalNumber && IsZero(Pow(downcasted, power) - @base)) // To keep user's desired precision
                    return downcasted;
                list.Add(downcasted);
            }
            foreach (var el in list)
                if (el is RealNumber r && r > 0)
                    return el;
            foreach (var el in list)
                if (el is RealNumber)
                    return el;
            return list[0];
        }

        /// <summary>
        /// e. g. Pow(2, 5) = 32
        /// </summary>
        /// <param name="base">
        /// The base of the exponential, base^power
        /// </param>
        /// <param name="power">
        /// The power of the exponential, base^power
        /// </param>
        /// <returns></returns>
        public static ComplexNumber Pow(ComplexNumber @base, ComplexNumber power)
        {
            // TODO: make it more detailed (e. g. +oo ^ +oo = +oo)
            if (power is IntegerNumber { Value: var pow })
                return Functional.BinaryIntPow(@base, pow);

            if (power is RationalNumber r && r.Value.Denominator.Abs() < 10) // there should be a minimal threshold to avoid long searches 
                return Pow(FindGoodRoot(@base, r.Value.Denominator), r.Value.Numerator);

            if (@base.IsFinite && power.IsFinite)
            {
                try
                {
                    return Complex.Pow(@base.AsComplex(), power.AsComplex());
                }
                catch (OverflowException)
                {
                    return RealNumber.NaN;
                }
            }
            else
                return RealNumber.NaN;
        }

        /// <summary>
        /// e. g. Log(2, 32) = 5
        /// </summary>
        /// <param name="base">
        /// Log's base, log(base, x) is a number y such that base^y = x
        /// </param>
        /// <param name="x">
        /// The number of which we want to get its base power
        /// </param>
        /// <returns></returns>
        public static ComplexNumber Log(RealNumber @base, ComplexNumber x) =>
            @base.IsFinite && x.IsFinite
            ? Complex.Log(x.AsComplex(), @base.AsDouble())
            : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of sine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Sin(ComplexNumber num)
            =>  num.IsFinite
                ? Complex.Sin(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of cosine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Cos(ComplexNumber num)
            =>  num.IsFinite
                ? Complex.Cos(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of tangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Tan(ComplexNumber num)
            => num.IsFinite
                ? Complex.Tan(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of cotangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Cotan(ComplexNumber num)
            => num.IsFinite
                ? 1 / Complex.Tan(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of arcsine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arcsin(ComplexNumber num)
            => num.IsFinite
                ? Complex.Asin(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of arccosine of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arccos(ComplexNumber num)
            => num.IsFinite
                ? Complex.Acos(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of arctangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arctan(ComplexNumber num)
            => num.IsFinite
                ? Complex.Atan(num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;

        /// <summary>
        /// Calculates the exact value of arccotangent of num
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        public static ComplexNumber Arccotan(ComplexNumber num)
            => num.IsFinite
                ? Complex.Atan(1 / num.AsComplex())
                : (ComplexNumber)RealNumber.NaN;
    }
}
