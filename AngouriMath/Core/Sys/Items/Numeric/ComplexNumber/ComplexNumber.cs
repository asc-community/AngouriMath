
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
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;

namespace AngouriMath.Core.Numerix
{
    /// <summary>
    /// Extension for RealNumbers
    /// https://en.wikipedia.org/wiki/Complex_number
    /// </summary>
    public partial class ComplexNumber : Number
    {
        /// <summary>
        /// Copies a complex number to keep it ComplexNumber class
        /// </summary>
        /// <returns></returns>
        public new ComplexNumber Copy()
            => Number.Copy(this) as ComplexNumber;

        /// <summary>
        /// Real part of the complex number
        /// </summary>
        public RealNumber Real { get; protected set; }

        /// <summary>
        /// Imaginary part of the complex number
        /// </summary>
        public RealNumber Imaginary { get; protected set; }

        /// <summary>
        /// Pair of definition states for Real and Imaginary parts respectively. See RealNumber.State
        /// </summary>
        public (RealNumber.UndefinedState Re, RealNumber.UndefinedState Im) State
            => (Real.State, Imaginary.State);
        

        /// <summary>
        /// Checks whether both parts of the complex number are definite. See RealNumber.IsDefinite()
        /// </summary>
        /// <returns></returns>
        public bool IsDefinite()
            => Real.IsDefinite() && Imaginary.IsDefinite();

        /// <summary>
        /// Inits all versions of the number beyond complex representation
        /// </summary>
        /// <param name="realPart"></param>
        /// <param name="imaginaryPart"></param>
        private void InitClass(RealNumber realPart, RealNumber imaginaryPart)
        {
            Real = realPart;
            Imaginary = imaginaryPart;
            Type = HierarchyLevel.COMPLEX;
            Init();
        }

        /// <summary>
        /// Use Number.Create(RealNumber, RealNumber) instead
        /// </summary>
        /// <param name="value"></param>
        internal ComplexNumber(Complex value)
        {
            InitClass(new RealNumber(value.Real), new RealNumber(value.Imaginary));
        }

        /// <summary>
        /// Use Number.Create(RealNumber, RealNumber) instead
        /// </summary>
        /// <param name="value"></param>
        internal ComplexNumber(RealNumber realPart, RealNumber imaginaryPart)
        {
            InitClass(realPart, imaginaryPart);
        }

        /// <summary>
        /// Use Number.Create(RealNumber, RealNumber) instead
        /// </summary>
        /// <param name="value"></param>
        protected ComplexNumber()
        {
            
        }

        /// <summary>
        /// Use Number.Copy(Number) instead, or this.Copy()
        /// </summary>
        /// <param name="value"></param>
        internal ComplexNumber(Number number)
        {
            if (number.Is(HierarchyLevel.COMPLEX))
                InitClass((number as ComplexNumber).Real, (number as ComplexNumber).Imaginary);
            else
                throw new InvalidNumberCastException(number.Type, HierarchyLevel.COMPLEX);
        }

        protected internal void Init()
        {
            /*
             will be filled with initialization of parent class, quaternion,
             when the last one will be implemented
            */
        }

        protected override (decimal Re, decimal Im) GetValue()
        {
            return (Real.Value, Imaginary.Value);
        }

        private static readonly RealNumber _zero = new RealNumber(0.0m);
        private static readonly RealNumber _plus_one = new RealNumber(1.0m);
        private static readonly RealNumber _minus_one = new RealNumber(-1.0m);
        protected internal string InternalToString()
        {
            string RenderNum(RealNumber number)
            {
                if (number == _minus_one)
                    return "-";
                else if (number == _plus_one)
                    return "";
                else
                    return number.ToString();
            }
            if (Imaginary.IsDefinite() && Imaginary == _zero)
                return Real.ToString();
            else if (Real.IsDefinite() && Real.Value == _zero)
                return RenderNum(Imaginary) + "i";
            return Real.ToString() + " + " + RenderNum(Imaginary) + "i";
        }

        protected internal string InternalLatexise()
        {
            string RenderNum(RealNumber number)
            {
                if (number == _minus_one)
                    return "-";
                else if (number == _plus_one)
                    return "";
                else
                    return number.Latexise();
            }
            if (Imaginary.IsDefinite() && Imaginary == _zero)
                return Real.Latexise();
            else if (Real.IsDefinite() && Real.Value == _zero)
                return RenderNum(Imaginary) + "i";
            var (im, sign) = Imaginary.Value > 0 ? (Imaginary, "+") : (-Imaginary, "-");
            return Real.Latexise() + sign + im.Latexise(im.IsFraction() && im.IsDefinite()) + "i";
        }

