﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace AngouriMath.Core
{
    public abstract partial class Number
    {
        public bool IsNatural()
            => Is(HierarchyLevel.INTEGER) && (this as IntegerNumber).Value > 0;

        public bool IsInteger()
            => Is(HierarchyLevel.INTEGER);

        public bool IsFraction()
            => IsRational() && !IsInteger();

        public bool IsRational()
            => Is(HierarchyLevel.RATIONAL);

        public bool IsIrrational()
            => IsReal() && !IsRational();

        public bool IsReal()
            => Is(HierarchyLevel.REAL);

        public bool IsComplex()
            => Is(HierarchyLevel.COMPLEX);

        public bool IsImaginary()
            => IsComplex() && !IsReal();

        

        public IntegerNumber AsIntegerNumber()
            => new IntegerNumber(this);

        public RationalNumber AsRationalNumber()
            => new RationalNumber(this);

        public RealNumber AsRealNumber()
            => new RealNumber(this);

        public ComplexNumber AsComplexNumber()
            => new ComplexNumber(this);

        public long AsInt()
            => AsIntegerNumber().Value;

        public decimal AsDecimal()
            => AsRealNumber().Value;

        public Complex AsComplex()
        {
            var asCom = AsComplexNumber();
            return new Complex(asCom.Real, asCom.Imaginary);
        }

        public double AsDouble()
            => (double) AsDecimal();


        public static implicit operator Number(int value)
            => new IntegerNumber(value);
        public static implicit operator Number(long value)
            => new IntegerNumber(value);

        public static implicit operator Number(float value)
            => new RealNumber((decimal)value);
        public static implicit operator Number(double value)
            => new RealNumber((decimal)value);
        public static implicit operator Number(decimal value)
            => new RealNumber(value);

        public static implicit operator Number(Complex a)
        => new ComplexNumber(new RealNumber(a.Real), new RealNumber(a.Imaginary));
    }
}
