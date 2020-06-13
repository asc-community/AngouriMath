
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

using System.Numerics;

namespace AngouriMath.Core.Numerix
{
    public partial class IntegerNumber : RationalNumber
    {
        /// <summary>
        /// Safely copies this
        /// </summary>
        /// <returns></returns>
        public new IntegerNumber Copy()
            => Number.Copy(this) as IntegerNumber;

        /// <summary>
        /// Exact value of the number
        /// </summary>
        public new BigInteger Value { get; internal set; }
        private void InitClass(BigInteger value, UndefinedState state)
        {
            Value = value;
            Type = HierarchyLevel.INTEGER;
            State = state;
            Init();
        }

        /// <summary>
        /// Use Number.Create(BigInteger) instead
        /// </summary>
        /// <param name="value"></param>
        internal IntegerNumber(BigInteger value)
        {
            InitClass(value, UndefinedState.DEFINED);
        }

        internal IntegerNumber()
        {

        }

        /// <summary>
        /// Use Number.Copy(Number) or this.Copy()
        /// </summary>
        /// <param name="number"></param>
        internal IntegerNumber(Number number)
        {
            number = Functional.Downcast(number);
            if (number.Is(HierarchyLevel.INTEGER))
                InitClass((number as IntegerNumber).Value, (number as RealNumber).State);
            else
                throw new InvalidNumberCastException(number.Type, HierarchyLevel.INTEGER);
        }

        protected internal new void Init()
        {
            Numerator = this;
            Denominator = One();
            base.Init();
        }

        public static bool operator <(IntegerNumber a, IntegerNumber b)
            => a.Value < b.Value;

        public static bool operator >(IntegerNumber a, IntegerNumber b)
            => a.Value > b.Value;

        public static bool operator <=(IntegerNumber a, IntegerNumber b)
            => a.Value <= b.Value;

        public static bool operator >=(IntegerNumber a, IntegerNumber b)
            => a.Value >= b.Value;

        public static IntegerNumber One()
        {
            var a = new IntegerNumber();
            a.Value = 1;
            a.Denominator = a;
            a.Numerator = a;
            ((RationalNumber) a).Init();
            return a;
        }

        protected internal new string InternalToString()
        {
            return InternalToStringDefinition(Value.ToString());
        }

        protected internal new string InternalLatexise()
            => InternalLatexiseDefinition(Value.ToString());
        internal static bool TryParse(string s, out IntegerNumber dst)
        {
            if (long.TryParse(s, out long res))
            {
                dst = Number.Create(res);
                return true;
            }
            dst = null;
            return false;
        }
    }
}
