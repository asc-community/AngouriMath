//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using System;
using System.Numerics;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator decimal(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(decimal), num.GetType());
                return re.EDecimal.ToDecimal();
            }

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator double(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(double), num.GetType());
                return re.EDecimal.ToDouble();
            }

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator float(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(float), num.GetType());
                return (float)re.EDecimal.ToDouble();
            }

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator int(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(int), num.GetType());
                return re.EDecimal.ToInt32Checked();
            }

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator long(Number num)
            {
                if (num is not Real re)
                    throw new NumberCastException(typeof(long), num.GetType());
                return re.EDecimal.ToInt64Checked();
            }

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
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

            /// <summary>
            /// Casts a <see cref="Number"/> into a primitive.
            /// </summary>
            /// <exception cref="NumberCastException">
            /// Thrown when either overflow or the instance of <see cref="Number"/> cannot be downcasted.
            /// </exception>
            public static explicit operator System.Numerics.Complex(Number num)
            {
                if (num is not Complex c)
                    throw new NumberCastException(typeof(Complex), num.GetType());
                return new System.Numerics.Complex((double)c.RealPart, (double)c.ImaginaryPart);
            }
        }
    }
}
