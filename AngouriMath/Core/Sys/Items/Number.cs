
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
using System.Globalization;
using System.Numerics;
 using System.Runtime.CompilerServices;
 using AngouriMath.Core.FromString;
 using AngouriMath.Core.Sys;

namespace AngouriMath.Core
{
    #pragma warning disable CS0660
    #pragma warning disable CS0661
    public partial class Number
    #pragma warning restore CS0660
    #pragma warning restore CS0661
    {
        /// <summary>
        /// To get real value of the number
        /// For example, new Number(3, 5).Re is 3
        /// </summary>
        public double Re { 
            get => IsNull ? double.NaN : value.Real; 
            set {
                this.value = new Complex(value, Im);
            } 
        }

        /// <summary>
        /// To get imaginary value of the number
        /// For example, new Number(3, 5).Im is 5
        /// </summary>
        public double Im
        {
            get => IsNull ? double.NaN : value.Imaginary;
            set {
                this.value = new Complex(Re, value);
                __isReal = this.value.Imaginary == 0;
            }
        }

        /// <summary>
        /// Fast access to Complex value
        /// </summary>
        internal Complex value;
        private bool isNull;
        public bool IsNull { get => isNull || (double.IsNaN(value.Imaginary)) || (double.IsNaN(value.Real)); set => isNull = value; }
        public Number(bool isNull)
        {
            this.isNull = isNull;
            __isReal = false;
        }

        public Number(object value)
        {
            if (value is Number)
            {
                var num = value as Number;
                IsNull = num.IsNull;
                __isReal = num.Im == 0;
                this.value = num.value;
            }
            else if (value is double || value is int)
            {
                var num = (double)value;
                IsNull = double.IsNaN(num);
                this.Re = num;
            }
        }

        /// <summary>
        /// Initialize a real number
        /// </summary>
        /// <param name="Re"></param>
        public Number(double Re)
        {
            IsNull = false;
            this.value = new Complex(Re, 0);
            __isReal = true;
        }

        /// <summary>
        /// Initialize a complex number 
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        public Number(double re, double im)
        {
            IsNull = false;
            __isReal = im == 0;
            this.value = new Complex(re, im);
        }
        public Number(Complex num)
        {
            IsNull = false;
            __isReal = num.Imaginary == 0;
            this.value = num;
        }
        public Number Copy()
        {
            return IsNull ? Number.Null : new Number(value);
        }

        /// <summary>
        /// Does not wrap the number with additional parentheses, unlike NumberEntity.ToString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string res;
            string a = Re.ToString(CultureInfo.InvariantCulture);
            if (Number.IsDoubleZero(Im))
            {
                if (Re == -0.0f) Re = 0.0f; // -0 workaround
                res = a;
            }
            else
            {
                string b = Math.Abs(Im).ToString(CultureInfo.InvariantCulture);
                if (!Number.IsDoubleZero(Re))
                    res = a + (Im < 0 ? " - " : " + ") + (Im == -1 || Im == 1 ? "" : b) + "i";
                else if (Im == -1)
                    res = "-i";
                else if (Im == 1)
                    res = "i";
                else if (Im == 0)
                    res = "0";
                else
                    res = (Im < 0 ? "-" : "") + b + "i";
            }
            return res.Replace(",", ".");
        }

