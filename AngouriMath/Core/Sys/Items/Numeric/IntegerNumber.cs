
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
    public partial class IntegerNumber : RationalNumber, System.IEquatable<IntegerNumber>
    {

        public static readonly IntegerNumber Zero = new IntegerNumber(EInteger.Zero);
        public static readonly IntegerNumber One = new IntegerNumber(EInteger.One);
        public static readonly IntegerNumber MinusOne = new IntegerNumber(-EInteger.One);
        /// <summary>Exact value of the number</summary>
        public new EInteger Value { get; }

        /// <summary>Use <see cref="Create(EInteger)"/> for consistency with
        /// <see cref="RationalNumber"/>, <see cref="RealNumber"/> and <see cref="ComplexNumber"/>.</summary>
        private protected IntegerNumber(EInteger value) : base(value) => Value = value;
        public static IntegerNumber Create(EInteger value) => new IntegerNumber(value);

        // TODO: Use C# 9 Covariant return types
        public override RealNumber Abs() => Create(Value.Abs());

        protected internal override string InternalToString() => InternalToStringDefinition(Value.ToString());
        protected internal override string InternalLatexise()
            => InternalLatexiseDefinition(Value.ToString());
        internal static bool TryParse(string s,
            [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out IntegerNumber? dst)
        {
            try
            {
                dst = EInteger.FromString(s);
                return true;
            }
            catch
            {
                dst = null;
                return false;
            }
        }
        public static bool operator >(IntegerNumber a, IntegerNumber b) => a.Value.CompareTo(b.Value) > 0;
        public static bool operator >=(IntegerNumber a, IntegerNumber b) => a.Value.CompareTo(b.Value) >= 0;
        public static bool operator <(IntegerNumber a, IntegerNumber b) => a.Value.CompareTo(b.Value) < 0;
        public static bool operator <=(IntegerNumber a, IntegerNumber b) => a.Value.CompareTo(b.Value) <= 0;
        public static IntegerNumber operator +(IntegerNumber a, IntegerNumber b) => OpSum(a, b);
        public static IntegerNumber operator -(IntegerNumber a, IntegerNumber b) => OpSub(a, b);
        public static IntegerNumber operator *(IntegerNumber a, IntegerNumber b) => OpMul(a, b);
        public static RationalNumber operator /(IntegerNumber a, IntegerNumber b) => (RationalNumber)OpDiv(a, b);
        public static IntegerNumber operator +(IntegerNumber a) => a;
        public static IntegerNumber operator -(IntegerNumber a) => OpMul(MinusOne, a);
        public static bool operator ==(IntegerNumber a, IntegerNumber b) => AreEqual(a, b);
        public static bool operator !=(IntegerNumber a, IntegerNumber b) => !AreEqual(a, b);
        public override bool Equals(object other) => other is IntegerNumber num && Equals(num);
        public bool Equals(IntegerNumber other) => AreEqual(this, other);
        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator IntegerNumber(sbyte value) => Create(value);
        public static implicit operator IntegerNumber(byte value) => Create(value);
        public static implicit operator IntegerNumber(short value) => Create(value);
        public static implicit operator IntegerNumber(ushort value) => Create(value);
        public static implicit operator IntegerNumber(int value) => Create(value);
        public static implicit operator IntegerNumber(uint value) => Create(value);
        public static implicit operator IntegerNumber(long value) => Create(value);
        public static implicit operator IntegerNumber(ulong value) => Create(value);
        public static implicit operator IntegerNumber(EInteger value) => Create(value);
    }
}
