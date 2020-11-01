/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Number
        {
            /// <summary>
            /// The denominator cannot be zero as the resulting value will not be a rational
            /// </summary>
            public partial record Rational : Real, System.IComparable<Rational>
            {
                /// <summary>
                /// Constructor does not downcast automatically.
                /// Use <see cref="Create(EInteger, EInteger)"/> or <see cref="Create(ERational)"/> for automatic downcasting.
                /// </summary>
                private protected Rational(ERational value)
                    : base(value.ToEDecimal(MathS.Settings.DecimalPrecisionContext)) => ERational = value;

                internal override Priority Priority => Priority.Div;

                /// <summary>
                /// The PeterO number representation in rational
                /// </summary>
                public ERational ERational { get; }

                /// <summary>
                /// A getter for the numerator
                /// </summary>
                // TODO: cache it
                public Integer Numerator => ERational.Numerator;

                /// <summary>
                /// A getter for the denominator
                /// </summary>
                // TODO: cache it
                public Integer Denominator => ERational.Denominator;

#pragma warning disable CS1591

                public void Deconstruct(out ERational rational) => rational = ERational;
                public void Deconstruct(out int? numerator, out int? denominator)
                {
                    numerator = ERational.Numerator.CanFitInInt32() ? ERational.Numerator.ToInt32Unchecked() : new int?();
                    denominator = ERational.Denominator.CanFitInInt32() ? ERational.Denominator.ToInt32Unchecked() : new int?();
                }

#pragma warning restore CS1591

                /// <inheritdoc/>
                public override bool IsExact => true;

                /// <summary>
                /// Creates an instance of Rational number of two integers
                /// </summary>
                public static Rational Create(EInteger numerator, EInteger denominator) =>
                    Create(ERational.Create(numerator, denominator));

                /// <summary>
                /// Creates an instance of Rational number
                /// </summary>
                public static Rational Create(ERational value)
                {
                    if (!value.IsFinite)
                        throw new System.ArgumentException("Non-finite values are not rationals - use RealNumber.Create instead");

                    if (!MathS.Settings.DowncastingEnabled)
                        return new Rational(value.ToLowestTerms());

                    // Call ToLowestTerms() through new RationalNumber first
                    // before determining whether the denominator equals one
                    var @return = new Rational(value.ToLowestTerms());
                    if (@return.ERational.Denominator.Equals(1))
                        return Integer.Create(@return.ERational.Numerator);
                    else
                        return @return;
                }

                // TODO: When we target .NET 5, remember to use covariant return types
                /// <inheritdoc/>
                public override Real Abs() => Create(ERational.Abs());

                /// <summary>
                /// Tries to find a pair of two <see cref="Integer"/>s
                /// (which are components that make up a <see cref="Rational"/>)
                /// so that its rational value is equal to <paramref name="num"/>.
                /// To set some options for this function, you can use
                /// <see cref="MathS.Settings.MaxAbsNumeratorOrDenominatorValue"/>
                /// to limit the absolute value of both denominator and numerator.
                /// </summary>
                /// <param name="num">
                /// e.g. 1.5m -> 3/2
                /// </param>
                /// <param name="iterCount">
                /// Number of iterations allowed to be spent for searching the rational.
                /// A higher value indicates a higher probability that it will find a <see cref="Rational"/>.
                /// Defaults to <see cref="MathS.Settings.FloatToRationalIterCount"/>.
                /// </param>
                /// <returns>
                /// <see cref="Rational"/> if found, <see langword="null"/> otherwise.
                /// </returns>
                public static Rational? FindRational(EDecimal num, int iterCount = int.MinValue)
                {
                    if (iterCount is int.MinValue)
                        iterCount = MathS.Settings.FloatToRationalIterCount;
                    if (iterCount <= 0)
                        return null;
                    if (!num.IsFinite)
                        return null;
                    var sign = num.Sign;
                    num *= sign;
                    var (intPart, rest) = num.SplitDecimal();
                    if (intPart > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                        return null;
                    if (IsZero(rest))
                        return Integer.Create(sign * intPart);
                    else
                    {
                        var inv = CtxDivide(EDecimal.One, rest);
                        var rat = FindRational(inv, iterCount - 1);
                        if (rat is null)
                            return null;
                        return new Rational((intPart * sign + sign / rat.ERational).ToLowestTerms());
                    }
                }

                internal static bool TryParse(string s,
                    [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Rational? dst)
                {
                    try
                    {
                        dst = ERational.FromString(s);
                        return true;
                    }
                    catch
                    {
                        dst = null;
                        return false;
                    }
                }

#pragma warning disable CS1591
                public static bool operator >(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) > 0;
                public static bool operator >=(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) >= 0;
                public static bool operator <(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) < 0;
                public static bool operator <=(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) <= 0;
                public int CompareTo(Rational other) => ERational.CompareTo(other.ERational);
                public static Rational operator +(Rational a, Rational b) => OpSum(a, b);
                public static Rational operator -(Rational a, Rational b) => OpSub(a, b);
                public static Rational operator *(Rational a, Rational b) => OpMul(a, b);
                public static Real operator /(Rational a, Rational b) => (Real)OpDiv(a, b);
                public static Rational operator +(Rational a) => a;
                public static Rational operator -(Rational a) => OpMul(Integer.MinusOne, a);
                public static implicit operator Rational(sbyte value) => Integer.Create(value);
                public static implicit operator Rational(byte value) => Integer.Create(value);
                public static implicit operator Rational(short value) => Integer.Create(value);
                public static implicit operator Rational(ushort value) => Integer.Create(value);
                public static implicit operator Rational(int value) => Integer.Create(value);
                public static implicit operator Rational(uint value) => Integer.Create(value);
                public static implicit operator Rational(long value) => Integer.Create(value);
                public static implicit operator Rational(ulong value) => Integer.Create(value);
                public static implicit operator Rational(EInteger value) => Integer.Create(value);
                public static implicit operator Rational(ERational value) => Create(value);
#pragma warning restore CS1591
            }
        }
    }
}
