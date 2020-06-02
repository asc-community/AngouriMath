
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
using System.Numerics;
using System.Text;

namespace AngouriMath.Core.Numerix
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
            => new IntegerNumber(Functional.Downcast(this));

        public RationalNumber AsRationalNumber()
            => new RationalNumber(Functional.Downcast(this));

        public RealNumber AsRealNumber()
            => new RealNumber(Functional.Downcast(this));

        public ComplexNumber AsComplexNumber()
            => new ComplexNumber(Functional.Downcast(this));

        public long AsInt()
            => (long)AsIntegerNumber().Value;

        public BigInteger AsBigInt()
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
            => Number.Create(value);
        public static implicit operator Number(long value)
            => Number.Create(value);

        public static implicit operator Number(float value)
            => new RealNumber((decimal)value);
        public static implicit operator Number(double value)
            => new RealNumber((decimal)value);
        public static implicit operator Number(decimal value)
            => new RealNumber(value);

        public static implicit operator Number(Complex a)
        => Number.Create(a.Real, a.Imaginary);
    }
}
