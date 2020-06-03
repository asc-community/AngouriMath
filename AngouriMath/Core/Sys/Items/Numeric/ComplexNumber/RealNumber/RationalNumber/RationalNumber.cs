
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

using System;

namespace AngouriMath.Core.Numerix
{
    public partial class RationalNumber : RealNumber
    {
        /// <summary>
        /// Safely copies the instance of a Number
        /// </summary>
        /// <returns></returns>
        public new RationalNumber Copy()
            => Number.Copy(this) as RationalNumber;

        /// <summary>
        /// Given a rational a / b, a is the Numerator of type IntegerNumber
        /// </summary>
        public IntegerNumber Numerator { get; protected set; }

        /// <summary>
        /// Given a rational a / b, b is the Denominator of type IntegerNumber
        /// </summary>
        public IntegerNumber Denominator { get; protected set; }
        private void InitClass(IntegerNumber numerator, IntegerNumber denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            Type = HierarchyLevel.RATIONAL;
            Init();
        }

        /// <summary>
        /// Use Number.Create(IntegerNumber, IntegerNumber) instead
        /// </summary>
        /// <param name="numerator"></param>
        /// <param name="denominator"></param>
        internal RationalNumber(IntegerNumber numerator, IntegerNumber denominator)
        {
            InitClass(numerator, denominator);
        }

        protected RationalNumber()
        {

        }

        /// <summary>
        /// Use Number.Copy(Number) or this.Copy() instead
        /// </summary>
        /// <param name="number"></param>
        internal RationalNumber(Number number)
        {
            number = Functional.Downcast(number);
            if (number.Is(HierarchyLevel.RATIONAL))
                InitClass((number as RationalNumber).Numerator, (number as RationalNumber).Denominator);
            else
                throw new InvalidNumberCastException(number.Type, HierarchyLevel.RATIONAL);
        }

        protected internal new void Init()
        {
            if (Denominator.Value < 0)
            {
                Numerator = new IntegerNumber(-Numerator.Value);
                Denominator = new IntegerNumber(-Denominator.Value);
            }
            if (Denominator.Value != 0)
                this.Value = (decimal)Numerator.Value / (decimal)Denominator.Value;
            else
            {
                if (Numerator.Value == 0)
                    State = UndefinedState.NAN;
                else if (Numerator.Value > 0)
                    State = UndefinedState.POSITIVE_INFINITY;
                else
                    State = UndefinedState.NEGATIVE_INFINITY;
            }
            base.Init();
        }

        protected internal new string InternalToString()
        {
            return InternalToStringDefinition(Numerator.ToString() + " / " + Denominator.ToString());
        }
        protected internal new string InternalLatexise()
            => InternalLatexiseDefinition(@"\frac{" + Numerator.Latexise() + "}{" + Denominator.Latexise() + "}");
        internal static bool TryParse(string s, out RationalNumber dst)
        {
            dst = null;
            var pos = s.IndexOf("/");
            if (pos == -1)
                return false;
            var numString = s.Substring(0, pos).Replace(" ", "");
            var denString = s.Substring(pos + 1).Replace(" ", "");
            if (IntegerNumber.TryParse(numString, out var num) && IntegerNumber.TryParse(denString, out var den))
            {
                dst = new RationalNumber(num, den);
                return true;
            }
            return false;
        }
    }
}
