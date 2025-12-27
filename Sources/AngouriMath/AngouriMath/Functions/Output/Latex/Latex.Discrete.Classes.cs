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
        // Standardize on the first notation in https://en.wikipedia.org/wiki/Truth_function#Table_of_binary_truth_functions
        partial record Boolean
        {
            /// <inheritdoc/>
            public override string Latexise() => (bool)this ? @"\top " : @"\bot ";
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
            // NOTE: \veebar (⊻) can disambiguate better than \oplus (⊕) which can mean the direct sum in algebra.
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \veebar {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            // NOTE:
            // \to (→) is an object-language symbol within the formal system being studied. It's the implication operator used in logical expressions.
            // \implies (⇒) is a metalanguage symbol indicating a logical consequence or entailment. It's used to express that one statement logically follows from another.
            public override string Latexise()
                => $@"{Assumption.Latexise(Assumption.Priority < Priority)} \to {Conclusion.Latexise(Conclusion.Priority < Priority)}";
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
