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
            private protected override string StringizeNode() => ((bool)this).ToString();
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Notf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode() => $"not {Argument.Stringize(Argument.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Andf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority < Priority)} and {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Orf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority < Priority)} or {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority < Priority)} xor {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            // The grammar folds `implies` to the left, like everything but `^` and `provided`, so
            // it is the *conclusion* that needs bracketing when it is an implication of its own.
            // The two are not interchangeable: `false implies (true implies false)` is true, and
            // `(false implies true) implies false` is false, so printing the first without its
            // brackets said the opposite of what it meant.
            private protected override string StringizeNode()
                => $"{Assumption.Stringize(Assumption.Priority < Priority)} implies {Conclusion.Stringize(Conclusion.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority <= Priority)} = {Right.Stringize(Right.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority <= Priority)} > {Right.Stringize(Right.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority <= Priority)} >= {Right.Stringize(Right.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority <= Priority)} < {Right.Stringize(Right.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            private protected override string StringizeNode()
                => $"{Left.Stringize(Left.Priority <= Priority)} <= {Right.Stringize(Right.Priority <= Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
