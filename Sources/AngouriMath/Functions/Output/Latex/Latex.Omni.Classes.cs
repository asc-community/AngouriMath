using System.Linq;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Set
        {
            partial record FiniteSet
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => IsSetEmpty ? @"\emptyset" : $@"\left\{{ {string.Join(", ", Elements.Select(c => c.Latexise()))} \right\}}";
            }

            partial record Interval
            {
                /// <inheritdoc/>
                public override string Latexise()
                {
                    var left = LeftClosed ? "[" : "(";
                    var right = RightClosed ? "]" : ")";
                    return @"\left" + left + Left.Latexise() + "; " + Right.Latexise() + @"\right" + right;
                }
            }

            partial record ConditionalSet
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"\left\{{ {Var.Latexise()} : {Predicate.Latexise()} \right\}}";
            }

            partial record SpecialSet
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"\mathbb{{{Stringize()[0]}}}";
            }

            partial record Unionf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \cup {Right.Latexise(Right.Priority < Priority)}";
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \cap {Right.Latexise(Right.Priority < Priority)}";
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \setminus {Right.Latexise(Right.Priority < Priority)}";
            }

            partial record Inf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Element.Latexise(Element.Priority < Priority)} \in {SupSet.Latexise(SupSet.Priority < Priority)}";
            }
        }

        partial record Providedf
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"\left\({Expression.Latexise()} \: \text{{for}} \: {Predicate.Latexise()}\right\)";
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"\begin{{cases}}" +
                string.Join(@"\\",
                Cases.Select(c =>
                {
                    if (c.Predicate == Boolean.True)
                        return $@"{c.Expression.Latexise()} \: \text{{otherwise}}";
                    return $@"{c.Expression.Latexise()} \: \text{{for}} \: {c.Predicate.Latexise()}";
                }
                ))
                +
                @"\end{cases}";
        }
    }
}
