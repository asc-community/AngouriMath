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
                return GatherProducts(this).ToArray() switch
                {
                    var onlyTwoElements when onlyTwoElements.Length == 2 =>
                        (onlyTwoElements[0], onlyTwoElements[1]) switch
                        {
                            (Integer(-1), Complex n) => (-n).Latexise(parenthesesRequired: false),
                            (Integer(-1), var other) => $"-{other.Latexise(other.Priority < Priority)}",
                            (Number a, Number b) => $@"{a.Latexise(a.Priority < Priority)} \cdot {b.Latexise(b.Priority < Priority)}",
                            (var mp, var md) when mp.Priority >= md.Priority => $@"{mp.Latexise(mp.Priority < Priority)} {md.Latexise(md.Priority < Priority)}",
                            (var mp, var md) => $@"{mp.Latexise(mp.Priority < Priority)} \cdot {md.Latexise(md.Priority < Priority)}"
                        },
                    var longArray => 
                        longArray.AggregateIndexed("",
                            (prevOut, index, currIn) => 
                            {
                                if (index == 0)
                                    return currIn.Latexise(currIn.Priority < Priority);
                                var currOut = currIn.Latexise(currIn.Priority < Priority);
                                return (longArray[index - 1], currIn) switch
                                {
                                    (var a, var b) when a.Priority == b.Priority => $@"{prevOut} {currOut}",
                                    (Variable, Variable) => $@"{prevOut} {currOut}",
                                    _ => $@"{prevOut} \cdot {currOut}"
                                };
                            })
                };

                static IEnumerable<Entity> GatherProducts(Entity expr)
                    => expr switch
                    {
                        Mulf(var a, var b) => GatherProducts(a).Concat(GatherProducts(b)),
                        var other => new[]{ other }
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
            public override string Latexise() => $@"\varphi({Argument.Latexise()})";
        }
    }
}
