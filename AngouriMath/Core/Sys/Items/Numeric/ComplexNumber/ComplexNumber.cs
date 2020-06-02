
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
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using AngouriMath.Core.Exceptions;
using AngouriMath.Core.FromString;

namespace AngouriMath.Core.Numerix
{
    public partial class ComplexNumber : Number
    {
        public new ComplexNumber Copy()
            => Number.Copy(this) as ComplexNumber;
        public RealNumber Real { get; protected set; }
        public RealNumber Imaginary { get; protected set; }

        public (RealNumber.UndefinedState Re, RealNumber.UndefinedState Im) State
        {
            get => (Real.State, Imaginary.State);
        }

        public bool IsDefinite()
            => Real.IsDefinite() && Imaginary.IsDefinite();
        private void InitClass(RealNumber realPart, RealNumber imaginaryPart)
        {
            Real = realPart;
            Imaginary = imaginaryPart;
            Type = HierarchyLevel.COMPLEX;
            Init();
        }

        public ComplexNumber(Complex value)
        {
            InitClass(new RealNumber(value.Real), new RealNumber(value.Imaginary));
        }

        public ComplexNumber(RealNumber realPart, RealNumber imaginaryPart)
        {
            InitClass(realPart, imaginaryPart);
        }

        protected ComplexNumber()
        {
            
        }

        public ComplexNumber(Number number)
        {
            if (number.Is(HierarchyLevel.COMPLEX))
                InitClass((number as ComplexNumber).Real, (number as ComplexNumber).Imaginary);
            else
                throw new InvalidNumberCastException(number.Type, HierarchyLevel.COMPLEX);
        }

        protected internal void Init()
        {

        }
        
        protected override (decimal Re, decimal Im) GetValue()
        {
            return (Real.Value, Imaginary.Value);
        }

        private static RealNumber _zero = new RealNumber(0.0m);
        private static RealNumber _plus_one = new RealNumber(1.0m);
        private static RealNumber _minus_one = new RealNumber(-1.0m);
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
            var (Im, sign) = Imaginary.Value > 0 ? (Imaginary, "+") : (-Imaginary, "-");
            return Real.Latexise() + sign + Imaginary.Latexise(Imaginary.IsFraction() && Imaginary.IsDefinite()) + "i";
        }

        public ComplexNumber Conjugate()
            => new ComplexNumber(Real, Imaginary * Number.Create(-1));

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


        public static ComplexNumber Indefinite(RealNumber.UndefinedState re, RealNumber.UndefinedState im)
            => new ComplexNumber(new RealNumber(re), new RealNumber(im));

        public static ComplexNumber Indefinite(RealNumber.UndefinedState both)
            => new ComplexNumber(new RealNumber(both), new RealNumber(both));

        public static ComplexNumber NegNegInfinity()
            => new ComplexNumber(RealNumber.NegativeInfinity(), RealNumber.NegativeInfinity());
        public static ComplexNumber NegPosInfinity()
            => new ComplexNumber(RealNumber.NegativeInfinity(), RealNumber.PositiveInfinity());
        public static ComplexNumber PosNegInfinity()
            => new ComplexNumber(RealNumber.PositiveInfinity(), RealNumber.NegativeInfinity());
        public static ComplexNumber PosPosInfinity()
            => new ComplexNumber(RealNumber.PositiveInfinity(), RealNumber.PositiveInfinity());

        public static ComplexNumber Parse(string s)
        {
            if (TryParse(s, out var res))
                return Number.Functional.Downcast(res) as ComplexNumber;
            else
                throw new ParseException("Cannot parse number from " + s);
        }

        // TODO
        public static bool TryParse(string s, out ComplexNumber dst)
        {
            dst = null;
            if (string.IsNullOrEmpty(s))
                return false;
            if (RealNumber.TryParse(s, out var real))
            {
                dst = real;
                return true;
            }
            if (s.Last() == 'i')
                if (s == "i")
                {
                    dst = Number.Create(0.0, 1.0);
                    return true;
                }
                else if (s == "-i")
                {
                    dst = Number.Create(0.0, -1.0);
                    return true;
                }
                else if (RealNumber.TryParse(s.Substring(0, s.Length - 1), out var realPart))
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
