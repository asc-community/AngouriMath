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
        partial record Set
        {
            partial record FiniteSet
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $"{{ {string.Join(", ", Elements.Select(c => c.Stringize()))} }}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Interval
            {
                /// <inheritdoc/>
                public override string Stringize()
                {
                    var left = LeftClosed ? "[" : "(";
                    var right = RightClosed ? "]" : ")";
                    return left + Left.Stringize() + "; " + Right.Stringize() + right;
                }
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record ConditionalSet
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $"{{ {Var.Stringize()} : {Predicate.Stringize()} }}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record SpecialSet
            {
                partial record Booleans
                {
                    /// <inheritdoc/>
                    public override string Stringize() => "BB";
                    /// <inheritdoc/>
                    public override string ToString() => Stringize();
                }

                partial record Integers
                {
                    /// <inheritdoc/>
                    public override string Stringize() => "ZZ";
                    /// <inheritdoc/>
                    public override string ToString() => Stringize();
                }

                partial record Rationals
                {
                    /// <inheritdoc/>
                    public override string Stringize() => "QQ";
                    /// <inheritdoc/>
                    public override string ToString() => Stringize();
                }

                partial record Reals
                {
                    /// <inheritdoc/>
                    public override string Stringize() => "RR";
                    /// <inheritdoc/>
                    public override string ToString() => Stringize();
                }

                partial record Complexes
                {
                    /// <inheritdoc/>
                    public override string Stringize() => "CC";
                    /// <inheritdoc/>
                    public override string ToString() => Stringize();
                }
            }

            partial record Unionf
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \/ {Right.Stringize(Right.Priority < Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Intersectionf
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} /\ {Right.Stringize(Right.Priority < Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record SetMinusf
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \ {Right.Stringize(Right.Priority < Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Inf
            {
                /// <inheritdoc/>
                public override string Stringize()
                    => $@"{Element.Stringize(Element.Priority < Priority)} in {SupSet.Stringize(SupSet.Priority < Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }
        }

        

        partial record Providedf
        {
            /// <inheritdoc/>
            public override string Stringize() => $@"{Expression.Stringize()} provided {Predicate.Stringize()}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            public override string Stringize() => $"({string.Join(", ", Cases.Select(n => $"{n.Expression} if {n.Predicate}"))})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Matrix
        {
            /// <inheritdoc/>
            public override string Stringize()
                => "[" +
                    string.Join(", ",
                        IsVector switch
                        {
                            true => this.Select(c => c.ToString()),
                            false => this.Select(c => $"[{string.Join(", ", ((Matrix)c).T)}]"),
                        }) +
                   "]";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Application
        {
            /// <inheritdoc/>
            public override string Stringize()
                => Expression.Stringize(Expression.Priority < Priority) + " " +
                    " ".Join(Arguments.Select(arg => arg.Stringize(arg.Priority <= Priority)));

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Lambda
        {
            /// <inheritdoc/>
            public override string Stringize()
                => Parameter.Stringize() + " -> " + Body.Stringize(Body.Priority < Priority);

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
