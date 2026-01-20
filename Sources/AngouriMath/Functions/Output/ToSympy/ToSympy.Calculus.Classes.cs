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
            internal override string ToSymPy() => $"sympy.integrate({Expression.ToSymPy()}, {(Range is var (from, to) ? $"({Var.ToSymPy()}, {from.ToSymPy()}, {to.ToSymPy()})" : Var.ToSymPy())})";
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
