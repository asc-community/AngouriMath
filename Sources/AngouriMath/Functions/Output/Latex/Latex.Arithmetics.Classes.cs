//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using PeterO.Numbers;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Sumf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                Augend.Latexize(Augend.LatexPriority < LatexPriority)
                + (Addend.Latexize(Addend.LatexPriority < LatexPriority) is var addend && addend.StartsWith("-")
                    ? addend : "+" + addend);
        }

        public partial record Minusf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                Minuend.Latexize(Minuend.LatexPriority < LatexPriority)
                + "-" + Subtrahend.Latexize(Subtrahend.LatexPriority <= LatexPriority);
        }

        public partial record Mulf
        {
            /// <inheritdoc/>
            public override string Latexize()
            {
                var longArray = GatherProducts(this).ToArray();
                return longArray.AggregateIndexed("",
                    (prevOut, index, currIn) =>
                    {
                        switch (index)
                        {
                            case 0:
                                return currIn switch
                                {
                                    // -1, -2, 2i, i, -i, -2i etc. in the front and not (1+i) etc.
                                    Number { LatexPriority: Priority.Sum } and not Complex { RealPart.IsZero: false, ImaginaryPart.IsZero: false } =>
                                        currIn.Latexize(false),
                                    _ => currIn.Latexize(currIn.LatexPriority < LatexPriority)
                                };
                            case 1:
                                if (longArray[index - 1] is Integer(-1))
                                    return $"-{currIn.Latexize(currIn.LatexPriority < LatexPriority || currIn is Modf)}"; // display "-1 * x * y" as "-x \cdot y", only for the first -1
                                break;
                        }
                        // A factor other than the first that is a `mod` needs brackets: \bmod sits
                        // at this precedence level and is not associative, so "x \cdot y \bmod z"
                        // reads as "(x y) mod z". 2 * (3 mod 2) is 2 and (2 * 3) mod 2 is 0.
                        var currOut = currIn.Latexize(currIn.LatexPriority < LatexPriority || currIn is Modf);

                        return (longArray[index - 1], currIn) switch // whether we use juxtaposition and omit \cdot
                        {
                            // NOTE: upright text are to be interpreted as a whole while italic text are to be interpreted as individual characters.
                            // Therefore, constants formatted as upright text, and multi-character variables are not considered for juxtaposition.

                            // Don't juxtapose upright variables with numbers like displaying "var2" for "var*2" since "var2" may be interpreted as one variable.
                            // Also, don't produce upright "ei" (one variable with two chars) for e*i, or "ei^2" for e*i^2.
                            // but "e (2+i)" and "e (2+i)^2" are fine with the parentheses - so we have the LatexPriority check.
                            (Variable { IsLatexUprightFormatted: true }
                                or Complex { ImaginaryPart.IsZero: false, LatexPriority: >= Priority.Mul } /* don't combine upright "i" with an upright variable*/,
                             Variable { IsLatexUprightFormatted: true } or Number { LatexPriority: >= Priority.Mul }
                                or Factorialf(Number { LatexPriority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true })
                                or Powf(Number { LatexPriority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true }
                                        or Factorialf(Number { LatexPriority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true }), _)) => false,
                            // 2 * (3/4) instead of 2 (3/4) which is a mixed number (= 2 + 3/4)
                            (Number { LatexPriority: Priority.Leaf }, { LatexPriority: Priority.Div }) => false,
                            // 2 * 3 instead of 2 3 (= 23), 2 * 3^4 instead of 2 3^4 (= 23^4), but "(2+i) 2", "2 (2+i)" and "2 (2+i)^2" are fine with the parentheses - so we have the LatexPriority check.
                            (_, Number { LatexPriority: >= Priority.Mul } or Factorialf(Number { LatexPriority: Priority.Leaf })
                                or Powf(Number { LatexPriority: Priority.Leaf } or Factorialf(Number { LatexPriority: Priority.Leaf }), _)) => false, // Keep the \cdot in "f(x) \cdot -2" "f(x) \cdot 2i" "f(x) \cdot -2i"
                            (var left, var right) => left.LatexPriority >= right.LatexPriority &&
                                !(left.LatexPriority == Priority.Div && right.LatexPriority == Priority.Div) // Without \cdot, the fraction lines may appear too closely together.
                        } ? $@"{prevOut} {currOut}" : $@"{prevOut} \cdot {currOut}";
                    });

                static IEnumerable<Entity> GatherProducts(Entity expr)
                    => expr switch
                    {
                        Mulf(var a, var b) => GatherProducts(a).Concat(GatherProducts(b)),
                        var other => [other]
                    };
            }
        }

        public partial record Divf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                @"\frac{" + Dividend.Latexize() + "}{" + Divisor.Latexize() + "}";
        }

        public partial record Modf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                Dividend.Latexize(Dividend.Priority < Priority) + @" \bmod " + Divisor.Latexize(Divisor.Priority <= Priority);
        }

        public partial record Logf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                Base == 10
                ? @"\log\left(" + Antilogarithm.Latexize() + @"\right)"
                : Base == MathS.e
                ? @"\ln\left(" + Antilogarithm.Latexize() + @"\right)"
                : @"\log_{" + Base.Latexize() + @"}\left(" + Antilogarithm.Latexize() + @"\right)";
        }

        public partial record Powf
        {
            /// <inheritdoc/>
            public override string Latexize()
            {
                if (Exponent is Rational { ERational: { Numerator: var numerator, Denominator: var denominator } }
                    and not Integer)
                {
                    var str =
                        @"\sqrt" + (denominator.Equals(2) ? "" : "[" + denominator + "]")
                        + "{" + Base.Latexize() + "}";
                    var abs = numerator.Abs();
                    if (!abs.Equals(EInteger.One))
                        str += "^{" + abs + "}";
                    if (numerator < 0)
                        str = @"\frac{1}{" + str + "}";
                    return str;
                }
                else
                {
                    return "{" + Base.Latexize(Base.LatexPriority <= LatexPriority) + "}^{" + Exponent.Latexize() + "}";
                }
            }
        }

        public partial record Factorialf
        {
            /// <inheritdoc/>
            public override string Latexize() =>
                Argument.Latexize(Argument.LatexPriority <= LatexPriority) + "!";
        }

        partial record Signumf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\operatorname{{sgn}}\left({Argument.Latexize()}\right)";
        }

        partial record Absf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\left|{Argument.Latexize()}\right|";
        }

        partial record Floorf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\left\lfloor{{{Argument.Latexize()}}}\right\rfloor";
        }

        partial record Ceilf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\left\lceil{{{Argument.Latexize()}}}\right\rceil";
        }

        partial record Roundf
        {
            // The nearest-integer brackets: a floor on the left, a ceiling on the right.
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\left\lfloor{{{Argument.Latexize()}}}\right\rceil";
        }

        partial record Minf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\min\left({Left.Latexize()}, {Right.Latexize()}\right)";
        }

        partial record Maxf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\max\left({Left.Latexize()}, {Right.Latexize()}\right)";
        }

        partial record Gcdf
        {
            /// <inheritdoc/>
            public override string Latexize()
                => $@"\gcd\left({Left.Latexize()}, {Right.Latexize()}\right)";
        }

        partial record Phif
        {
            /// <inheritdoc/>
            // NOTE: \operatorname is used here to distinguish the phi function from variables, consistent with sgn and other functions.
            public override string Latexize() => $@"\operatorname{{\varphi}}\left({Argument.Latexize()}\right)";
        }
    }
}
