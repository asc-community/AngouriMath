/* 
 * Copyright (c) 2019-2021 Angouri.
 * AngouriMath is licensed under MIT. 
 * Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
 * Website: https://am.angouri.org.
 */
using PeterO.Numbers;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Sumf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                Augend.Latexise(Augend.Priority < Priority)
                + (Addend.Latexise(Addend.Priority < Priority) is var addend && addend.StartsWith("-")
                    ? addend : "+" + addend);
        }

        public partial record Minusf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                Subtrahend.Latexise(Subtrahend.Priority < Priority)
                + "-" + Minuend.Latexise(Minuend.Priority <= Priority);
        }

        public partial record Mulf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => (Multiplier, Multiplicand) switch
                {
                    (Integer(-1), Complex n) => (-n).Latexise(Multiplier.Priority < Priority),
                    (Number a, Number b) => $@"{a.Latexise(a.Priority < Priority)} \cdot {b.Latexise(b.Priority < Priority)}",
                    (var mp, var md) => $"{mp.Latexise(mp.Priority < Priority)} {md.Latexise(md.Priority < Priority)}"
                };
        }

        public partial record Divf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                @"\frac{" + Dividend.Latexise() + "}{" + Divisor.Latexise() + "}";
        }

        public partial record Logf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                Base == 10
                ? @"\log\left(" + Antilogarithm.Latexise() + @"\right)"
                : Base == MathS.e
                ? @"\ln\left(" + Antilogarithm.Latexise() + @"\right)"
                : @"\log_{" + Base.Latexise() + @"}\left(" + Antilogarithm.Latexise() + @"\right)";
        }

        public partial record Powf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                if (Exponent is Rational { ERational: { Numerator: var numerator, Denominator: var denominator } }
                    and not Integer)
                {
                    var str =
                        @"\sqrt" + (denominator.Equals(2) ? "" : "[" + denominator + "]")
                        + "{" + Base.Latexise() + "}";
                    var abs = numerator.Abs();
                    if (!abs.Equals(EInteger.One))
                        str += "^{" + abs + "}";
                    if (numerator < 0)
                        str = @"\frac{1}{" + str + "}";
                    return str;
                }
                else
                {
                    return "{" + Base.Latexise(Base.Priority <= Priority) + "}^{" + Exponent.Latexise() + "}";
                }
            }
        }

        public partial record Factorialf
        {
            /// <inheritdoc/>
            public override string Latexise() =>
                Argument.Latexise(Argument.Priority <= Priority) + "!";
        }

        partial record Signumf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"\operatorname{{sgn}}\left({Argument.Latexise()}\right)";
        }

        partial record Absf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"\left|{Argument.Latexise()}\right|";
        }

        partial record Phif
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"\varphi({Argument.Latexise()})";
        }
    }
}
