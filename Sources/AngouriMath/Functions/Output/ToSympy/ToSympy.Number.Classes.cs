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
            partial record Complex
            {
                internal override string ToSymPy()
                {
                    if (ImaginaryPart == 0)
                        return RealPart.ToSymPy();
                    if (RealPart == 0)
                        return $"{ImaginaryPart.ToSymPy()} * sympy.I";
                    return $"{RealPart.ToSymPy()} + {ImaginaryPart.ToSymPy()} * sympy.I";
                }
            }

            partial record Real
            {
                internal override string ToSymPy()
                    => Stringize();
            }

            partial record Rational
            {
                internal override string ToSymPy()
                    => $"sympy.Rational({Numerator.ToSymPy()}, {Denominator.ToSymPy()}";
            }

            partial record Integer
            {
                internal override string ToSymPy()
                    => Stringize();
            }
        }
    }
}
