using System;

namespace AngouriMath.Core.Numeric
{
    public partial class RationalNumber : RealNumber
    {
        public IntegerNumber Numerator { get; protected set; }
        public IntegerNumber Denominator { get; protected set; }
        private void InitClass(IntegerNumber numerator, IntegerNumber denominator)
        {
            Numerator = numerator;
            Denominator = denominator;
            Type = HierarchyLevel.RATIONAL;
            Init();
        }
        public RationalNumber(IntegerNumber numerator, IntegerNumber denominator)
        {
            InitClass(numerator, denominator);
        }

        protected RationalNumber()
        {

        }

        public RationalNumber(Number number)
        {
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
                this.Value = (decimal)Numerator.Value / Denominator.Value;
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
