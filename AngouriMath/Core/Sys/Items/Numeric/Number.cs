
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
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using AngouriMath.Extensions;
using PeterO.Numbers;

namespace AngouriMath.Core.Numerix
{
    /// <summary>
    /// This class represents all possible numerical values as a hierarchy,
    /// <list>
    ///   Number
    ///   <list type="bullet">
    ///     ComplexNumber
    ///       <list type="bullet">
    ///         RealNumber
    ///         <list type="bullet">
    ///           RationalNumber
    ///           <list type="bullet">
    ///             IntegerNumber
    ///           </list>
    ///         </list>
    ///       </list>
    ///     </list>
    ///   </list>
    /// </summary>
    public abstract partial class Number : Sys.Interfaces.ILatexiseable
    {
        /// <summary>
        /// The final value. Only useful for calculations
        /// </summary>
        public (EDecimal Re, EDecimal Im) Value => GetValue();

        protected abstract (EDecimal Re, EDecimal Im) GetValue();

        /// <summary>
        /// This function serves not only convenience but also protects from unexpected cases, for example,
        /// if a new type added
        /// </summary>
        protected static T SuperSwitch<T>(
            Number num,
            Func<IntegerNumber, T> ifInt,
            Func<RationalNumber, T> ifRat,
            Func<RealNumber, T> ifReal,
            Func<ComplexNumber, T> ifCom
        )
            => num switch
            {
                IntegerNumber n => ifInt(n),
                RationalNumber r => ifRat(r),
                RealNumber r => ifReal(r),
                ComplexNumber c => ifCom(c),
                _ => throw new NotSupportedException(num.GetType() + " is not supported.")
            };
        /// <summary>
        /// This function serves not only convenience but also protects from unexpected cases, for example,
        /// if a new type added
        /// </summary>
        protected static T SuperSwitch<T>(
            T num,
            Func<IntegerNumber, IntegerNumber> ifInt,
            Func<RationalNumber, RationalNumber> ifRat,
            Func<RealNumber, RealNumber> ifReal,
            Func<ComplexNumber, ComplexNumber> ifCom
        ) where T : Number
            => (T)(Number)(num switch
            {
                IntegerNumber n => ifInt(n),
                RationalNumber r => ifRat(r),
                RealNumber r => ifReal(r),
                ComplexNumber c => ifCom(c),
                _ => throw new NotSupportedException(num.GetType() + " is not supported.")
            });
        /// <summary>
        /// This function serves not only convenience but also protects from unexpected cases, for example,
        /// if a new type added
        /// </summary>
        protected static T SuperSwitch<T>(
            Number num1, Number num2,
            Func<IntegerNumber, IntegerNumber, T> ifInt,
            Func<RationalNumber, RationalNumber, T> ifRat,
            Func<RealNumber, RealNumber, T> ifReal,
            Func<ComplexNumber, ComplexNumber, T> ifCom
        ) => (num1, num2) switch
            {
                (IntegerNumber n1, IntegerNumber n2) => ifInt(n1, n2),
                (RationalNumber r1, RationalNumber r2) => ifRat(r1, r2),
                (RealNumber r1, RealNumber r2) => ifReal(r1, r2),
                (ComplexNumber c1, ComplexNumber c2) => ifCom(c1, c2),
                _ => throw new NotSupportedException($"({num1.GetType()}, {num2.GetType()}) is not supported.")
            };
        /// <summary>
        /// This function serves not only convenience but also protects from unexpected cases, for example,
        /// if a new type added
        /// </summary>
        protected static T SuperSwitch<T>(
            T num1, T num2,
            Func<IntegerNumber, IntegerNumber, IntegerNumber> ifInt,
            Func<RationalNumber, RationalNumber, RationalNumber> ifRat,
            Func<RealNumber, RealNumber, RealNumber> ifReal,
            Func<ComplexNumber, ComplexNumber, ComplexNumber> ifCom
        ) where T : Number
            => (T)(Number)((num1, num2) switch
            {
                (IntegerNumber n1, IntegerNumber n2) => ifInt(n1, n2),
                (RationalNumber r1, RationalNumber r2) => ifRat(r1, r2),
                (RealNumber r1, RealNumber r2) => ifReal(r1, r2),
                (ComplexNumber c1, ComplexNumber c2) => ifCom(c1, c2),
                _ => throw new NotSupportedException($"({num1.GetType()}, {num2.GetType()}) is not supported.")
            });

        /// <summary>
        /// Gets a latexised version of a number
        /// </summary>
        /// <returns></returns>
        public string Latexise() => Latexise(false);

        internal protected abstract string InternalLatexise();
        internal string Latexise(bool needParentheses) =>
            // If parentheses are required, they might be only required when complicated numbers are wrapped,
            // such as fractions and complex but not a single i
            needParentheses
                  && this.Value != (EDecimal.Zero, EDecimal.One)
                  && (this is RationalNumber || (this is ComplexNumber && !(this is RealNumber)))
                ? @$"\left({InternalLatexise()}\right)"
                : InternalLatexise();

        public override string ToString()
            => ToString(false);

