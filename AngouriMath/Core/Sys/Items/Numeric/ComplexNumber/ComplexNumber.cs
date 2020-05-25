using System;
using System.Globalization;
using System.IO.Compression;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AngouriMath.Core.Numerix
{
    public partial class ComplexNumber : Number
    {
        public new ComplexNumber Copy()
            => new ComplexNumber(this);
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
    }
}
