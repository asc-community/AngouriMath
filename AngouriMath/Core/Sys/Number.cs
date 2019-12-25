using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace AngouriMath.Core
{
    public class Number
    {
        /// <summary>
        /// To get real value of the number
        /// For example, new Number(3, 5).Re is 3
        /// </summary>
        public double Re { 
            get => value.Real; 
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
            get => value.Imaginary; 
            set {
                this.value = new Complex(Re, value);
            }
        }
        internal Complex value;

        /// <summary>
        /// Initialize a real number
        /// </summary>
        /// <param name="Re"></param>
        public Number(double Re)
        {
            this.Re = Re;
            this.Im = 0;
        }

        /// <summary>
        /// Initialize a complex number 
        /// </summary>
        /// <param name="Re"></param>
        /// <param name="Im"></param>
        public Number(double Re, double Im)
        {
            this.Re = Re;
            this.Im = Im;
        }
        public Number(Complex num)
        {
            this.value = num;
        }
        public Number Copy()
        {
            return new Number(value);
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

        public static implicit operator Number(int num) => new Number(num);
        public static implicit operator Number(double num) => new Number(num);
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

        /// <summary>
        /// Check whether a number is complex or real
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
        {
            return !Number.IsDoubleZero(Im);
        }
        public static Number Pow(Number a, Number b) => new Number(Complex.Pow(a.value, b.value));
        public static Number Log(Number a, Number b) => new Number(Complex.Log(a.value, b.Re));
        public static Number Sin(Number a) => new Number(Complex.Sin(a.value));
        public static Number Cos(Number a) => new Number(Complex.Cos(a.value));
        public static double Abs(Number a) => Complex.Abs(a.value);
        public static bool IsDoubleZero(double a)
        {
            return Math.Abs(a) < MathS.EQUALITY_THRESHOLD;
        }
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
