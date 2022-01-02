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
        partial record Boolean
        {
            internal override string ToSymPy()
                => this ? "True" : "False";
        }

        partial record Notf
        {
            internal override string ToSymPy()
                => $"not {Argument.ToSymPy(Argument.Priority < Priority)}";
        }

        partial record Andf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} and {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} or {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} ^ {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            internal override string ToSymPy()
                => $"sympy.Implies({Assumption.ToSymPy()}, {Conclusion.ToSymPy()})";
        }

        partial record Equalsf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} == {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} > {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} >= {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} < {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} <= {Right.ToSymPy(Right.Priority < Priority)}";
        }
    }
}
