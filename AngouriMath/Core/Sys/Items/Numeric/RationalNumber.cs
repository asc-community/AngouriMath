
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
    public partial class RationalNumber : RealNumber, System.IEquatable<RationalNumber>
    {
        /// <summary>Exact value of the number</summary>
        public new ERational Value { get; }

        /// <summary>
        /// Does not downcast automatically.
        /// Use <see cref="Create(EInteger, EInteger)"/> or <see cref="Create(ERational)"/> for automatic downcasting
        /// </summary>
        private protected RationalNumber(ERational value)
            // Will throw if denominator is zero (zero denominators are not rational)
            : base(value.ToEDecimal(MathS.Settings.DecimalPrecisionContext)) => Value = value.ToLowestTerms();
        public static RationalNumber Create(EInteger numerator, EInteger denominator) =>
            Create(new ERational(numerator, denominator));
        public static RationalNumber Create(ERational value) {
            if (!MathS.Settings.DowncastingEnabled)
                return new RationalNumber(value);

            // Call ToLowestTerms() through new RationalNumber first
            // before determining whether the denominator equals one
            var @return = new RationalNumber(value);
            if (value.IsFinite && @return.Value.Denominator.Equals(1))
                return IntegerNumber.Create(@return.Value.Numerator);
            else
                return @return;
        }

        // TODO: Use C# 9 Covariant return types
        public override RealNumber Abs() => Create(Value.Abs());

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
            var intPart = num.ToEInteger();
            if (intPart > MathS.Settings.MaxAbsNumeratorOrDenominatorValue)
                return null;
            var rest = CtxSubtract(num, intPart);
            if (IsZero(rest))
                return IntegerNumber.Create(sign * intPart);
            else
            {
                var inv = CtxDivide(EDecimal.One, rest);
                var rat = FindRational(inv, iterCount - 1);
                if (rat is null)
                    return null;
                return new RationalNumber(intPart * sign + sign / rat.Value);
            }
        }
        protected internal override string InternalToString() => InternalToStringDefinition(Value.ToString());
        protected internal override string InternalLatexise()
            => InternalLatexiseDefinition($@"\frac{{{Value.Numerator}}}{{{Value.Denominator}}}");
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

        public static bool operator >(RationalNumber a, RationalNumber b) => a.Value.CompareTo(b.Value) > 0;
        public static bool operator >=(RationalNumber a, RationalNumber b) => a.Value.CompareTo(b.Value) >= 0;
        public static bool operator <(RationalNumber a, RationalNumber b) => a.Value.CompareTo(b.Value) < 0;
        public static bool operator <=(RationalNumber a, RationalNumber b) => a.Value.CompareTo(b.Value) <= 0;
        public static RationalNumber operator +(RationalNumber a, RationalNumber b) => OpSum(a, b);
        public static RationalNumber operator -(RationalNumber a, RationalNumber b) => OpSub(a, b);
        public static RationalNumber operator *(RationalNumber a, RationalNumber b) => OpMul(a, b);
        public static RationalNumber operator /(RationalNumber a, RationalNumber b) => (RationalNumber)OpDiv(a, b);
        public static RationalNumber operator +(RationalNumber a) => a;
        public static RationalNumber operator -(RationalNumber a) => OpMul(IntegerNumber.MinusOne, a);
        public static bool operator ==(RationalNumber a, RationalNumber b) => AreEqual(a, b);
        public static bool operator !=(RationalNumber a, RationalNumber b) => !AreEqual(a, b);
        public override bool Equals(object other) => other is RationalNumber num && Equals(num);
        public bool Equals(RationalNumber other) => AreEqual(this, other);
        public override int GetHashCode() => Value.GetHashCode();
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
