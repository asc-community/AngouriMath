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
            /// <inheritdoc/>
            public override string Stringize() =>
                Augend.Stringize(Augend.Priority < Priority) + " + " + Addend.Stringize(Addend.Priority < Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Minusf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Minuend.Stringize(Minuend.Priority < Priority) + " - " + Subtrahend.Stringize(Subtrahend.Priority <= Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Mulf
        {
            /// <inheritdoc/>
            // Multiplication is associative, so a product or a quotient on the right may stay
            // unbracketed -- but `mod` shares this precedence level and is not associative, so a
            // `mod` on the right must be bracketed or it is re-read as the outer operator:
            // `2 * (3 mod 2)` is 2 and `(2 * 3) mod 2` is 0.
            public override string Stringize() =>
                (Multiplier is Integer(-1) && !MathS.Diagnostic.OutputExplicit ? "-"
                    : Multiplier.Stringize(Multiplier.Priority < Priority) + " * ")
                + Multiplicand.Stringize(Multiplicand.Priority < Priority || Multiplicand is Modf);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Divf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Dividend.Stringize(Dividend.Priority < Priority) + " / " + Divisor.Stringize(Divisor.Priority <= Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Modf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Dividend.Stringize(Dividend.Priority < Priority) + " mod " + Divisor.Stringize(Divisor.Priority <= Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Logf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => Base == MathS.e ?
                $"ln({Antilogarithm.Stringize()})"
                :
                $"log({Base.Stringize()}, {Antilogarithm.Stringize()})";

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Powf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Exponent == 0.5m
                ? "sqrt(" + Base.Stringize() + ")"
                // The base takes <=, the exponent takes <, which is the mirror of the rule the
                // left-associative operators above use: ^ groups to the right, so it is the
                // *left* operand that needs bracketing when it is a power of its own.
                // (2 ^ 3) ^ 2 printed as 2 ^ 3 ^ 2 before, which is 512 where the expression
                // printed is 64.
                : Base.Stringize(Base.Priority <= Priority) + " ^ " + Exponent.Stringize(Exponent.Priority < Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Phif
        {
            /// <inheritdoc/>
            public override string Stringize() => $@"phi({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Signumf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"sgn({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Absf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"abs({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Floorf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"floor({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Ceilf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"ceil({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Roundf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"round({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Minf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"min({Left.Stringize()}, {Right.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Maxf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"max({Left.Stringize()}, {Right.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Gcdf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"gcd({Left.Stringize()}, {Right.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Factorialf
        {
            /// <inheritdoc/>
            public override string Stringize() => Argument.Stringize(Argument.Priority <= Priority) + "!";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
