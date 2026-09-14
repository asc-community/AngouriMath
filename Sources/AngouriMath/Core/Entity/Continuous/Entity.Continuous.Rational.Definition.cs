//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System;
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
                    // The search below is a continued fraction in hundred-digit arithmetic, and
                    // it is run on the result of every arithmetic operation on a Real, so that
                    // 0.5 comes back as 1/2: twenty microseconds a time, most of the cost of a
                    // numerical evaluation (https://github.com/asc-community/AngouriMath/issues/1338).
                    // Almost every result is not a small rational, and that is decided first in
                    // System.Decimal, whose twenty-eight digits are enough to say so reliably;
                    // only a value the cheap search does not rule out reaches the exact one.
                    if (!MayBeASmallRational(num, iterCount))
                        return null;
                    return FindRationalExactly(num, iterCount);
                }

                /// <summary>The continued fraction in the decimal's own arithmetic, level by level.</summary>
                private static Rational? FindRationalExactly(EDecimal num, int iterCount)
                {
                    if (iterCount <= 0)
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
                        var rat = FindRationalExactly(inv, iterCount - 1);
                        if (rat is null)
                            return null;
                        return new Rational((intPart * sign + sign / rat.ERational).ToLowestTerms());
                    }
                }

                /// <summary>
                /// Whether <paramref name="num"/> may be within the exact search's tolerance of a
                /// rational with a numerator and a denominator within
                /// <see cref="MathS.Settings.MaxAbsNumeratorOrDenominatorValue"/>, decided by the
                /// same continued fraction in <see cref="double"/>. A <see langword="false"/> is
                /// reliable; a <see langword="true"/> is only a reason to run the exact search.
                /// </summary>
                /// <remarks>
                /// <para>
                /// The exact search accepts a level whose remainder is below the integer part
                /// times <see cref="MathS.Settings.PrecisionErrorZeroRange"/>, ten to the minus
                /// sixteen. In a double the remainder at a level carries an error of the value's
                /// representation error, two to the minus fifty-two, times the square of that
                /// level's convergent denominator; the tolerance here is the exact one widened by
                /// that bound, so a value the exact search would accept is never refused, and
                /// where the bound has grown past a tenth the double can no longer tell and the
                /// question is handed to the exact search. A false yes costs the exact search,
                /// which then refuses; it was what every value paid.
                /// </para>
                /// <para>
                /// The double is read off the mantissa's leading bits and the exponent directly:
                /// PeterO's own conversions to a double or a decimal round the whole
                /// hundred-digit mantissa and cost two to three microseconds, as much as the
                /// arithmetic they were meant to spare.
                /// </para>
                /// </remarks>
                internal static bool MayBeASmallRational(EDecimal num, int iterCount, double integerTolerance = 0)
                {
                    var bound = EDecimal.FromEInteger(MathS.Settings.MaxAbsNumeratorOrDenominatorValue.Value).ToDouble();
                    // In a double first, which is a third of a microsecond and settles most
                    // values; where the double's error has grown past deciding -- a value
                    // near a small rational, whose partial quotients are large early -- the
                    // same search in a decimal, whose twenty-eight digits keep the error below
                    // the tolerance for every denominator within the bound; and only then the
                    // exact one.
                    if (!TryReadAsDouble(num, out var value))
                        return true;
                    value = Math.Abs(value);
                    // Within the caller's tolerance of an integer -- Real.Create's, which
                    // rounds a residual of exact cancellation such as sin(pi)'s onto zero.
                    var nearestInteger = Math.Round(value);
                    if (Math.Abs(value - nearestInteger) <= integerTolerance)
                        return true;
                    var byDouble = ContinuedFractionMayTerminate(value, iterCount, bound, out var undecided);
                    if (!undecided)
                        return byDouble;
                    decimal asDecimal;
                    try { asDecimal = Math.Abs(num.ToDecimal()); }
                    catch (OverflowException) { return true; }
                    return ContinuedFractionMayTerminateInDecimal(asDecimal, iterCount, (decimal)bound);
                }

                /// <summary>
                /// The continued fraction of <paramref name="value"/> in a double: whether it may
                /// reach a level the exact search accepts within <paramref name="iterCount"/>
                /// levels and with every convergent denominator within <paramref name="bound"/>.
                /// <paramref name="undecided"/> is set where the double's error -- its
                /// representation error times the square of a convergent's denominator -- has
                /// grown past a tenth before the question was settled.
                /// </summary>
                private static bool ContinuedFractionMayTerminate(double value, int iterCount, double bound, out bool undecided)
                {
                    undecided = false;
                    if (value > 1e300)
                        return false;
                    // The convergents' denominators, k_n = a_n k_(n-1) + k_(n-2) from k_(-2) = 1 and
                    // k_(-1) = 0, to stop once they pass the bound; and the absolute error the
                    // level's remainder carries, the representation error of the value to begin
                    // with, divided by the square of each fractional part inverted on the way
                    // (the derivative of 1/f) and one rounding of each division. Checked with
                    // this epsilon on twelve thousand rationals within the bound and on as many
                    // values within ten to the minus ten of one against the exact search: none
                    // refused.
                    const double epsilon = 1e-15;   // the reading's error, two powers of ten and the double's own
                    double previousDenominator = 1, denominator = 0;
                    var rest = value;
                    var error = epsilon * value + 1e-300;
                    for (var i = 0; i < iterCount; i++)
                    {
                        var integral = Math.Floor(rest);
                        if (integral > bound)
                            return false;
                        var fractional = rest - integral;
                        var nextDenominator = integral * denominator + previousDenominator;
                        if (nextDenominator > bound)
                            return false;
                        if (error > 0.1)
                        {
                            undecided = true;
                            return true;
                        }
                        // Within the error of a remainder the exact search accepts, or of the
                        // next integer, where the true integral part is one more and the true
                        // remainder small.
                        if (fractional <= integral * 1e-16 + error || 1 - fractional <= error || (integral == 0 && fractional == 0))
                            return true;
                        error = error / (fractional * fractional) + epsilon / fractional;
                        (previousDenominator, denominator) = (denominator, nextDenominator);
                        rest = 1 / fractional;
                    }
                    return false;
                }

                /// <summary>The same in a decimal, whose error never grows past deciding within the bound.</summary>
                private static bool ContinuedFractionMayTerminateInDecimal(decimal value, int iterCount, decimal bound)
                {
                    decimal previousDenominator = 1, denominator = 0;
                    var rest = value;
                    for (var i = 0; i < iterCount; i++)
                    {
                        var integral = decimal.Floor(rest);
                        if (integral > bound)
                            return false;
                        var fractional = rest - integral;
                        var nextDenominator = integral * denominator + previousDenominator;
                        if (nextDenominator > bound)
                            return false;
                        var error = nextDenominator * nextDenominator * 1e-27m;
                        if (fractional <= integral * 1e-16m + error || 1 - fractional <= error || (integral == 0 && fractional == 0))
                            return true;
                        if (fractional < 1e-27m)
                            return true;
                        (previousDenominator, denominator) = (denominator, nextDenominator);
                        rest = 1m / fractional;
                    }
                    return false;
                }

                /// <summary>
                /// <paramref name="num"/> as a double to within a few units in the last place,
                /// from the leading bits of its mantissa and its exponent, without rounding the
                /// whole mantissa; <see langword="false"/> where the value is outside a double's
                /// range.
                /// </summary>
                private static bool TryReadAsDouble(EDecimal num, out double value)
                {
                    value = 0;
                    if (!num.IsFinite)
                        return false;
                    var exponent = num.Exponent;
                    if (!exponent.CanFitInInt32())
                        return false;
                    var decimalExponent = exponent.ToInt32Unchecked();
                    var mantissa = num.UnsignedMantissa;
                    var bits = mantissa.GetUnsignedBitLengthAsInt64();
                    var shift = bits > 62 ? (int)(bits - 62) : 0;
                    var top = (double)(shift == 0 ? mantissa : mantissa.ShiftRight(shift)).ToInt64Checked();
                    // The binary shift and the decimal exponent applied in two halves each, in
                    // turn, since either alone can be far outside a double's range while the
                    // value is not -- a five-hundred-digit mantissa is shifted by sixteen
                    // hundred bits -- and a power of ten with a whole exponent is a unit or so
                    // in the last place where one with a fractional exponent is several.
                    var magnitude = decimalExponent + shift * 0.30102999566398120 + 18;
                    if (magnitude > 290 || magnitude < -290)
                        return false;
                    // Math.ScaleB is not in netstandard2.0; a power of two with a whole
                    // exponent is exact in a double, and past its range the value is not
                    // this filter's to decide.
                    var halfExponent = decimalExponent / 2;
                    var halfShift = shift / 2;
                    value = top * Math.Pow(10, halfExponent);
                    value *= Math.Pow(2, halfShift);
                    value *= Math.Pow(10, decimalExponent - halfExponent);
                    value *= Math.Pow(2, shift - halfShift);
                    if (num.IsNegative)
                        value = -value;
                    return !double.IsInfinity(value) && !double.IsNaN(value);
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