        /// <summary>
        /// Returns conjugate of a complex number
        /// Given this = a + ib, Conjugate() -> a - ib
        /// </summary>
        /// <returns>
        /// Conjugate of the number
        /// </returns>
        public ComplexNumber Conjugate()
            => new ComplexNumber(Real, Imaginary * Number.Create(-1));

        /// <summary>
        /// See Number.Abs(ComplexNumber)
        /// </summary>
        /// <returns></returns>
        public RealNumber Abs()
        {
            var Re2 = Real * Real;
            var Im2 = Imaginary * Imaginary;
            var sum2 = Re2 + Im2;
            if (sum2.State == RealNumber.UndefinedState.NAN)
                return RealNumber.NaN();
            if (sum2.State == RealNumber.UndefinedState.POSITIVE_INFINITY)
                return RealNumber.PositiveInfinity();
            if (sum2.State == RealNumber.UndefinedState.NEGATIVE_INFINITY)
                throw new SysException("Universe collapse"); // unreacheable case (I hope so)
            return Number.Pow(Real * Real + Imaginary * Imaginary, new RealNumber(0.5m)) as RealNumber;
        }

        /// <summary>
        /// Creates an indefinite complex number on both real and imaginary parts separately, e. g.
        /// ComplexNumber Indefinite(RealNumber.UndefinedState.POSITIVE_INFINITY, RealNumber.UndefinedState.NEGATIVE_INFINITY)
        /// -> +oo + -ooi
        /// </summary>
        /// <param name="re">
        /// State that must not be DEFINED (otherwise an expception will be thrown)
        /// </param>
        /// <param name="im">
        /// State that must not be DEFINED (otherwise an expception will be thrown)
        /// </param>
        /// <returns></returns>
        internal static ComplexNumber Indefinite(RealNumber.UndefinedState re, RealNumber.UndefinedState im)
            => new ComplexNumber(new RealNumber(re), new RealNumber(im));

        /// <summary>
        /// Creates an indefinite complex number on both real and imaginary parts, e. g.
        /// ComplexNumber Indefinite(RealNumber.UndefinedState.POSITIVE_INFINITY)
        /// -> +oo + +ooi
        /// </summary>
        /// <param name="re">
        /// State that must not be DEFINED (otherwise an expception will be thrown)
        /// </param>
        /// <param name="im">
        /// State that must not be DEFINED (otherwise an expception will be thrown)
        /// </param>
        /// <returns></returns>
        internal static ComplexNumber Indefinite(RealNumber.UndefinedState both)
            => Indefinite(both, both);

        /// <summary>
        /// Special case of Indefinite() (see ComplexNumber.Indefinite()), -oo + -ooi
        /// </summary>
        /// <returns></returns>
        internal static ComplexNumber NegNegInfinity()
            => new ComplexNumber(RealNumber.NegativeInfinity(), RealNumber.NegativeInfinity());

        /// <summary>
        /// Special case of Indefinite() (see ComplexNumber.Indefinite()), -oo + +ooi
        /// </summary>
        /// <returns></returns>
        internal static ComplexNumber NegPosInfinity()
            => new ComplexNumber(RealNumber.NegativeInfinity(), RealNumber.PositiveInfinity());

        /// <summary>
        /// Special case of Indefinite() (see ComplexNumber.Indefinite()), +oo + -ooi
        /// </summary>
        /// <returns></returns>
        internal static ComplexNumber PosNegInfinity()
            => new ComplexNumber(RealNumber.PositiveInfinity(), RealNumber.NegativeInfinity());

        /// <summary>
        /// Special case of Indefinite() (see ComplexNumber.Indefinite()), +oo + +ooi
        /// </summary>
        /// <returns></returns>
        internal static ComplexNumber PosPosInfinity()
            => new ComplexNumber(RealNumber.PositiveInfinity(), RealNumber.PositiveInfinity());

        /// <summary>
        /// Parses a string into ComplexNumber
        /// May throw ParseException
        /// </summary>
        /// <returns>
        /// ComplexNumber
        /// </returns>
        public static ComplexNumber Parse(string source)
        {
            if (TryParse(source, out var res))
                return Number.Functional.Downcast(res) as ComplexNumber;
            else
                throw new ParseException("Cannot parse number from " + source);
        }

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
        public static bool TryParse(string source, out ComplexNumber dst)
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
                    dst = Number.Create(0.0, 1.0);
                    return true;
                }
                else if (source == "-i")
                {
                    dst = Number.Create(0.0, -1.0);
                    return true;
                }
                else if (RealNumber.TryParse(source.Substring(0, source.Length - 1), out var realPart))
                {
                    dst = Number.Create(0, realPart);
                    return true;
                }
                else
                    return false;
            return false;
        }
    }
}
