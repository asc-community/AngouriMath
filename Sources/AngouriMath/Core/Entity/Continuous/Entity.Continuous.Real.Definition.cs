//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {

        partial record Number
        {
            /// <summary>
            /// Represents a real number, such complex
            /// that its imaginary part equals 0
            /// </summary>
#pragma warning disable SealedOrAbstract // The only few exceptions: Complex, Real, Rational
            public partial record Real : Complex, System.IComparable<Real>
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <summary>
                /// Constructor does not downcast automatically. Use <see cref="Create(EDecimal)"/> for automatic downcasting.
                /// </summary>
                private protected Real(EDecimal @decimal) : base(null, null) => EDecimal = @decimal;

                /// <summary>
                /// The PeterO number representation in <see cref="PeterO.Numbers.EDecimal"/>
                /// </summary>
                public EDecimal EDecimal { get; }

                /// <summary>
                /// Deconstructs as record
                /// </summary>
                public void Deconstruct(out EDecimal @decimal) => @decimal = EDecimal;

                /// <inheritdoc/>
                public override Real RealPart => this;
                internal override Priority Priority => EDecimal.IsNegative ? Priority.Sum : Priority.Leaf;

                /// <inheritdoc/>
                public override bool IsExact => !EDecimal.IsFinite;

                /// <summary>Strictly less than 0</summary>
                public bool IsNegative => EDecimal.IsNegative;

                /// <summary>Strictly greater than 0</summary>
                public bool IsPositive => !EDecimal.IsNegative && !EDecimal.IsZero;

                /// <summary>
                /// Creates an instance of Real
                /// (one can do it by implicit conversation)
                /// </summary>
                public static Real Create(EDecimal value)
                {
                    if (!MathS.Settings.DowncastingEnabled)
                        return new Real(value);

                    if (!value.IsFinite)
                        return new Real(value);
                    var (intPart, intRest) = value.SplitDecimal();
                    // If the difference between value & round(value) is zero (see Number.IsZero), we consider value as an integer
                    var tolerance = MathS.Settings.DowncastingTolerance;
                    if (intRest.LessThan(tolerance))
                        return Integer.Create(intPart);
                    if (intRest.GreaterThan(1 - tolerance))
                        return Integer.Create(intPart.Increment());

                    var attempt = Rational.FindRational(value);
                    if (attempt is null ||
                        attempt.ERational.Numerator.Abs() > MathS.Settings.MaxAbsNumeratorOrDenominatorValue ||
                        attempt.ERational.Denominator.Abs() > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                        return new Real(value);
                    else
                        return attempt;
                }

                /// <inheritdoc/>
                public override Real Abs() => Create(EDecimal.Abs());

                internal static bool TryParse(string s,
                    [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Real? dst)
                {
                    try
                    {
                        dst = EDecimal.FromString(s);
                        return true;
                    }
                    catch
                    {
                        dst = null;
                        return false;
                    }
                }

                /// <summary>Negative Infinity (-oo)</summary>
                [ConstantField] public static readonly Real NegativeInfinity = new Real(EDecimal.NegativeInfinity);

                /// <summary>Positive Infinity (+oo)</summary>
                [ConstantField] public static readonly Real PositiveInfinity = new Real(EDecimal.PositiveInfinity);

                /// <summary>Not A Number (NaN)</summary>
                [ConstantField] public static readonly Real NaN = new Real(EDecimal.NaN);

                /// <summary>
                /// Converts the given number to a double (not recommended in general unless you need a built-in type)
                /// </summary>
                public double AsDouble() => EDecimal.ToDouble();

                // Comparison here is on the value and answers a bool, unlike the operators on
                // Entity, which build an inequality node to be solved or evaluated later.
                //
                // These order NaN rather than refusing to: NaN sorts above every number, so
                // NaN > 1 and NaN >= 1 are both true and 1 < NaN is true as well. That is a
                // total order and it is not what double does, where every comparison against
                // NaN is false. Measured at +1, -1 and NaN rather than assumed, because the
                // IEEE habit is the one a reader arrives with.

                /// <summary>
                /// Whether the first is strictly greater. <see cref="NaN"/> counts as greater
                /// than every number, so <c>NaN &gt; x</c> holds for any real <c>x</c>.
                /// </summary>
                public static bool operator >(Real a, Real b) => a.EDecimal.GreaterThan(b.EDecimal);

                /// <summary>Whether the first is greater or they are equal.</summary>
                public static bool operator >=(Real a, Real b) => a.EDecimal.GreaterThanOrEquals(b.EDecimal);

                /// <summary>
                /// Whether the first is strictly less. <see cref="NaN"/> is less than nothing,
                /// and every number is less than it.
                /// </summary>
                public static bool operator <(Real a, Real b) => a.EDecimal.LessThan(b.EDecimal);

                /// <summary>Whether the first is less or they are equal.</summary>
                public static bool operator <=(Real a, Real b) => a.EDecimal.LessThanOrEquals(b.EDecimal);

                /// <summary>
                /// Negative, zero or positive as this is less than, equal to or greater than
                /// <paramref name="other"/>, which is what sorting wants.
                /// </summary>
                /// <exception cref="System.ArgumentNullException">
                /// Thrown where <paramref name="other"/> is <see langword="null"/>. Unlike
                /// <see cref="System.IComparable{T}"/>'s usual contract, null does not sort first
                /// here; it is refused.
                /// </exception>
                public int CompareTo(Real? other) => other is null ? throw new System.ArgumentNullException() : EDecimal.CompareTo(other.EDecimal);

                /// <summary>Their sum, computed.</summary>
                public static Real operator +(Real a, Real b) => OpSum(a, b);

                /// <summary>Their difference, computed.</summary>
                public static Real operator -(Real a, Real b) => OpSub(a, b);

                /// <summary>Their product, computed.</summary>
                public static Real operator *(Real a, Real b) => OpMul(a, b);

                /// <summary>
                /// Their quotient, computed. Division by zero gives <see cref="NaN"/> rather
                /// than throwing, so a quotient always has an answer of some kind.
                /// </summary>
                public static Real operator /(Real a, Real b) => OpDiv(a, b).Downcast<Real>();

                /// <summary>The operand itself; unary plus changes nothing.</summary>
                public static Real operator +(Real a) => a;

                /// <summary>Its negation, computed.</summary>
                public static Real operator -(Real a) => OpMul(Integer.MinusOne, a);
                /// <summary>
                /// The floored remainder, which takes the sign of the divisor: -7 % 3 is 2 and
                /// 7 % (-3) is -2. See https://github.com/asc-community/AngouriMath/issues/708.
                /// </summary>
                /// <remarks>
                /// This one used to truncate and so took the sign of the dividend, disagreeing
                /// with the same operator on <see cref="Integer"/> and on
                /// <see cref="Rational"/> -- which one applied depended on the static type at
                /// the call site rather than on the values.
                /// </remarks>
                public static Real operator %(Real a, Real b)
                    => a.EDecimal.Remainder(b.EDecimal, MathS.Settings.DecimalPrecisionContext)
                        .Alias(out var truncated)
                        .IsZero || truncated.IsNegative == b.EDecimal.IsNegative
                        ? truncated
                        : truncated.Add(b.EDecimal, MathS.Settings.DecimalPrecisionContext);
                // With MathS.Settings.DowncastingEnabled, which is the default, a value that is
                // exactly whole arrives as an Integer and one that is exactly a ratio as a
                // Rational. The declared type is Real either way, so the difference is in the
                // runtime type and in how the number prints, not in the signature.

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(sbyte value) => (long)value;

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(byte value) => (ulong)value;

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(short value) => (long)value;

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(ushort value) => (ulong)value;

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(int value) => (long)value;

                /// <summary>The number as a <see cref="Real"/>.</summary>
                public static implicit operator Real(uint value) => (ulong)value;

                /// <summary>
                /// The number as a <see cref="Real"/> — an <see cref="Integer"/> at runtime
                /// while downcasting is enabled.
                /// </summary>
                public static implicit operator Real(long value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);

                /// <summary>
                /// The number as a <see cref="Real"/> — an <see cref="Integer"/> at runtime
                /// while downcasting is enabled.
                /// </summary>
                public static implicit operator Real(ulong value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);

                /// <summary>
                /// The integer as a <see cref="Real"/> — an <see cref="Integer"/> at runtime
                /// while downcasting is enabled.
                /// </summary>
                public static implicit operator Real(EInteger value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);

                /// <summary>
                /// The ratio as a <see cref="Real"/>. While downcasting is enabled it stays a
                /// <see cref="Rational"/> and so stays exact; with it off the ratio is evaluated
                /// to the working precision and a third becomes a finite decimal.
                /// </summary>
                public static implicit operator Real(ERational value) => MathS.Settings.DowncastingEnabled ? Rational.Create(value) : Create(value.ToEDecimal(MathS.Settings.DecimalPrecisionContext));

                /// <summary>The decimal as a <see cref="Real"/>.</summary>
                public static implicit operator Real(EDecimal value) => Create(value);

                /// <summary>
                /// The value as a <see cref="Real"/>, read as the binary <see langword="float"/>
                /// it is — so <c>0.1f</c> becomes the value that literal holds, not one tenth.
                /// </summary>
                public static implicit operator Real(float value) => Create(EDecimal.FromSingle(value));

                /// <summary>
                /// The value as a <see cref="Real"/>, read as the binary
                /// <see langword="double"/> it is rather than as the decimal it was written as.
                /// </summary>
                public static implicit operator Real(double value) => Create(EDecimal.FromDouble(value));

                /// <summary>
                /// The value as a <see cref="Real"/>. A <see langword="decimal"/> is decimal
                /// already, so this one keeps the digits that were written.
                /// </summary>
                public static implicit operator Real(decimal value) => Create(EDecimal.FromDecimal(value));
            }
        }
    }
}
