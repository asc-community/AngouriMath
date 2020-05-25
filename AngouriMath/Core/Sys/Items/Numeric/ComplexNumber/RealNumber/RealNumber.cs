using System;

namespace AngouriMath.Core.Numerix
{
    public partial class RealNumber : ComplexNumber
    {
        public new RealNumber Copy()
            => new RealNumber(this);
        public enum UndefinedState : System.Byte
        {
            DEFINED,
            POSITIVE_INFINITY,
            NEGATIVE_INFINITY,
            NAN
        }

        public new decimal Value { get; protected set; }
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

        public RealNumber(UndefinedState state)
        {
            if (state == UndefinedState.DEFINED)
                throw new InvalidOperationException("You cannot define undefined number with defined state");
            InitClass(0, state);
        }

        public RealNumber(decimal value)
        {
            InitClass(value, UndefinedState.DEFINED);
        }

        protected RealNumber()
        {
            
        }

        public RealNumber(Number number)
        {
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
            => InternalToStringDefinition(Value.ToString());

        protected internal new string InternalLatexise()
            => InternalLatexiseDefinition(Value.ToString());


        internal static bool TryParse(string s, out RealNumber dst)
        {
            if (decimal.TryParse(s, out var res))
            {
                dst = new RealNumber(res);
                return true;
            }
            dst = null;
            return false;
        }

        public bool IsDefinite()
            => State == UndefinedState.DEFINED;

        public static RealNumber NegativeInfinity()
            => new RealNumber(UndefinedState.NEGATIVE_INFINITY);

        public static RealNumber PositiveInfinity()
            => new RealNumber(UndefinedState.POSITIVE_INFINITY);

        public static RealNumber NaN()
            => new RealNumber(UndefinedState.NAN);
    }
}
