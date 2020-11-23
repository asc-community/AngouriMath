/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core.Exceptions;
using System;
using System.Numerics;

namespace AngouriMath
{
    partial record Entity
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        partial record Number
        {
            public static explicit operator double(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(double), num.GetType());
                return re.EDecimal.ToDouble();
            }

            public static explicit operator float(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(float), num.GetType());
                return (float)re.EDecimal.ToDouble();
            }

            public static explicit operator int(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(int), num.GetType());
                return re.EDecimal.ToInt32Checked();
            }

            public static explicit operator long(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(long), num.GetType());
                return re.EDecimal.ToInt64Checked();
            }

            public static explicit operator BigInteger(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(int), num.GetType());
                
                int plannedPrecision;
                try
                {
                    plannedPrecision = MathS.Settings.DecimalPrecisionContext.Value.Precision.ToInt32Checked();
                }
                catch (OverflowException)
                {
                    throw new InvalidNumberException("The given precision is too high");
                }

                return re.EDecimal.ToSizedEInteger(plannedPrecision * 3).ToBigInteger();
            }

            public static explicit operator System.Numerics.Complex(Number num)
            {
                if (num is not Complex c)
                    throw new NumberCastException(typeof(Complex), num.GetType());
                return new System.Numerics.Complex((double)c.RealPart, (double)c.ImaginaryPart);
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
