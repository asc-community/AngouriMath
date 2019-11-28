using System;
using System.Collections.Generic;
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

        public override string ToString()
        {
            string a = Re.ToString();
            if (Number.IsDoubleZero(Im))
                return a;
            string b = Math.Abs(Im).ToString();
            if (!Number.IsDoubleZero(Re))
                return a + (Im < 0 ? " - " : " + ") + b + "i";
            else
                return b + "i";
        }

        public static implicit operator Number(int num) => new Number(num);
        public static implicit operator Number(double num) => new Number(num);
        public static Number operator +(Number a, Number b) => new Number(a.value + b.value);
        public static Number operator -(Number a, Number b) => new Number(a.value - b.value);
        public static Number operator *(Number a, Number b) => new Number(a.value * b.value);
        public static Number operator /(Number a, Number b) => new Number(a.value / b.value);
        public static Number Pow(Number a, Number b) => new Number(Complex.Pow(a.value, b.value));
        public static Number Log(Number a, Number b) => new Number(Complex.Log(a.value, b.Re));
        public static Number Sin(Number a) => new Number(Complex.Sin(a.value));
        public static Number Cos(Number a) => new Number(Complex.Cos(a.value));
        public static bool IsDoubleZero(double a)
        {
            return Math.Abs(a) < 0.0000000000001;
        }
    }
}
