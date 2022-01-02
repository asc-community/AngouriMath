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
        public partial record Sinf
        {
            internal override string ToSymPy() => "sympy.sin(" + Argument.ToSymPy() + ")";
        }

        public partial record Cosf
        {
            internal override string ToSymPy() => "sympy.cos(" + Argument.ToSymPy() + ")";
        }

        public partial record Secantf
        {
            internal override string ToSymPy() => "sympy.sec(" + Argument.ToSymPy() + ")";
        }

        public partial record Cosecantf
        {
            internal override string ToSymPy() => "sympy.csc(" + Argument.ToSymPy() + ")";
        }

        public partial record Tanf
        {
            internal override string ToSymPy() => "sympy.tan(" + Argument.ToSymPy() + ")";
        }

        public partial record Cotanf
        {
            internal override string ToSymPy() => "sympy.cot(" + Argument.ToSymPy() + ")";
        }

        public partial record Arcsinf
        {
            internal override string ToSymPy() => "sympy.asin(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccosf
        {
            internal override string ToSymPy() => "sympy.acos(" + Argument.ToSymPy() + ")";
        }

        public partial record Arctanf
        {
            internal override string ToSymPy() => "sympy.atan(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccotanf
        {
            internal override string ToSymPy() => "sympy.acot(" + Argument.ToSymPy() + ")";
        }

        public partial record Arcsecantf
        {
            internal override string ToSymPy() => "sympy.asec(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccosecantf
        {
            internal override string ToSymPy() => "sympy.acsc(" + Argument.ToSymPy() + ")";
        }
    }
}