        internal protected abstract string InternalToString();
        internal string ToString(bool needParentheses) =>
            needParentheses
            && (this is RationalNumber || (this is ComplexNumber && !(this is RealNumber)))
            ? $"({InternalToString()})"
            : InternalToString();

        /// <summary>
        /// Finds all complex roots of a number
        /// e. g. sqrt(1) = { -1, 1 }
        /// root(1, 4) = { -i, i, -1, 1 }
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rootPower"></param>
        /// <returns></returns>
        public static Set GetAllRoots(ComplexNumber value, EInteger rootPower)
        {
            // Avoid infinite recursion from Abs to GetAllRoots again
            MathS.Settings.FloatToRationalIterCount.Set(0);
            var res = new Set();
            EDecimal phi = (Log(MathS.DecimalConst.e, value / value.Abs()) / MathS.i).Real.Value;
            if (phi.IsNaN()) // (value / value.Abs()) is NaN when value is zero
                phi = EDecimal.Zero;

            EDecimal newMod = Pow(Abs(value), CtxDivide(EDecimal.One, rootPower)).Real.Value;

            var i = ComplexNumber.ImaginaryOne;
            for (int n = 0; n < rootPower; n++)
            {
                EDecimal newPow = CtxAdd(CtxDivide(phi, rootPower), CtxDivide(CtxMultiply(CtxMultiply(2, MathS.DecimalConst.pi), n), rootPower));
                var root = newMod.ToNumber() * Pow(MathS.DecimalConst.e, i * newPow.ToNumber());
                res.Add(root);
            }
            MathS.Settings.FloatToRationalIterCount.Unset();
            return res;
        }

        public static Set GetAllRootsOf1(EInteger rootPower)
        {
            var res = new Set();
            res.FastAddingMode = true;
            for (int i = 0; i < rootPower; i++)
            {
                var angle = RationalNumber.Create(i * 2, rootPower) * MathS.pi;
                res.Add((MathS.Cos(angle) + MathS.i * MathS.Sin(angle)).InnerSimplify());
            }
            res.FastAddingMode = false;
            return res;
        }

        /// <summary>
        /// Returns the absolute value of a complex number num, to be precise,
        /// if num = a + ib, num.Abs() -> sqrt(a^2 + b^2)
        /// </summary>
        /// <param name="num">
        /// RealNumber
        /// </param>
        /// <returns></returns>
        public static RealNumber Abs(ComplexNumber num)
            => num.Abs();

        /// <summary>
        /// Forcefully casts one to Complex with downcasting
        /// </summary>
        /// <returns></returns>
        public Complex AsComplex() =>
            this is ComplexNumber c
            ? new Complex(c.Real.Value.ToDouble(), c.Imaginary.Value.ToDouble())
            : throw new InvalidNumberCastException(GetType(), typeof(ComplexNumber));

        /// <summary>
        /// Forcefully casts one to double with downcasting
        /// </summary>
        /// <returns></returns>
        public double AsDouble() =>
            this is RealNumber r
            ? r.Value.ToDouble()
            : throw new InvalidNumberCastException(GetType(), typeof(RealNumber));
        public static implicit operator Number(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator Number(byte value) => IntegerNumber.Create(value);
        public static implicit operator Number(short value) => IntegerNumber.Create(value);
        public static implicit operator Number(ushort value) => IntegerNumber.Create(value);
        public static implicit operator Number(int value) => IntegerNumber.Create(value);
        public static implicit operator Number(uint value) => IntegerNumber.Create(value);
        public static implicit operator Number(long value) => IntegerNumber.Create(value);
        public static implicit operator Number(ulong value) => IntegerNumber.Create(value);
        public static implicit operator Number(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator Number(ERational value) => RationalNumber.Create(value);
        public static implicit operator Number(EDecimal value) => RealNumber.Create(value);
        public static implicit operator Number(float value) => RealNumber.Create(EDecimal.FromSingle(value));
        public static implicit operator Number(double value) => RealNumber.Create(EDecimal.FromDouble(value));
        public static implicit operator Number(decimal value) => RealNumber.Create(EDecimal.FromDecimal(value));
        public static implicit operator Number(Complex value) =>
            ComplexNumber.Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
    }

    public class InvalidNumberCastException : InvalidCastException
    {
        public InvalidNumberCastException(Type typeFrom, Type typeTo)
            : base("Cannot cast from " + typeFrom + " to " + typeTo) { }
    }
    public class UniverseCollapseException : Exceptions.SysException
    {
        public UniverseCollapseException() : base("Universe collapse!") { }
    }

    internal static class EDecimalWrapper
    {
        internal static bool IsGreater(EDecimal a, EDecimal b)
            => a.CompareTo(b) == 1;

        internal static bool IsLess(EDecimal a, EDecimal b)
            => a.CompareTo(b) == -1;

        internal static bool IsEqual(EDecimal a, EDecimal b)
            => a.CompareTo(b) == 0;

        internal static bool IsGreaterOrEqual(EDecimal a, EDecimal b)
            => IsGreater(a, b) || IsEqual(a, b);

        internal static bool IsLessOrEqual(EDecimal a, EDecimal b)
            => IsLess(a, b) || IsEqual(a, b);
    }
}
