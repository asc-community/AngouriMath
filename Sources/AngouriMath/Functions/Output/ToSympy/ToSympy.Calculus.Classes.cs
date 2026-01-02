//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        public partial record Derivativef
        {
            internal override string ToSymPy() => $"sympy.diff({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Integralf
        {
            // TODO: The 3rd parameter of sympy.integrate is not interpreted as iterations, unlike sympy.diff
            // which allows both sympy.diff(expr, var, iterations) and sympy.diff(expr, var1, var2, var3...)
            internal override string ToSymPy() => $"sympy.integrate({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Limitf
        {
            internal override string ToSymPy() =>
                @$"sympy.limit({Expression.ToSymPy()}, {Var.ToSymPy()}, {Destination.ToSymPy()}{ApproachFrom switch
                {
                    ApproachFrom.Left => ", '-'",
                    ApproachFrom.BothSides => "",
                    ApproachFrom.Right => ", '+'",
                    _ => throw new AngouriBugException
                        ($"Unresolved enum {ApproachFrom}")
                }})";
        }
    }
}
