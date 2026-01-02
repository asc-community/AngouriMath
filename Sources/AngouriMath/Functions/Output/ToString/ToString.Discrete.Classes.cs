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
            public override string Stringize() => ((bool)this).ToString();
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"not {Argument.Stringize(Argument.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} and {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} or {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} xor {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Assumption.Stringize(Assumption.Priority < Priority)} implies {Conclusion.Stringize(Conclusion.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} = {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} > {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} >= {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} < {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} <= {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
