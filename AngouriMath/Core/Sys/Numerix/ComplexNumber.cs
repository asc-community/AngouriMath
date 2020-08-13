
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

using System.Linq;
using System.Numerics;
using AngouriMath;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;
using AngouriMath.Core.Numerix;
using PeterO.Numbers;

namespace AngouriMath.Core.Numerix
{
    /// <summary>
    /// Extension for RealNumbers
    /// https://en.wikipedia.org/wiki/Complex_number
    /// Constructor does not downcast automatically. Use <see cref="Create(RealNumber, RealNumber)"/> for automatic downcasting
    /// </summary>
    public record ComplexNumber(RealNumber? Real_, RealNumber? Imaginary_) : Entity.Num
    {
        public virtual RealNumber Real => Real_ ?? IntegerNumber.Zero;
        public RealNumber Imaginary => Imaginary_ ?? IntegerNumber.Zero;
        public override Priority Priority =>
            (Real, Imaginary) switch
            {
                ({ IsZero: false }, { IsZero: false }) => Priority.Sum,
                (_, { IsNegative: true }) => Priority.Mul,
                _ => Priority.Num
            };
        public static readonly ComplexNumber ImaginaryOne = new ComplexNumber(0, 1);
        public static readonly ComplexNumber MinusImaginaryOne = new ComplexNumber(0, -1);

        protected override bool ThisIsFinite => Real.Value.IsFinite && Imaginary.Value.IsFinite;
        public override bool IsExact => Real.IsExact && Imaginary.IsExact;
        public bool IsZero => Real.Value.IsZero && Imaginary.Value.IsZero;
        public bool IsNaN => this == RealNumber.NaN;

        public static ComplexNumber Create(RealNumber real, RealNumber imaginary) =>
            Create(real.Value, imaginary.Value);
        public static ComplexNumber Create(EDecimal real, EDecimal imaginary)
        {
            if (!MathS.Settings.DowncastingEnabled)
                return new ComplexNumber(real, imaginary);
            if (real.IsNaN() || imaginary.IsNaN())
                return RealNumber.NaN;
            if (imaginary.IsFinite && IsZero(imaginary)) // Momo's fix
                return RealNumber.Create(real);
            else
                return new ComplexNumber(RealNumber.Create(real), RealNumber.Create(imaginary));
        }
        public void Deconstruct(out RealNumber real, out RealNumber imaginary) => (real, imaginary) = (Real, Imaginary);

        internal override string Stringize()
        {
            static string RenderNum(RealNumber number)
            {
                if (number == IntegerNumber.MinusOne)
                    return "-";
                else if (number == IntegerNumber.One)
                    return "";
                else
                    return number.ToString();
            }
            if (Imaginary.IsFinite && Imaginary == IntegerNumber.Zero)
                return Real.ToString();
            else if (Real.IsFinite && Real == IntegerNumber.Zero)
                return RenderNum(Imaginary) + "i";
            var (l, r) = Imaginary is RationalNumber and not IntegerNumber ? ("(", ")") : ("", "");
            var (im, sign) = Imaginary > 0 ? (Imaginary, "+") : (-Imaginary, "-");
            return Real.ToString() + " " + sign + " " + l + RenderNum(im) + r + "i";
        }

        public override string Latexise()
        {
            static string RenderNum(RealNumber number)
            {
                if (number == IntegerNumber.MinusOne)
                    return "-";
                else if (number == IntegerNumber.One)
                    return "";
                else
                    return number.Latexise();
            }
            if (Imaginary.IsFinite && Imaginary == IntegerNumber.Zero)
                return Real.Latexise();
            else if (Real.IsFinite && Real == IntegerNumber.Zero)
                return RenderNum(Imaginary) + "i";
            var (im, sign) = Imaginary > 0 ? (Imaginary, "+") : (-Imaginary, "-");
            return Real.Latexise() + " " + sign + " " +
                (im == 1 ? "" : im.Latexise(Imaginary is RationalNumber and not IntegerNumber)) + "i";
        }

        /// <summary>Returns conjugate of a complex number. Given this = a + ib, Conjugate() -> a - ib</summary>
        /// <returns>Conjugate of the number</returns>
        public ComplexNumber Conjugate() => Create(Real, -Imaginary);

