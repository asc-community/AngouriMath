
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

using System.Numerics;
using PeterO.Numbers;

namespace AngouriMath.Core.Numerix
{
    /// <summary>
    /// This class represents all possible numerical values as a hierarchy,
    /// Number
    ///   ╚═ ComplexNumber
    ///             ╚═ RealNumber
    ///                     ╚═ RationalNumber
    ///                              ╚═ IntegerNumber
    /// </summary>
    public abstract partial class Number
    {
        /// <summary>
        /// Natural number is an integer number with a value of greater than 0
        /// this ∈ N
        /// </summary>
        /// <returns></returns>
        public bool IsNatural()
            => Is(HierarchyLevel.INTEGER) && (this as IntegerNumber).Value > 0;

        /// <summary>
        /// Checks whether a Number is Integer
        /// this ∈ Z
        /// </summary>
        /// <returns></returns>
        public bool IsInteger()
            => Is(HierarchyLevel.INTEGER);

        /// <summary>
        /// Checks whether a Number is a rational number but is not integer
        /// this ∈ Q Λ this ∉ Z
        /// </summary>
        /// <returns></returns>
        public bool IsFraction()
            => IsRational() && !IsInteger();

        /// <summary>
        /// Checks whether a Number is a rational
        /// this ∈ Q
        /// </summary>
        /// <returns></returns>
        public bool IsRational()
            => Is(HierarchyLevel.RATIONAL);

        /// <summary>
        /// Checks whether one is real but not rational
        /// this ∈ R Λ this ∉ Q
        /// </summary>
        /// <returns></returns>
        public bool IsIrrational()
            => IsReal() && !IsRational();

        /// <summary>
        /// Checks whether one is real
        /// this ∈ R
        /// </summary>
        /// <returns></returns>
        public bool IsReal()
            => Is(HierarchyLevel.REAL);

        /// <summary>
        /// Checks whether one is complex
        /// this ∈ C
        /// this function always returns true because AM doesn't support quaternions
        /// </summary>
        /// <returns></returns>
        public bool IsComplex()
            => Is(HierarchyLevel.COMPLEX);

        /// <summary>
        /// Checks whether one is complex but not real
        /// this ∈ C Λ this ∉ R
        /// </summary>
        /// <returns></returns>
        public bool IsImaginary()
            => IsComplex() && !IsReal();

        
        /// <summary>
        /// Forcefully casts one to IntegerNumber with downcasting
        /// </summary>
        /// <returns></returns>
        public IntegerNumber AsIntegerNumber()
            => new IntegerNumber(Functional.Downcast(this));

        /// <summary>
        /// Forcefully casts one to RationalNumber with downcasting
        /// </summary>
        /// <returns></returns>
        public RationalNumber AsRationalNumber()
            => new RationalNumber(Functional.Downcast(this));

        /// <summary>
        /// Forcefully casts one to RealNumber with downcasting
        /// </summary>
        /// <returns></returns>
        public RealNumber AsRealNumber()
            => new RealNumber(Functional.Downcast(this));

        /// <summary>
        /// Forcefully casts one to ComplexNumber with downcasting
        /// </summary>
        /// <returns></returns>
        public ComplexNumber AsComplexNumber()
            => new ComplexNumber(Functional.Downcast(this));

        /// <summary>
        /// Forcefully casts one to long with downcasting
        /// </summary>
        /// <returns></returns>
        public long AsInt()
            => (long)AsIntegerNumber().Value;

        /// <summary>
        /// Forcefully casts one to EInteger with downcasting
        /// </summary>
        /// <returns></returns>
        public EInteger AsBigInt()
            => AsIntegerNumber().Value;

        /// <summary>
        /// Forcefully casts one to EDecimal with downcasting
        /// </summary>
        /// <returns></returns>
        public EDecimal AsDecimal()
            => AsRealNumber().Value;

        /// <summary>
        /// Forcefully casts one to Complex with downcasting
        /// </summary>
        /// <returns></returns>
        public Complex AsComplex()
        {
            var asCom = AsComplexNumber();
            return new Complex(asCom.Real, asCom.Imaginary);
        }

        /// <summary>
        /// Forcefully casts one to double with downcasting
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
            => (double) AsDecimal();


        public static implicit operator Number(int value)
            => Number.Create(value);
        public static implicit operator Number(long value)
            => Number.Create(value);

        public static implicit operator Number(float value)
            => new RealNumber(EDecimal.FromDouble(value));
        public static implicit operator Number(double value)
            => new RealNumber(EDecimal.FromDouble(value));
        public static implicit operator Number(EDecimal value)
            => new RealNumber(value);

        public static implicit operator Number(Complex a)
        => Number.Create(a.Real, a.Imaginary);
    }
}
