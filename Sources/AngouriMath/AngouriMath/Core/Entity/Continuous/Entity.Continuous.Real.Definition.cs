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
                /// The PeterO number representation in decimal
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
                    if (intRest.LessThan(MathS.Settings.PrecisionErrorZeroRange))
                        return Integer.Create(intPart);
                    if (intRest.GreaterThan(1 - MathS.Settings.PrecisionErrorZeroRange.Value))
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

#pragma warning disable CS1591
                public static bool operator >(Real a, Real b) => a.EDecimal.GreaterThan(b.EDecimal);
                public static bool operator >=(Real a, Real b) => a.EDecimal.GreaterThanOrEquals(b.EDecimal);
                public static bool operator <(Real a, Real b) => a.EDecimal.LessThan(b.EDecimal);
                public static bool operator <=(Real a, Real b) => a.EDecimal.LessThanOrEquals(b.EDecimal);
                public int CompareTo(Real? other) => other is null ? throw new System.ArgumentNullException() : EDecimal.CompareTo(other.EDecimal);
                public static Real operator +(Real a, Real b) => OpSum(a, b);
                public static Real operator -(Real a, Real b) => OpSub(a, b);
                public static Real operator *(Real a, Real b) => OpMul(a, b);
                public static Real operator /(Real a, Real b) => OpDiv(a, b).Downcast<Real>();
                public static Real operator +(Real a) => a;
                public static Real operator -(Real a) => OpMul(Integer.MinusOne, a);
                public static Real operator %(Real a, Real b) => a.EDecimal.Remainder(b.EDecimal, MathS.Settings.DecimalPrecisionContext);
                public static implicit operator Real(sbyte value) => (long)value;
                public static implicit operator Real(byte value) => (ulong)value;
                public static implicit operator Real(short value) => (long)value;
                public static implicit operator Real(ushort value) => (ulong)value;
                public static implicit operator Real(int value) => (long)value;
                public static implicit operator Real(uint value) => (ulong)value;
                public static implicit operator Real(long value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);
                public static implicit operator Real(ulong value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);
                public static implicit operator Real(EInteger value) => MathS.Settings.DowncastingEnabled ? Integer.Create(value) : new Real(value);
                public static implicit operator Real(ERational value) => MathS.Settings.DowncastingEnabled ? Rational.Create(value) : Create(value.ToEDecimal(MathS.Settings.DecimalPrecisionContext));
                public static implicit operator Real(EDecimal value) => Create(value);
                public static implicit operator Real(float value) => Create(EDecimal.FromSingle(value));
                public static implicit operator Real(double value) => Create(EDecimal.FromDouble(value));
                public static implicit operator Real(decimal value) => Create(EDecimal.FromDecimal(value));
#pragma warning restore CS1591

            }
        }
    }
}