        /// <summary>The magnitude of this <see cref="ComplexNumber"/>. See <see cref="Number.Abs(ComplexNumber)"/></summary>
        public virtual RealNumber Abs() =>
            (RealNumber)Sqrt(Real.Value * Real.Value + Imaginary.Value * Imaginary.Value);

        public RealNumber Phase() => Imaginary.Value.Atan2(Real.Value, MathS.Settings.DecimalPrecisionContext);

        public static ComplexNumber CreatePolar(EDecimal magnitude, EDecimal phase)
        {
            var context = MathS.Settings.DecimalPrecisionContext;
            return Create(magnitude.Multiply(phase.Cos(context), context), magnitude.Multiply(phase.Sin(context), context));
        }

        /// <summary>-oo + -ooi</summary>
        public static readonly ComplexNumber NegNegInfinity =
            new ComplexNumber(RealNumber.NegativeInfinity, RealNumber.NegativeInfinity);

        /// <summary>-oo + +ooi</summary>
        public static readonly ComplexNumber NegPosInfinity =
            new ComplexNumber(RealNumber.NegativeInfinity, RealNumber.PositiveInfinity);

        /// <summary>+oo + -ooi</summary>
        public static readonly ComplexNumber PosNegInfinity =
            new ComplexNumber(RealNumber.PositiveInfinity, RealNumber.NegativeInfinity);

        /// <summary>+oo + +ooi</summary>
        public static readonly ComplexNumber PosPosInfinity =
            new ComplexNumber(RealNumber.PositiveInfinity, RealNumber.PositiveInfinity);

        /// <summary>
        /// Parses a string into ComplexNumber
        /// May throw ParseException
        /// </summary>
        /// <returns>
        /// ComplexNumber
        /// </returns>
        public static ComplexNumber Parse(string source) =>
            TryParse(source, out var res) ? res : throw new ParseException("Cannot parse number from " + source);

        /// <summary>
        /// Tries to parse a ComplexNumber from string
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dst">
        /// The result will be written to this variable only if parsing was successful,
        /// if it was not, do not access this variable
        /// </param>
        /// <returns>
        /// If parsing was successful - true,
        /// false otherwise
        /// </returns>
        // TODO: parse more possible values of complex numbers
        public static bool TryParse(string source,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out ComplexNumber? dst)
        {
            dst = null;
            if (string.IsNullOrEmpty(source))
                return false;
            if (RealNumber.TryParse(source, out var real))
            {
                dst = real;
                return true;
            }
            if (source.Last() == 'i')
                if (source == "i")
                {
                    dst = ImaginaryOne;
                    return true;
                }
                else if (source == "-i")
                {
                    dst = MinusImaginaryOne;
                    return true;
                }
                else if (RealNumber.TryParse(source.Substring(0, source.Length - 1), out var realPart))
                {
                    dst = new ComplexNumber(0, realPart);
                    return true;
                }
                else
                    return false;
            return false;
        }
        public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b) => OpSum(a, b);
        public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b) => OpSub(a, b);
        public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b) => OpMul(a, b);
        public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b) => OpDiv(a, b);
        public static ComplexNumber operator +(ComplexNumber a) => a;
        public static ComplexNumber operator -(ComplexNumber a) => OpMul(IntegerNumber.MinusOne, a);
        public static bool operator ==(ComplexNumber a, ComplexNumber b) => AreEqual(a, b);
        public static bool operator !=(ComplexNumber a, ComplexNumber b) => !AreEqual(a, b);
        public static implicit operator ComplexNumber(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(byte value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(short value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(ushort value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(int value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(uint value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(long value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(ulong value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator ComplexNumber(ERational value) => RationalNumber.Create(value);
        public static implicit operator ComplexNumber(EDecimal value) => RealNumber.Create(value);
        public static implicit operator ComplexNumber(float value) => RealNumber.Create(EDecimal.FromSingle(value));
        public static implicit operator ComplexNumber(double value) => RealNumber.Create(EDecimal.FromDouble(value));
        public static implicit operator ComplexNumber(decimal value) => RealNumber.Create(EDecimal.FromDecimal(value));
        public static implicit operator ComplexNumber(Complex value) =>
            Create(EDecimal.FromDouble(value.Real), EDecimal.FromDouble(value.Imaginary));
    }
}
