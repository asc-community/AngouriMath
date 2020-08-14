
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

using System.Runtime.CompilerServices;
using PeterO.Numbers;

[assembly: InternalsVisibleTo("DotnetBenchmark")]
namespace AngouriMath.Core.Numerix
{
    public record RealNumber : ComplexNumber, System.IComparable<RealNumber>
    {
        /// <summary>
        /// Constructor does not downcast automatically. Use <see cref="Create(EDecimal)"/> for automatic downcasting.
        /// </summary>
        private protected RealNumber(EDecimal @decimal) : base(null, null) => Decimal = @decimal;
        public EDecimal Decimal { get; }
        public void Deconstruct(out EDecimal @decimal) => @decimal = Decimal;
        public override RealNumber Real => this;
        public override Priority Priority => Decimal.IsNegative ? Priority.Mul : Priority.Number;
        public override bool IsExact => !Decimal.IsFinite;
        public bool IsNegative => Decimal.IsNegative;
        public bool IsPositive => !Decimal.IsNegative && !Decimal.IsZero;
        public static RealNumber Create(EDecimal value)
        {
            if (!MathS.Settings.DowncastingEnabled)
                return new RealNumber(value);

            if (!value.IsFinite)
                return new RealNumber(value);
            var (intPart, intRest) = value.SplitDecimal();
            // If the difference between value & round(value) is zero (see Number.IsZero), we consider value as an integer
            if (intRest.LessThan(MathS.Settings.PrecisionErrorZeroRange))
                return IntegerNumber.Create(intPart);
            if (intRest.GreaterThan(1 - MathS.Settings.PrecisionErrorZeroRange.Value))
                return IntegerNumber.Create(intPart.Increment());

            var attempt = RationalNumber.FindRational(value);
            if (attempt is null ||
                attempt.Rational.Numerator.Abs() > MathS.Settings.MaxAbsNumeratorOrDenominatorValue ||
                attempt.Rational.Denominator.Abs() > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                return new RealNumber(value);
            else
                return attempt;
        }

        public override RealNumber Abs() => Create(Decimal.Abs());

        internal override string Stringize() =>
            Decimal.IsFinite ? Decimal.ToString()
            : Decimal.IsPositiveInfinity() ? "+oo"
            : Decimal.IsNegativeInfinity() ? "-oo"
            : Decimal.IsNaN() ? "NaN"
            : throw new UniverseCollapseException();

        public override string Latexise() =>
            Decimal.IsFinite ? Decimal.ToString()
            : Decimal.IsPositiveInfinity() ? @"\infty "
            : Decimal.IsNegativeInfinity() ? @"-\infty "
            : Decimal.IsNaN() ? @"\mathrm{undefined}"
            : throw new UniverseCollapseException();

        internal static bool TryParse(string s,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RealNumber? dst)
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

        /// <summary>
        /// Creates an instance of Negative Infinity RealNumber (-oo)
        /// </summary>
        /// <returns></returns>
        public static readonly RealNumber NegativeInfinity = new RealNumber(EDecimal.NegativeInfinity);

        /// <summary>
        /// Creates an instance of Positive Infinity RealNumber (+oo)
        /// </summary>
        /// <returns></returns>
        public static readonly RealNumber PositiveInfinity = new RealNumber(EDecimal.PositiveInfinity);

        /// <summary>
        /// Creates an instance of Not A Number RealNumber (NaN)
        /// </summary>
        /// <returns></returns>
        public static readonly RealNumber NaN = new RealNumber(EDecimal.NaN);
        public static bool operator >(RealNumber a, RealNumber b) => a.Decimal.GreaterThan(b.Decimal);
        public static bool operator >=(RealNumber a, RealNumber b) => a.Decimal.GreaterThanOrEquals(b.Decimal);
        public static bool operator <(RealNumber a, RealNumber b) => a.Decimal.LessThan(b.Decimal);
        public static bool operator <=(RealNumber a, RealNumber b) => a.Decimal.LessThanOrEquals(b.Decimal);
        public int CompareTo(RealNumber other) => Decimal.CompareTo(other.Decimal);
        public static RealNumber operator +(RealNumber a, RealNumber b) => OpSum(a, b);
        public static RealNumber operator -(RealNumber a, RealNumber b) => OpSub(a, b);
        public static RealNumber operator *(RealNumber a, RealNumber b) => OpMul(a, b);
        public static RealNumber operator /(RealNumber a, RealNumber b) => (RealNumber)OpDiv(a, b);
        public static RealNumber operator +(RealNumber a) => a;
        public static RealNumber operator -(RealNumber a) => OpMul(IntegerNumber.MinusOne, a);
        public static bool operator ==(RealNumber a, RealNumber b) => AreEqual(a, b);
        public static bool operator !=(RealNumber a, RealNumber b) => !AreEqual(a, b);
        public static implicit operator RealNumber(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(byte value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(short value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(ushort value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(int value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(uint value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(long value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(ulong value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator RealNumber(ERational value) => RationalNumber.Create(value);
        public static implicit operator RealNumber(EDecimal value) => Create(value);
        public static implicit operator RealNumber(float value) => Create(EDecimal.FromSingle(value));
        public static implicit operator RealNumber(double value) => Create(EDecimal.FromDouble(value));
        public static implicit operator RealNumber(decimal value) => Create(EDecimal.FromDecimal(value));
    }
}