        private static bool TryToDouble(string s, out double res)
            => double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out res);


        /// <summary>
        /// Parses a Number from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns>
        /// Number if parsing went without errors
        /// An exception thrown otherwise
        /// </returns>
        public static Number Parse(string s)
        {
            Number res;
            if (TryParse(s, out res))
                return res;
            else
                throw new ParseException("String `" + s + "` cannot be parsed as Number");
        }

        /// <summary>
        /// To get a number from a string
        /// </summary>
        /// <param name="s"></param>
        /// <param name="result">
        /// Number to be overriden by the result of the function (even if not successful)
        /// </param>
        /// <returns>
        /// true if parsing was successful,
        /// false otherwise
        /// </returns>
        public static bool TryParse(string s, out Number result)
        {
            result = null;
            if (s == "")
                return false;

            if (s == "-i")
            {
                result = new Number(0, -1);
                return true;
            }
            if (s == "i")
            {
                result = new Number(0, 1);
                return true;
            }
            if (s[s.Length - 1] == 'i')
            {
                double imag;
                if (TryToDouble(s.Substring(0, s.Length - 1), out imag))
                {
                    result = new Number(0, imag);
                    return true;
                }
                else
                    return false;
            }
            else
            {
                double real;
                if (TryToDouble(s, out real))
                {
                    result = new Number(real, 0);
                    return true;
                }
                else
                    return false;
            }
        }


        internal bool __isReal;
        public static implicit operator Number(int num) => new Number(num);
        public static implicit operator Number(double num) => new Number(num);
        public static implicit operator Number(Complex num) => new Number(num);
        public static implicit operator Complex(Number num) => num.value;
        public static Number operator +(Number a, Number b) => new Number(a.value + b.value);
        public static Number operator -(Number a, Number b) => new Number(a.value - b.value);
        public static Number operator *(Number a, Number b) => new Number(a.value * b.value);
        public static Number operator /(Number a, Number b) => new Number(a.value / b.value);
        public static Number operator+(Number n) => new Number( n.value);
        public static Number operator-(Number n) => new Number(-n.value);
        public static bool operator ==(Number a, Number b)
        {
            if ( ((object)a) == null && ((object)b) == null )
                return true;
            if (((object)a) == null || ((object)b) == null)
                return false;
            return Number.IsDoubleZero(Number.Abs(a - b));
        }
        public static bool operator !=(Number a, Number b)
        {
            if (a == null && b == null)
                return false;
            if (b == null)
                return !double.IsNaN(a.Re) && !double.IsNaN(a.Im);
            return !(a == b);
        }
        public static readonly Number Null = new Number(true);

        public bool IsReal()
            => !IsComplex();

        /// <summary>
        /// Check whether a number is complex or real
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
        {
            return !Number.IsDoubleZero(Im);
        }
        public bool IsInteger()
        {
            return !IsComplex() && (Number.IsDoubleZero(Re - Math.Round(Re)));
        }
        public bool IsNatural()
        {
            return !IsComplex() && IsInteger() && Re > 0;
        }
        public static Number Pow(Number a, Number b) => new Number(b.value == 0.5 ? Complex.Sqrt(a.value) : Complex.Pow(a.value, b.value));
        public static Number Log(Number a, Number @base) => new Number(Complex.Log(a.value, @base.Re));
        public static Number Sin(Number a) => a.__isReal ? new Number(Math.Sin(a.value.Real)) : new Number(Complex.Sin(a.value));
        public static Number Cos(Number a) => a.__isReal ? new Number(Math.Cos(a.value.Real)) : new Number(Complex.Cos(a.value));
        public static Number Tan(Number a) => a.__isReal ? new Number(Math.Tan(a.value.Real)) : new Number(Complex.Tan(a.value));

        /// <summary>
        /// Finds all roots of a number
        /// e. g. sqrt(1) = { -1, 1 }
        /// root(1, 4) = { -i, i, -1, 1 }
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rootPower"></param>
        /// <returns></returns>
        public static Set GetAllRoots(Number value, int rootPower)
        {
            var res = new Set();
            Complex val = value.value;
            double phi = (Complex.Log(val / Complex.Abs(val)) / MathS.i.value).Real;
            double newMod = Math.Pow(Complex.Abs(val), 1.0 / rootPower);
            Complex i = new Complex(0, 1);
            for (int n = 0; n < rootPower; n++)
            {
                double newPow = phi / rootPower + 2 * Math.PI * n / rootPower;
                res.Add(newMod * Complex.Pow(Math.E, i * newPow));
            }
            return res;
        }

        static readonly System.Random random = new Random();

        /// <summary>
        /// Returns a complex number a + ib such that a, base in [-1; 1]
        /// </summary>
        /// <returns></returns>
        public static Number Random()
            => new Number(random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1);
        public static Number Cotan(Number a) => a.__isReal ? new Number(1 / Math.Tan(a.value.Real)) : new Number(1 / Complex.Tan(a.value));
        public static Number Arcsin(Number a) => new Number(Complex.Asin(a.value));
        public static Number Arccos(Number a) => new Number(Complex.Acos(a.value));
        public static Number Arctan(Number a) => new Number(Complex.Atan(a.value));
        public static Number Arccotan(Number a) => new Number(Complex.Atan(1 / a.value));
        public static double Abs(Number a) => Complex.Abs(a.value);
        public static bool IsDoubleZero(double a)
        {
            return Math.Abs(a) < MathS.Utils.EQUALITY_THRESHOLD;
        }

        public static double Dist(Number n1, Number n2)
            => Abs(n1 - n2);

    }
}
