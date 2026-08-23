//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;
using HonkSharp.Laziness;
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
#pragma warning disable SealedOrAbstract // The only few exceptions: Complex, Real, Rational
            public partial record Rational : Real, System.IComparable<Rational>
#pragma warning restore SealedOrAbstract // AMAnalyzer
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
                public Integer Numerator => numerator.GetValue(static @this => @this.ERational.Numerator, this);
                private LazyPropertyA<Integer> numerator;

                /// <summary>
                /// A getter for the denominator
                /// </summary>
                public Integer Denominator => denominator.GetValue(static @this => @this.ERational.Denominator, this);
                private LazyPropertyA<Integer> denominator;

                /// <summary>Takes the ratio out, so that <c>var (r) = rational;</c> works.</summary>
                /// <param name="rational">The value as an <see cref="ERational"/>.</param>
                public void Deconstruct(out ERational rational) => rational = ERational;

                /// <summary>
                /// Takes the two parts out, so that <c>var (num, den) = rational;</c> works.
                /// They are in lowest terms and the denominator is positive, since that is the
                /// only form a <see cref="Rational"/> is ever built in.
                /// </summary>
                /// <param name="numerator">The numerator.</param>
                /// <param name="denominator">The denominator.</param>
                public void Deconstruct(out Integer numerator, out Integer denominator)
                {
                    numerator = Numerator;
                    denominator = Denominator;
                }

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
                /// <exception cref="InvalidNumberException">Thrown if </exception>
                public static Rational Create(ERational value)
                {
                    if (!value.IsFinite)
                        throw new InvalidNumberException(
                            $"{value} is not finite, and only a finite value is a rational - "
                            + $"use {nameof(Real)}.{nameof(Real.Create)} instead");

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
                    // Whether the continued fraction has terminated is a question about the
                    // digits of intPart, so the tolerance is relative to it. Number.IsZero
                    // compares against PrecisionErrorZeroRange outright, which at the top of
                    // the recursion -- where intPart is 0 and rest is the whole number --
                    // declared every value below 1e-16 to be the integer 0. That is how
                    // 1e-20 came to parse as 0. Below the top, intPart is at least 1 (rest
                    // was inverted to get here), so the test stays as permissive as it was.
                    if (rest.Abs().LessThan(EDecimal.FromEInteger(intPart)
                            .Multiply(MathS.Settings.PrecisionErrorZeroRange, MathS.Settings.DecimalPrecisionContext)))
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

                // Comparison here is on the value and answers a bool, where the same operators
                // on Entity build an inequality node instead.

                /// <summary>Whether the first is strictly greater.</summary>
                public static bool operator >(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) > 0;

                /// <summary>Whether the first is greater or they are equal.</summary>
                public static bool operator >=(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) >= 0;

                /// <summary>Whether the first is strictly less.</summary>
                public static bool operator <(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) < 0;

                /// <summary>Whether the first is less or they are equal.</summary>
                public static bool operator <=(Rational a, Rational b) => a.ERational.CompareTo(b.ERational) <= 0;

                /// <summary>
                /// Negative, zero or positive as this is less than, equal to or greater than
                /// <paramref name="other"/>, which is what sorting wants.
                /// </summary>
                /// <exception cref="System.ArgumentNullException">
                /// Thrown where <paramref name="other"/> is <see langword="null"/>, rather than
                /// sorting it first as <see cref="System.IComparable{T}"/> usually would.
                /// </exception>
                public int CompareTo(Rational? other) => other is null ? throw new System.ArgumentNullException() : ERational.CompareTo(other.ERational);

                /// <summary>Their sum, exactly.</summary>
                public static Rational operator +(Rational a, Rational b) => OpSum(a, b);

                /// <summary>Their difference, exactly.</summary>
                public static Rational operator -(Rational a, Rational b) => OpSub(a, b);

                /// <summary>Their product, exactly.</summary>
                public static Rational operator *(Rational a, Rational b) => OpMul(a, b);

                /// <summary>
                /// Their quotient. <see cref="Real"/> rather than <see cref="Rational"/>,
                /// because dividing by zero has to go somewhere and the answer is
                /// <see cref="Real.NaN"/>, which is not a ratio. Every other quotient of two
                /// ratios is a ratio and arrives as one.
                /// </summary>
                public static Real operator /(Rational a, Rational b) => (Real)OpDiv(a, b);

                /// <summary>The operand itself; unary plus changes nothing.</summary>
                public static Rational operator +(Rational a) => a;

                /// <summary>Its negation.</summary>
                public static Rational operator -(Rational a) => OpMul(Integer.MinusOne, a);
                
                /// <summary>
                /// The floored remainder, which takes the sign of the divisor: -7/2 % 3 is 5/2
                /// and -7/2 % (-3) is -1/2.
                /// See https://github.com/asc-community/AngouriMath/issues/708.
                /// </summary>
                /// <remarks>
                /// Adding the divisor whenever the truncated remainder came out negative is the
                /// right conversion only where the divisor is positive; for a negative one it
                /// moved the answer further from zero, so (-7/2) % (-3) came back as -7/2 --
                /// larger in magnitude than the divisor, and a remainder under no convention.
                /// </remarks>
                public static Rational operator %(Rational a, Rational b)
                    => a.ERational.Remainder(b.ERational)
                        .Alias(out var truncated)
                        .IsZero || truncated.IsNegative == b.ERational.IsNegative
                        ? truncated
                        : truncated + b;
                // As elsewhere, downcasting is on by default and a whole value arrives as an
                // Integer, so the runtime type is narrower than the declared one.

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(sbyte value) => (long)value;

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(byte value) => (ulong)value;

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(short value) => (long)value;

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(ushort value) => (ulong)value;

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(int value) => (long)value;

                /// <summary>The number as a <see cref="Rational"/>.</summary>
                public static implicit operator Rational(uint value) => (ulong)value;

                /// <summary>
                /// The number as a <see cref="Rational"/> — an <see cref="Integer"/> at runtime
                /// while downcasting is enabled.
                /// </summary>
                public static implicit operator Rational(long value)
                    => MathS.Settings.DowncastingEnabled
                        ? Integer.Create(value)
                        : new Rational(value);

                /// <summary>
                /// The number as a <see cref="Rational"/> — an <see cref="Integer"/> at runtime
                /// while downcasting is enabled.
                /// </summary>
                public static implicit operator Rational(ulong value)
                    => MathS.Settings.DowncastingEnabled
                        ? Integer.Create(value)
                        : new Rational(value);

                /// <summary>
                /// The integer as a <see cref="Rational"/> over one — an <see cref="Integer"/>
                /// at runtime while downcasting is enabled.
                /// </summary>
                public static implicit operator Rational(EInteger value)
                    => MathS.Settings.DowncastingEnabled
                        ? Integer.Create(value)
                        : new Rational(ERational.FromEInteger(value));

                /// <summary>
                /// The ratio as a <see cref="Rational"/>, in lowest terms and with a positive
                /// denominator: <c>2/4</c> arrives as <c>1/2</c> and <c>1/(-2)</c> as
                /// <c>(-1)/2</c>.
                /// </summary>
                public static implicit operator Rational(ERational value) => Create(value);
            }
        }
    }
}
