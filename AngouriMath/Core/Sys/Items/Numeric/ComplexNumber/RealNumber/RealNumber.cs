
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
using System.Globalization;

namespace AngouriMath.Core.Numerix
{
    public partial class RealNumber : ComplexNumber
    {
        public new RealNumber Copy()
            => Number.Copy(this) as RealNumber;

        /// <summary>
        /// Not only a real value can be assigned to a RealNumber
        /// Create a real number of undefined state with Number.Create(RealNumber.UndefinedState)
        /// </summary>
        public enum UndefinedState : byte
        {
            DEFINED,
            POSITIVE_INFINITY,
            NEGATIVE_INFINITY,
            NAN
        }

        /// <summary>
        /// The exact value of the type decimal
        /// </summary>
        public new decimal Value { get; protected set; }

        /// <summary>
        /// To check whether one is defined, you may write
        /// this.IsDefinite()
        /// Or check directly, this.State == RealNumber.UndefinedState.DEFINED
        /// </summary>
        public UndefinedState State { get; protected set; }
        private void InitClass(decimal value, UndefinedState state)
        {
            Value = value;
            Type = HierarchyLevel.REAL;
            State = state;
            switch (State)
            {
                case UndefinedState.NEGATIVE_INFINITY:
                    Value = decimal.MinValue;
                    break;
                case UndefinedState.POSITIVE_INFINITY:
                    Value = decimal.MaxValue;
                    break;
                case UndefinedState.NAN:
                    Value = 0;
                    break;
            }
            Init();
        }

        /// <summary>
        /// Use Number.Create(RealNumber.UndefinedState) instead
        /// </summary>
        /// <param name="state"></param>
        internal RealNumber(UndefinedState state)
        {
            if (state == UndefinedState.DEFINED)
                throw new InvalidOperationException("You cannot define undefined number with defined state");
            InitClass(0, state);
        }

        /// <summary>
        /// Use Number.Create(decimal) instead
        /// </summary>
        /// <param name="value"></param>
        internal RealNumber(decimal value)
        {
            InitClass(value, UndefinedState.DEFINED);
        }

        protected RealNumber()
        {
            
        }

        /// <summary>
        /// Use Number.Copy(Number) instead, or this.Copy()
        /// </summary>
        /// <param name="number"></param>
        internal RealNumber(Number number)
        {
            number = Functional.Downcast(number);
            if (number.Is(HierarchyLevel.REAL))
                InitClass((number as RealNumber).Value, (number as RealNumber).State);
            else
                throw new InvalidNumberCastException(number.Type, HierarchyLevel.REAL);
        }

        protected internal new void Init()
        {
            this.Real = this;
            this.Imaginary = Zero();
            base.Init();
        }

        protected override (decimal Re, decimal Im) GetValue()
        {
            return (Value, 0);
        }

        public static RealNumber Zero()
        {
            var ra = new RealNumber();
            ra.Value = 0;
            ra.Real = ra;
            ra.Imaginary = ra;
            ra.Type = HierarchyLevel.REAL;
            ra.State = UndefinedState.DEFINED;
            (ra as ComplexNumber).Init();
            return ra;
        }

        protected internal string InternalToStringDefinition(string str)
        {
            if (IsDefinite())
                return str;
            else
                return State switch
                {
                    UndefinedState.NEGATIVE_INFINITY => "-oo",
                    UndefinedState.POSITIVE_INFINITY => "+oo",
                    UndefinedState.NAN => "NaN"
                };
        }

        protected internal string InternalLatexiseDefinition(string str)
        {
            if (IsDefinite())
                return str;
            else
                return State switch
                {
                    UndefinedState.NEGATIVE_INFINITY => @"-\infty",
                    UndefinedState.POSITIVE_INFINITY => @"+\infty",
                    UndefinedState.NAN => @"NaN"
                };
        }

        protected internal new string InternalToString()
            => InternalToStringDefinition(Value.ToString(CultureInfo.InvariantCulture));

        protected internal new string InternalLatexise()
            => InternalLatexiseDefinition(Value.ToString(CultureInfo.InvariantCulture));


        internal static bool TryParse(string s, out RealNumber dst)
        {
            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var res))
            {
                dst = new RealNumber(res);
                return true;
            }
            dst = null;
            return false;
        }

        /// <summary>
        /// Checks whether one's state is defined meaning that it could be safely used for calculations
        /// </summary>
        /// <returns></returns>
        public bool IsDefinite()
            => State == UndefinedState.DEFINED;

        /// <summary>
        /// Creates an instance of Negative Infinity RealNumber (-oo)
        /// </summary>
        /// <returns></returns>
        public static RealNumber NegativeInfinity()
            => new RealNumber(UndefinedState.NEGATIVE_INFINITY);

        /// <summary>
        /// Creates an instance of Positive Infinity RealNumber (+oo)
        /// </summary>
        /// <returns></returns>
        public static RealNumber PositiveInfinity()
            => new RealNumber(UndefinedState.POSITIVE_INFINITY);

        /// <summary>
        /// Creates an instance of Not A Number RealNumber (NaN)
        /// </summary>
        /// <returns></returns>
        public static RealNumber NaN()
            => new RealNumber(UndefinedState.NAN);
    }
}
