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
                Minuend.ToSymPy(Minuend.Priority < Priority.Minus) + " - " + Subtrahend.ToSymPy(Subtrahend.Priority <= Priority.Minus);
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

        public partial record Modf
        {
            internal override string ToSymPy() =>
                "sympy.Mod(" + Dividend.ToSymPy() + ", " + Divisor.ToSymPy() + ")";
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

        public partial record Floorf
        {
            internal override string ToSymPy()
                => $@"sympy.floor({Argument.ToSymPy()})";
        }

        public partial record Ceilf
        {
            // SymPy spells it in full; `ceil` is not a name it has.
            internal override string ToSymPy()
                => $@"sympy.ceiling({Argument.ToSymPy()})";
        }

        public partial record Roundf
        {
            /// <remarks>
            /// SymPy has no symbolic round -- `RoundFunction` is an abstract base and
            /// raises, and `.round()` is a method on a concrete number rather than a
            /// function of an expression. So this is built out of what SymPy does have.
            ///
            /// It is deliberately not `sympy.floor(x + 1/2)`, which is the obvious
            /// translation and is wrong at every tie: that sends 1/2 to 1 and 5/2 to 3,
            /// where rounding half to even gives 0 and 2. The correction below is exactly
            /// the tie case -- when the fractional part is a half and the candidate is odd,
            /// step down to the even neighbour -- and was checked against SymPy's own
            /// `Rational.round()` on ties, non-ties and negatives.
            /// </remarks>
            internal override string ToSymPy()
            {
                var arg = Argument.ToSymPy();
                var candidate = $"sympy.floor({arg} + sympy.Rational(1, 2))";
                return $"sympy.Piecewise(({candidate} - 1, "
                     + $"sympy.Eq(sympy.frac({arg}), sympy.Rational(1, 2)) & sympy.Eq(sympy.Mod({candidate}, 2), 1)), "
                     + $"({candidate}, True))";
            }
        }

        public partial record Minf
        {
            internal override string ToSymPy()
                => $@"sympy.Min({Left.ToSymPy()}, {Right.ToSymPy()})";
        }

        public partial record Maxf
        {
            internal override string ToSymPy()
                => $@"sympy.Max({Left.ToSymPy()}, {Right.ToSymPy()})";
        }

        public partial record Gcdf
        {
            internal override string ToSymPy()
                => $@"sympy.gcd({Left.ToSymPy()}, {Right.ToSymPy()})";
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
