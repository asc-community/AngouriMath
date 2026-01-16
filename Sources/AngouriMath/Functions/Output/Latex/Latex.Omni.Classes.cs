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
                // NOTE: Comma is used as the separator following standard mathematical notation (ISO 80000-2).
                // Some regional variants use semicolon, but comma is more universally recognized.
                public override string Latexise()
                {
                    var left = LeftClosed ? "[" : "(";
                    var right = RightClosed ? "]" : ")";
                    return @"\left" + left + Left.Latexise() + ", " + Right.Latexise() + @"\right" + right;
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
                    => $@"{Left.Latexise(Left.LatexPriority < LatexPriority)} \cup {Right.Latexise(Right.LatexPriority < LatexPriority)}";
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Left.Latexise(Left.LatexPriority < LatexPriority)} \cap {Right.Latexise(Right.LatexPriority < LatexPriority)}";
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Left.Latexise(Left.LatexPriority < LatexPriority)} \setminus {Right.Latexise(Right.LatexPriority < LatexPriority)}";
            }

            partial record Inf
            {
                /// <inheritdoc/>
                public override string Latexise()
                    => $@"{Element.Latexise(Element.LatexPriority < LatexPriority)} \in {SupSet.Latexise(SupSet.LatexPriority < LatexPriority)}";
            }
        }

        partial record Providedf
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"{Expression.Latexise(Expression.LatexPriority < LatexPriority)} \quad \text{{for}} \quad {Predicate.Latexise(Predicate.LatexPriority <= LatexPriority)}";
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            public override string Latexise() => $@"\begin{{cases}}" +
                string.Join(@"\\",
                Cases.Select(c =>
                {
                    if (c.Predicate == Boolean.True)
                        return $@"{c.Expression.Latexise(c.Expression.LatexPriority < Priority.Provided)} & \text{{otherwise}}";
                    return $@"{c.Expression.Latexise(c.Expression.LatexPriority < Priority.Provided)} & \text{{for }} {c.Predicate.Latexise(c.Predicate.LatexPriority <= Priority.Provided)}"; // "for" used in https://mathworld.wolfram.com/Derivative.html
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
            // NOTE: Application represents curried function application. Multiple arguments
            // are rendered as separate applications: f(x)(y) rather than f(x, y)
            public override string Latexise()
            {
                var result = new StringBuilder(Expression.Latexise(Expression.LatexPriority < LatexPriority));
                foreach (var arg in Arguments)
                {
                    result.Append(@"\left(").Append(arg.Latexise()).Append(@"\right)");
                }
                return result.ToString();
            }
        }

        partial record Lambda
        {
            /// <inheritdoc/>
            // NOTE: \mapsto (↦) links the function variable with its output while
            // \rightarrow (→) links the function domain with its codomain. See https://math.stackexchange.com/a/651240, https://math.stackexchange.com/a/936591
            public override string Latexise()
                => Parameter.Latexise() + @" \mapsto " + Body.Latexise(Body.LatexPriority < LatexPriority);
        }
    }
}
