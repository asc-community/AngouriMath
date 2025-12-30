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
            {
                // Detect patterns like (a < b) ∧ (b < c) ∧ (c < d) and returns the chain of comparisons
                List<ComparisonSign>? comparisons = null;
                void GatherComparisons(Entity e) // Recursively gather all comparisons connected by AND
                {
                    switch (e)
                    {
                        case Andf(var left, var right):
                            GatherComparisons(left);
                            GatherComparisons(right);
                            break;
                        case ComparisonSign comp:
                            comparisons ??= [];
                            comparisons.Add(comp);
                            break;
                    }
                }
                GatherComparisons(this);

                // Check if comparisons form a valid chain: right side of each must equal left side of next
                if (comparisons is { Count: >= 2 })
                    for (int i = 0; i < comparisons.Count - 1; i++)
                        if (comparisons[i].DirectChildren[1] != comparisons[i + 1].DirectChildren[0])
                            goto fallback; // Not a valid chain

                // Try to detect chained comparisons: (a < b) ∧ (b < c) → a < b < c
                if (comparisons is { Count: > 1 } chain)
                {
                    var result = new System.Text.StringBuilder(chain[0].DirectChildren[0].Latexise(chain[0].DirectChildren[0].Priority < chain[0].Priority));
                    foreach (var comparison in chain)
                    {
                        var op = comparison switch
                        {
                            Equalsf => " = ",
                            Greaterf => " > ",
                            GreaterOrEqualf => @" \geq ",
                            Lessf => " < ",
                            LessOrEqualf => @" \leq ",
                            _ => null
                        };
                        if (op is null)
                            goto fallback;
                        result.Append(op).Append(comparison.DirectChildren[1].Latexise(comparison.DirectChildren[1].Priority < comparison.Priority));
                    }
                    return result.ToString();
                }

            fallback:
                return $@"{Left.Latexise(Left.Priority < Priority)} \land {Right.Latexise(Right.Priority < Priority)}";
            }
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
            // ISO 80000-2 puts ⇒ first for implies, but → is also accepted and is more common in logic contexts.
            // ⇒ can be used in steps for proofs if we were to add it later.
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
            // NOTE: While \geqslant (⩾) is more used in e.g. Russian texts, \geq (≥) is more universally used in English texts.
            // Since the output language of LaTeX is English (referencing Piecewise and Providedf), \geq is more appropriate.
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \geq {Right.Latexise(Right.Priority < Priority)}";
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
            // NOTE: While \leqslant (⩽) is more used in e.g. Russian texts, \leq (≤) is more universally used in English texts.
            // Since the output language of LaTeX is English (referencing Piecewise and Providedf), \leq is more appropriate.
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \leq {Right.Latexise(Right.Priority < Priority)}";
        }
    }
}
