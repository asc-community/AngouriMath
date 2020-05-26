namespace AngouriMath.Core.Numerix
{
    public partial class IntegerNumber : RationalNumber
    {
        public new IntegerNumber Copy()
            => Number.Copy(this) as IntegerNumber;
        public new long Value { get; protected set; }
        private void InitClass(long value, UndefinedState state)
        {
            Value = value;
            Type = HierarchyLevel.INTEGER;
            State = state;
            Init();
        }
        public IntegerNumber(long value)
        {
            InitClass(value, UndefinedState.DEFINED);
        }

        private IntegerNumber()
        {

        }

        public IntegerNumber(Number number)
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
                dst = new IntegerNumber(res);
                return true;
            }
            dst = null;
            return false;
        }
    }
}
