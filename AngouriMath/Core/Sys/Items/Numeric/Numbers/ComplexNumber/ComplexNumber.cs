﻿using System;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AngouriMath.Core
{
    public partial class ComplexNumber : Number
    {
        public RealNumber Real { get; protected set; }
        public RealNumber Imaginary { get; protected set; }
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

        protected internal string InternalToString()
            => Real.ToString() + " + " + Imaginary.ToString() + "i";

        protected internal string InternalLatexise()
        {
            var (Im, sign) = Imaginary.Value > 0 ? (Imaginary, "+") : (-Imaginary, "-");
            return Real.Latexise() + sign + Imaginary.Latexise(Imaginary.IsFraction() && Imaginary.IsDefinite()) + "i";
        }

        public ComplexNumber Conjugate()
            => new ComplexNumber(Real, Imaginary * new IntegerNumber(-1));

        public RealNumber Abs()
        => Number.Pow(Real * Real + Imaginary * Imaginary, new RealNumber(0.5m)) as RealNumber;

        internal static bool TryParse(string s, out ComplexNumber dst)
        {
            dst = null;
            return false;
        }
    }
}
