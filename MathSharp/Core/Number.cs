using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text;

namespace MathSharp.Core
{
    public class Number
    {
        public double Re { 
            get => value.Real; 
            set {
                this.value = new Complex(value, Im);
            } 
        }
        public double Im
        {
            get => value.Imaginary; 
            set {
                this.value = new Complex(Re, value);
            }
        }
        internal Complex value;
        public Number(double Re, double Im = 0)
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
            string a = Re.ToString();
            if (Number.IsDoubleZero(Im))
            {
                return a;
            }
            string b = Math.Abs(Im).ToString();
            if (!Number.IsDoubleZero(Re))
                return a + (Im < 0 ? " - " : " + ") + b + "i";
            else
                return b + "i";
        }
        public static double ToDouble(string s)
        {
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
        public static Number Parse(string s)
        {
            if (s.Length == 1 && s == "i")
                return new Number(0, 1);
            if (s[s.Length - 1] == 'i')
            {
                return new Number(0, ToDouble(s.Substring(0, s.Length - 1)));
            }
            else
                return ToDouble(s);
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
        public static Number Pow(Number a, Number b) => new Number(Complex.Pow(a.value, b.value));
        public static Number Log(Number a, Number b) => new Number(Complex.Log(a.value, b.Re));
        public static Number Sin(Number a) => new Number(Complex.Sin(a.value));
        public static Number Cos(Number a) => new Number(Complex.Cos(a.value));
        public static double Abs(Number a) => Complex.Abs(a.value);
        public static bool IsDoubleZero(double a)
        {
            return Math.Abs(a) < 0.0000000000001;
        }
    }
}
