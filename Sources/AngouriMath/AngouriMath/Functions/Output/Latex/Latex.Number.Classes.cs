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
                public override string Latexise()
                {
                    static string RenderNum(Real number)
                    {
                        if (number == Integer.MinusOne)
                            return "-";
                        else if (number == Integer.One)
                            return "";
                        else
                            return number.Latexise();
                    }
                    if (ImaginaryPart is Integer(0))
                        return RealPart.Latexise();
                    else if (RealPart is Integer(0))
                        return RenderNum(ImaginaryPart) + "i";
                    var (im, sign) = ImaginaryPart > 0 ? (ImaginaryPart, "+") : (-ImaginaryPart, "-");
                    return RealPart.Latexise() + " " + sign + " " +
                        (im == 1 ? "" : im.Latexise(ImaginaryPart is Rational and not Integer)) + "i";
                }
            }

            partial record Real
            {
                /// <inheritdoc/>
                public override string Latexise() => this switch
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
                public override string Latexise() => $@"\frac{{{ERational.Numerator}}}{{{ERational.Denominator}}}";

            }

            partial record Integer
            {
                /// <inheritdoc/>
                public override string Latexise() => EInteger.ToString();
            }
        }

        

        

        
    }
}