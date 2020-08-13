
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
	/// <summary>Use <see cref="Create(EInteger)"/> instead of the constructor for consistency with
	/// <see cref="RationalNumber"/>, <see cref="RealNumber"/> and <see cref="ComplexNumber"/>.</summary>
    /// <param name="_">Ignored. It is here to disamiguate the <see cref="Deconstruct(out int)"/> method.</param>
    public record IntegerNumber(EInteger Integer, bool _ = false) : RationalNumber(Integer), System.IComparable<IntegerNumber>
    {
        public override Priority Priority => Priority.Num;
        public static readonly IntegerNumber Zero = new IntegerNumber(EInteger.Zero);
        public static readonly IntegerNumber One = new IntegerNumber(EInteger.One);
        public static readonly IntegerNumber MinusOne = new IntegerNumber(-EInteger.One);

        public static IntegerNumber Create(EInteger value) => new IntegerNumber(value);

        public void Deconstruct(out int? value) =>
            value = Integer.CanFitInInt32() ? Integer.ToInt32Unchecked() : new int?();

        // TODO: When we target .NET 5, remember to use covariant return types
        public override RealNumber Abs() => Create(Integer.Abs());

        internal override string Stringize() => Integer.ToString();
        public override string Latexise() => Integer.ToString();
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
        public static bool operator >(IntegerNumber a, IntegerNumber b) => a.Integer.CompareTo(b.Integer) > 0;
        public static bool operator >=(IntegerNumber a, IntegerNumber b) => a.Integer.CompareTo(b.Integer) >= 0;
        public static bool operator <(IntegerNumber a, IntegerNumber b) => a.Integer.CompareTo(b.Integer) < 0;
        public static bool operator <=(IntegerNumber a, IntegerNumber b) => a.Integer.CompareTo(b.Integer) <= 0;
        public int CompareTo(IntegerNumber other) => Integer.CompareTo(other.Integer);
        public static IntegerNumber operator +(IntegerNumber a, IntegerNumber b) => OpSum(a, b);
        public static IntegerNumber operator -(IntegerNumber a, IntegerNumber b) => OpSub(a, b);
        public static IntegerNumber operator *(IntegerNumber a, IntegerNumber b) => OpMul(a, b);
        public static RealNumber operator /(IntegerNumber a, IntegerNumber b) => (RealNumber)OpDiv(a, b);
        public static IntegerNumber operator +(IntegerNumber a) => a;
        public static IntegerNumber operator -(IntegerNumber a) => OpMul(MinusOne, a);
        public static bool operator ==(IntegerNumber a, IntegerNumber b) => AreEqual(a, b);
        public static bool operator !=(IntegerNumber a, IntegerNumber b) => !AreEqual(a, b);
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
