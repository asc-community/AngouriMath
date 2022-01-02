//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Sumf
        {
            internal override string ToSymPy() =>
                Augend.ToSymPy(Augend.Priority < Priority.Sum) + " + " + Addend.ToSymPy(Addend.Priority < Priority.Sum);
        }

        public partial record Minusf
        {
            internal override string ToSymPy() =>
                Subtrahend.ToSymPy(Subtrahend.Priority < Priority.Minus) + " - " + Minuend.ToSymPy(Minuend.Priority <= Priority.Minus);
        }

        public partial record Mulf
        {
            internal override string ToSymPy() =>
                Multiplier.ToSymPy(Multiplier.Priority < Priority.Mul) + " * " + Multiplicand.ToSymPy(Multiplicand.Priority < Priority.Mul);
        }

        public partial record Divf
        {
            internal override string ToSymPy() =>
                Dividend.ToSymPy(Dividend.Priority < Priority.Div) + " / " + Divisor.ToSymPy(Divisor.Priority <= Priority.Div);
        }

        public partial record Logf
        {
            internal override string ToSymPy() => "sympy.log(" + Antilogarithm.ToSymPy() + ", " + Base.ToSymPy() + ")";
        }

        public partial record Powf
        {
            internal override string ToSymPy() =>
                Exponent == 0.5m
                ? "sympy.sqrt(" + Base.ToSymPy() + ")"
                : Base.ToSymPy(Base.Priority < Priority.Pow) + " ** " + Exponent.ToSymPy(Exponent.Priority < Priority.Pow);
        }

        public partial record Signumf
        {
            internal override string ToSymPy()
                => $@"sympy.sign({Argument.ToSymPy()})";
        }

        public partial record Absf
        {
            internal override string ToSymPy()
                => $@"sympy.Abs({Argument.ToSymPy()})";
        }

        public partial record Phif
        {
            internal override string ToSymPy() => $"sympy.totient({Argument.ToSymPy()})";
        }

        public partial record Factorialf
        {
            internal override string ToSymPy() => "sympy.factorial(" + Argument.ToSymPy() + ")";
        }
    }
}
