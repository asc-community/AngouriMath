//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Text;

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

        partial record Matrix
        {
            /// <inheritdoc/>
            public override string Latexise()
            {
                if (IsVector)
                {
                    var sb = new StringBuilder();
                    sb.Append(@"\begin{bmatrix}");
                    sb.Append(string.Join(@" \\ ", InnerMatrix.Iterate().Select(k => k.Value.Latexise())));
                    sb.Append(@"\end{bmatrix}");
                    return sb.ToString();
                }
                {
                    var sb = new StringBuilder();
                    sb.Append(@"\begin{bmatrix}");
                    var lines = new List<string>();
                    for (int x = 0; x < RowCount; x++)
                    {
                        var items = new List<string>();

                        for (int y = 0; y < ColumnCount; y++)
                            items.Add(this[x, y].Latexise());

                        var line = string.Join(" & ", items);
                        lines.Add(line);
                    }
                    sb.Append(string.Join(@" \\ ", lines));
                    sb.Append(@"\end{bmatrix}");
                    return sb.ToString();
                }
            }
        }

        partial record Application
        {
            /// <inheritdoc/>
            public override string Latexise()
                => Expression.Latexise(Expression.Priority < Priority) + " " +
                    " ".Join(Arguments.Select(arg => arg.Latexise(arg.Priority <= Priority)));
        }

        partial record Lambda
        {
            /// <inheritdoc/>
            public override string Latexise()
                => Parameter.Latexise() + @" \rightarrow " + Body.Latexise(Body.Priority < Priority);
        }
    }
}
