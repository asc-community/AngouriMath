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
                                    Number { Priority: Priority.Sum } and not Complex { RealPart.IsZero: false, ImaginaryPart.IsZero: false } =>
                                        currIn.Latexise(false),
                                    _ => currIn.Latexise(currIn.Priority < Priority)
                                };
                            case 1:
                                if (longArray[index - 1] is Integer(-1))
                                    return $"-{currIn.Latexise(currIn.Priority < Priority)}"; // display "-1 * x * y" as "-x \cdot y", only for the first -1
                                break;
                        }
                        var currOut = currIn.Latexise(currIn.Priority < Priority);

                        return (longArray[index - 1], currIn) switch // whether we use juxtaposition and omit \cdot
                        {
                            // NOTE: upright text are to be interpreted as a whole while italic text are to be interpreted as individual characters.
                            // Therefore, constants formatted as upright text, and multi-character variables are not considered for juxtaposition.

                            // Don't juxtapose upright variables with numbers like displaying "var2" for "var*2" since "var2" may be interpreted as one variable.
                            // Also, don't produce upright "ei" (one variable with two chars) for e*i, or "ei^2" for e*i^2.
                            // but "e (2+i)" and "e (2+i)^2" are fine with the parentheses - so we have the priority check.
                            (Variable { IsLatexUprightFormatted: true }
                                or Complex { ImaginaryPart.IsZero: false, Priority: >= Priority.Mul } /* don't combine upright "i" with an upright variable*/,
                             Variable { IsLatexUprightFormatted: true } or Number { Priority: >= Priority.Mul }
                                or Factorialf(Number { Priority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true })
                                or Powf(Number { Priority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true }
                                        or Factorialf(Number { Priority: Priority.Leaf } or Variable { IsLatexUprightFormatted: true }), _)) => false,
                            // 2 * (3/4) instead of 2 (3/4) which is a mixed number (= 2 + 3/4)
                            (Number { Priority: Priority.Leaf }, { Priority: Priority.Div }) => false,
                            // 2 * 3 instead of 2 3 (= 23), 2 * 3^4 instead of 2 3^4 (= 23^4), but "(2+i) 2", "2 (2+i)" and "2 (2+i)^2" are fine with the parentheses - so we have the priority check.
                            (_, Number { Priority: >= Priority.Mul } or Factorialf(Number { Priority: Priority.Leaf })
                                or Powf(Number { Priority: Priority.Leaf } or Factorialf(Number { Priority: Priority.Leaf }), _)) => false, // Keep the \cdot in "f(x) \cdot -2" "f(x) \cdot 2i" "f(x) \cdot -2i"
                            (var left, var right) => left.Priority >= right.Priority &&
                                !(left.Priority == Priority.Div && right.Priority == Priority.Div) // Without \cdot, the fraction lines may appear too closely together.
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
            // NOTE: \operatorname is used here to distinguish the phi function from variables, consistent with sgn and other functions.
            public override string Latexise() => $@"\operatorname{{\varphi}}\left({Argument.Latexise()}\right)";
        }
    }
}
