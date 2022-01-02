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
        partial record Number
        {
            /// <summary>
            /// Extension for <see cref="Real"/>
            /// <a href="https://en.wikipedia.org/wiki/Complex_number"/>
            /// </summary>
#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Complex
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.Stringize();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Stringize();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + "i";
                    var (l, r) = ImaginaryPart is Rational and not Integer ? ("(", ")") : ("", "");
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.Stringize() + " " + sign + " " + l + RenderNum(im) + r + "i";
                }
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Real
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize() => this switch
                {
                    { IsFinite: true } => EDecimal.ToString(),
                    { IsNaN: true } => "NaN",
                    { IsNegative: true } => "-oo",
                    _ => "+oo",
                };
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

#pragma warning disable SealedOrAbstract // AMAnalyzer
            partial record Rational
#pragma warning restore SealedOrAbstract // AMAnalyzer
            {
                /// <inheritdoc/>
                public override string Stringize() => ERational.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Integer
            {
                /// <inheritdoc/>
                public override string Stringize() => EInteger.ToString();
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }
        }
    }
}
