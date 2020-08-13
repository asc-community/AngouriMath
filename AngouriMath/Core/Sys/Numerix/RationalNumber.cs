
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

using PeterO.Numbers;

namespace AngouriMath.Core.Numerix
{
	/// <summary>
	/// Constructor does not downcast automatically.
	/// Use <see cref="Create(EInteger, EInteger)"/> or <see cref="Create(ERational)"/> for automatic downcasting.
	/// The denominator cannot be zero as the resulting value will not be a rational
	/// </summary>
    public record RationalNumber(ERational Rational)
	    : RealNumber(Rational.ToEDecimal(MathS.Settings.DecimalPrecisionContext)), System.IComparable<RationalNumber>
    {
        public override Priority Priority => Priority.Div;
        public override bool IsExact => true;
        public static RationalNumber Create(EInteger numerator, EInteger denominator) =>
            Create(ERational.Create(numerator, denominator));
        public static RationalNumber Create(ERational value) {
            if (!value.IsFinite)
                throw new System.ArgumentException("Non-finite values are not rationals - use RealNumber.Create instead");

            if (!MathS.Settings.DowncastingEnabled)
                return new RationalNumber(value.ToLowestTerms());

            // Call ToLowestTerms() through new RationalNumber first
            // before determining whether the denominator equals one
            var @return = new RationalNumber(value.ToLowestTerms());
            if (@return.Rational.Denominator.Equals(1))
                return IntegerNumber.Create(@return.Rational.Numerator);
            else
                return @return;
        }
        public void Deconstruct(out int? numerator, out int? denominator)
        {
            numerator = Rational.Numerator.CanFitInInt32() ? Rational.Numerator.ToInt32Unchecked() : new int?();
            denominator = Rational.Denominator.CanFitInInt32() ? Rational.Denominator.ToInt32Unchecked() : new int?();
        }

        // TODO: When we target .NET 5, remember to use covariant return types
        public override RealNumber Abs() => Create(Rational.Abs());

        /// <summary>
        /// Tries to find a pair of two <see cref="IntegerNumber"/>s
        /// (which are components that make up a <see cref="RationalNumber"/>)
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
        /// A higher value indicates a higher probability that it will find a <see cref="RationalNumber"/>.
        /// Defaults to <see cref="MathS.Settings.FloatToRationalIterCount"/>.
        /// </param>
        /// <returns>
        /// <see cref="RationalNumber"/> if found, <see langword="null"/> otherwise.
        /// </returns>
        public static RationalNumber? FindRational(EDecimal num, int iterCount = int.MinValue)
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
                return IntegerNumber.Create(sign * intPart);
            else
            {
                var inv = CtxDivide(EDecimal.One, rest);
                var rat = FindRational(inv, iterCount - 1);
                if (rat is null)
                    return null;
                return new RationalNumber((intPart * sign + sign / rat.Rational).ToLowestTerms());
            }
        }
        internal override string Stringize() => Rational.ToString();
        public override string Latexise() => $@"\frac{{{Rational.Numerator}}}{{{Rational.Denominator}}}";
        internal static bool TryParse(string s,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out RationalNumber? dst)
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

        public static bool operator >(RationalNumber a, RationalNumber b) => a.Rational.CompareTo(b.Rational) > 0;
        public static bool operator >=(RationalNumber a, RationalNumber b) => a.Rational.CompareTo(b.Rational) >= 0;
        public static bool operator <(RationalNumber a, RationalNumber b) => a.Rational.CompareTo(b.Rational) < 0;
        public static bool operator <=(RationalNumber a, RationalNumber b) => a.Rational.CompareTo(b.Rational) <= 0;
        public int CompareTo(RationalNumber other) => Rational.CompareTo(other.Rational);
        public static RationalNumber operator +(RationalNumber a, RationalNumber b) => OpSum(a, b);
        public static RationalNumber operator -(RationalNumber a, RationalNumber b) => OpSub(a, b);
        public static RationalNumber operator *(RationalNumber a, RationalNumber b) => OpMul(a, b);
        public static RealNumber operator /(RationalNumber a, RationalNumber b) => (RealNumber)OpDiv(a, b);
        public static RationalNumber operator +(RationalNumber a) => a;
        public static RationalNumber operator -(RationalNumber a) => OpMul(IntegerNumber.MinusOne, a);
        public static bool operator ==(RationalNumber a, RationalNumber b) => AreEqual(a, b);
        public static bool operator !=(RationalNumber a, RationalNumber b) => !AreEqual(a, b);
        public static implicit operator RationalNumber(sbyte value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(byte value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(short value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(ushort value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(int value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(uint value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(long value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(ulong value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(EInteger value) => IntegerNumber.Create(value);
        public static implicit operator RationalNumber(ERational value) => Create(value);
    }
}
