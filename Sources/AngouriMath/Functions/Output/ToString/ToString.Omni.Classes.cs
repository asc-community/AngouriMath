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
                // Union is associative, so a union on the right may stay unbracketed -- but `\/`
                // and `\` share one precedence level and are folded by one loop in the grammar, so
                // a *set difference* on the right must be bracketed or it is re-read as the outer
                // operator: `{ 1, 2 } \/ ({ 3 } \ { 1, 2 })` is { 1, 2, 3 } and
                // `({ 1, 2 } \/ { 3 }) \ { 1, 2 }` is { 3 }.
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \/ {Right.Stringize(Right.Priority < Priority || Right is SetMinusf)}";
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
                // Set difference is not associative, and it shares its precedence level with
                // union, so anything at that level on the right needs bracketing -- the same rule
                // `-` and `/` follow. `{1,2,3} \ ({2,3} \ {3})` is { 1, 3 } where
                // `({1,2,3} \ {2,3}) \ {3}` is { 1 }.
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \ {Right.Stringize(Right.Priority <= Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }

            partial record Inf
            {
                /// <inheritdoc/>
                // `in` is folded to the left and is not associative: `(a in b) in c` asks whether
                // a truth value is an element of c, `a in (b in c)` whether a is an element of one.
                public override string Stringize()
                    => $@"{Element.Stringize(Element.Priority < Priority)} in {SupSet.Stringize(SupSet.Priority <= Priority)}";
                /// <inheritdoc/>
                public override string ToString() => Stringize();
            }
        }

        

        partial record Providedf
        {
            /// <inheritdoc/>
            // `provided` is the one infix operator the grammar folds to the *right*, so this is
            // the mirror of the `implies` rule: the flat form is read as
            // `a provided (b provided c)`, and it is the left operand that has to say when it is
            // an attached condition of its own. `(x provided p) provided q` printed flat came
            // back as `x provided (p provided q)` -- the same value, since both are `x` exactly
            // when `p` and `q` hold, and a different expression, which is what the round trip is
            // about.
            public override string Stringize() => $@"{Expression.Stringize(Expression.Priority <= Priority.Provided)} provided {Predicate.Stringize(Predicate.Priority < Priority.Provided)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Piecewise
        {
            /// <inheritdoc/>
            // piecewise(a provided p, b provided q). The `if` spelling is not in the grammar,
            // and neither is a bare comma-separated list, so a printed piecewise came back
            // either as a product with `if` read as an undeclared variable, or as nothing at
            // all once there was more than one case.
            public override string Stringize()
                => "piecewise(" + string.Join(", ",
                    Cases.Select(n => n.Expression.Stringize(n.Expression.Priority < Priority)
                                      + " provided "
                                      + n.Predicate.Stringize(n.Predicate.Priority < Priority))) + ")";
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
            // apply(f, a, b), for the same reason: juxtaposition is not application in the
            // grammar, and `(x -> x + 1) 2` came back as a power.
            public override string Stringize()
                => "apply(" + Expression.Stringize() + ", " +
                    string.Join(", ", Arguments.Select(arg => arg.Stringize())) + ")";

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Lambda
        {
            /// <inheritdoc/>
            // lambda(p, body), because that is what the parser reads. The arrow spelling is
            // not in the grammar at all, and `->` there is the implication operator, so a
            // printed lambda used to come back as an implication.
            public override string Stringize()
                => "lambda(" + Parameter.Stringize() + ", " + Body.Stringize() + ")";

            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }
    }
}
