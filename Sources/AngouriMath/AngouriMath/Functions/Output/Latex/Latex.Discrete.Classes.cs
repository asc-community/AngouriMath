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
            /// <inheritdoc/>
            public override string Latexise() => $@"\operatorname{{{(bool)this}}}";
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"\neg{{{Argument.Latexise(Argument.Priority < Priority)}}}";
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \land {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \lor {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \oplus {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Assumption.Latexise(Assumption.Priority < Priority)} \implies {Conclusion.Latexise(Conclusion.Priority < Priority)}";
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} = {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} > {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \geqslant {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} < {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \leqslant {Right.Latexise(Right.Priority < Priority)}";
        }
    }
}
