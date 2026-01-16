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
                => Argument is Equalsf(var left, var right)
                   ? $@"{left.Latexise(left.LatexPriority <= Priority.Equal)} \neq {right.Latexise(right.LatexPriority <= Priority.Equal)}"
                   : $@"\neg{{{Argument.Latexise(Argument.LatexPriority < LatexPriority)}}}";
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                var result = new System.Text.StringBuilder();
                Entity? left = null;
                bool first = true;
                foreach (var child in LinearChildren(this))
                {
                    switch (child)
                    {
                        // Detect chained comparisons like (a < b) ∧ (b < c) ∧ (c < d) and display as a < b < c < d
                        case ComparisonSign:
                            var renew = left != child.DirectChildren[0];
                            if (!first && renew) result.Append(@" \land ");
                            first = false;
                            if (first || renew) result.Append(child.DirectChildren[0].Latexise(child.DirectChildren[0].LatexPriority <= child.LatexPriority));
                            renew = false;
                            result.Append(child switch {
                                Equalsf => " = ",
                                Greaterf => " > ",
                                GreaterOrEqualf => @" \geq ",
                                Lessf => " < ",
                                LessOrEqualf => @" \leq ",
                                _ => throw new Core.Exceptions.AngouriBugException("Unexpected comparison sign in Andf.Latexise")
                            }).Append(child.DirectChildren[1].Latexise(child.DirectChildren[1].LatexPriority <= child.LatexPriority));
                            left = child.DirectChildren[1];
                            break;
                        case Notf (Equalsf eq):
                            renew = left != eq.Left;
                            if (!first && renew) result.Append(@" \land ");
                            first = false;
                            if (first || renew) result.Append(eq.Left.Latexise(eq.Left.LatexPriority <= eq.LatexPriority));
                            renew = false;
                            result.Append(@" \neq ").Append(eq.Right.Latexise(eq.Right.LatexPriority <= eq.LatexPriority));
                            left = eq.Right;
                            break;
                        default:
                            if (!first) result.Append(" \\land ");
                            left = null;
                            first = false;
                            result.Append(child.Latexise(child.LatexPriority < LatexPriority));
                            break;
                    }
                }
                return result.ToString();
            }
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority < LatexPriority)} \lor {Right.Latexise(Right.LatexPriority < LatexPriority)}";
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            // NOTE: \veebar (⊻) can disambiguate better than \oplus (⊕) which can mean the direct sum in algebra.
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority < LatexPriority)} \veebar {Right.Latexise(Right.LatexPriority < LatexPriority)}";
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            // NOTE:
            // \to (→) is an object-language symbol within the formal system being studied. It's the implication operator used in logical expressions.
            // \implies (⇒) is a metalanguage symbol indicating a logical consequence or entailment. It's used to express that one statement logically follows from another.
            // ISO 80000-2 puts ⇒ first for implies, but → is also accepted and is more common in logic contexts.
            // ⇒ can be used in steps for proofs if we were to add it later.
            public override string Latexise()
                => $@"{Assumption.Latexise(Assumption.LatexPriority <= LatexPriority)} \to {Conclusion.Latexise(Conclusion.LatexPriority < LatexPriority)}";
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority <= LatexPriority)} = {Right.Latexise(Right.LatexPriority <= LatexPriority)}";
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority <= LatexPriority)} > {Right.Latexise(Right.LatexPriority <= LatexPriority)}";
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            // NOTE: While \geqslant (⩾) is more used in e.g. Russian texts, \geq (≥) is more universally used in English texts.
            // Since the output language of LaTeX is English (referencing Piecewise and Providedf), \geq is more appropriate.
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority <= LatexPriority)} \geq {Right.Latexise(Right.LatexPriority <= LatexPriority)}";
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority <= LatexPriority)} < {Right.Latexise(Right.LatexPriority <= LatexPriority)}";
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            // NOTE: While \leqslant (⩽) is more used in e.g. Russian texts, \leq (≤) is more universally used in English texts.
            // Since the output language of LaTeX is English (referencing Piecewise and Providedf), \leq is more appropriate.
            public override string Latexise()
                => $@"{Left.Latexise(Left.LatexPriority <= LatexPriority)} \leq {Right.Latexise(Right.LatexPriority <= LatexPriority)}";
        }
    }
}
