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
        partial record Number
        {
            partial record Complex
            {
                /// <inheritdoc/>
                private protected override string LatexizeNode()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.Latexize();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Latexize();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + @"\mathrm{i}"; // Display i upright per ISO 80000-2.
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.Latexize() + " " + sign + " " +
                        (im == 1 ? "" : im.Latexize(ImaginaryPart is Rational and not Integer)) + @"\mathrm{i}";
                }
            }

            partial record Real
            {
                /// <inheritdoc/>
                private protected override string LatexizeNode() => this switch
                {
                    { IsFinite: true } => EDecimal.ToString(),
                    { IsNaN: true } => @"\mathrm{undefined}",
                    { IsNegative: true } => @"-\infty ",
                    _ => @"\infty ",
                };
            }

            partial record Rational
            {
                /// <inheritdoc/>
                private protected override string LatexizeNode() => $@"\frac{{{ERational.Numerator}}}{{{ERational.Denominator}}}";

            }

            partial record Integer
            {
                /// <inheritdoc/>
                private protected override string LatexizeNode() => EInteger.ToString();
            }
        }
    }
}