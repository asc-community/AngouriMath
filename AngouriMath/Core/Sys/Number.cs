using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace AngouriMath.Core
{
    #pragma warning disable CS0660
    #pragma warning disable CS0661
    public class Number
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
        public override string ToString()
        {
            string res;
            string a = Re.ToString(CultureInfo.InvariantCulture);
            if (Number.IsDoubleZero(Im))
            {
                res = a;
            }
            else
            {
                string b = Math.Abs(Im).ToString(CultureInfo.InvariantCulture);
                if (!Number.IsDoubleZero(Re))
                    res = a + (Im < 0 ? " - " : " + ") + b + "i";
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
        public static double ToDouble(string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// To get a number from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Number Parse(string s)
        {
            if (s == "-i")
            {
                return new Number(0, -1);
            }
            if (s == "i")
            {
                return new Number(0, 1);
            }
            if (s[s.Length - 1] == 'i')
            {
                return new Number(0, ToDouble(s.Substring(0, s.Length - 1)));
            }
            else
            {
                return ToDouble(s);
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
        public static bool operator >(Number a, Number b)
        {
            if (a.IsComplex() || b.IsComplex())
                throw new MathSException("Can't compare complex number with a number");
            return a.Re > b.Re;
        }
        public static bool operator <(Number a, Number b)
        {
            if (a.IsComplex() || b.IsComplex())
                throw new MathSException("Can't compare complex number with a number");
            return a.Re < b.Re;
        }
        public static readonly Number Null = new Number(true);

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
        public static Number Pow(Number a, Number b) => new Number(b.value == 0.5 ? Complex.Sqrt(a.value) : Complex.Pow(a.value, b.value));
        public static Number Log(Number a, Number b) => new Number(Complex.Log(a.value, b.Re));
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
        public static NumberSet GetAllRoots(Number value, int rootPower)
        {
            var res = new NumberSet();
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

        public static Number Cotan(Number a) => a.__isReal ? new Number(1 / Math.Tan(a.value.Real)) : new Number(1 / Complex.Tan(a.value));
        public static Number Arcsin(Number a) => a.__isReal ? new Number(Math.Asin(a.value.Real)) : new Number(Complex.Asin(a.value));
        public static Number Arccos(Number a) => a.__isReal ? new Number(Math.Acos(a.value.Real)) : new Number(Complex.Acos(a.value));
        public static Number Arctan(Number a) => a.__isReal ? new Number(Math.Atan(a.value.Real)) : new Number(Complex.Atan(a.value));
        public static Number Arccotan(Number a) => a.__isReal ? new Number(Math.Atan(1 / a.value.Real)) : new Number(Complex.Atan(1 / a.value));
        public static double Abs(Number a) => Complex.Abs(a.value);
        public static bool IsDoubleZero(double a)
        {
            return Math.Abs(a) < MathS.EQUALITY_THRESHOLD;
        }

        internal void _add(Number a) => this.value += a.value;
        internal void _sub(Number a) => this.value -= a.value;
        internal void _mul(Number a) => this.value *= a.value;
        internal void _div(Number a) => this.value /= a.value;
        internal void _pow(Number a) => this.value = Complex.Pow(this.value, a.value);
        internal void _log(Number a) => this.value = Complex.Log(this.value, a.Re);
        internal void _sin() => this.value = Complex.Sin(this.value);
        internal void _cos() => this.value = Complex.Cos(this.value);

    }

    /// <summary>
    /// A set of Numbers
    /// </summary>
    public class NumberSet : List<Number>
    {
        internal NumberSet()
        {

        }
        internal void Include(Number num)
        {
            var alreadyExists = false;
            foreach (var elem in this)
                if (elem == num)
                {
                    alreadyExists = true;
                    break;
                }
            if (!alreadyExists)
                this.Add(num);
        }
        public override string ToString()
        {
            return "[" + string.Join(", ", this) + "]";
        }
    }
}
