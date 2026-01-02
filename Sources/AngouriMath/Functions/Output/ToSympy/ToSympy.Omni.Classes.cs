//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    partial record Entity
    {
        partial record Set
        {
            partial record FiniteSet
            {
                internal override string ToSymPy()
                    => $"FiniteSet({string.Join(", ", Elements.Select(c => c.ToSymPy()))})";
            }

            partial record Interval
            {
                internal override string ToSymPy()
                    => $"Interval({Left}, {Right}, left_open={((Boolean)!LeftClosed).ToSymPy()}, right_open={((Boolean)!RightClosed).ToSymPy()})";
            }

            partial record ConditionalSet
            {
                internal override string ToSymPy()
                    => $"ConditionSet({Var.ToSymPy()}, {Predicate.ToSymPy()}, {((SpecialSet)Codomain).ToSymPy()})";
            }

            partial record SpecialSet
            {
                partial record Integers
                {
                    internal override string ToSymPy()
                        => "S.Integers";
                }

                partial record Rationals
                {
                    internal override string ToSymPy()
                        => "S.Rationals";
                }

                partial record Reals
                {
                    internal override string ToSymPy()
                        => "S.Reals";
                }

                partial record Complexes
                {
                    internal override string ToSymPy()
                        => "S.Complexes";
                }

                internal override string ToSymPy()
                        => throw new NotSufficientlySupportedException($"There is no {this} in either SymPy or AM's {nameof(ToSymPy)}");
            }

            partial record Unionf
            {
                internal override string ToSymPy()
                    => $"Union({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record Intersectionf
            {
                internal override string ToSymPy()
                    => $"Intersection({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record SetMinusf
            {
                internal override string ToSymPy()
                    => $"Complement({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record Inf
            {
                internal override string ToSymPy()
                    => $"{Element.ToSymPy(Element.Priority < Priority)} in {SupSet.ToSymPy(SupSet.Priority < Priority)}";
            }
        }



        partial record Providedf
        {
            // TODO: is it the right way of using refine?
            internal override string ToSymPy() => $"sympy.refine({Expression.ToSymPy()}, sympy.Q.is_true({Predicate.ToSymPy()}))";
        }

        partial record Piecewise
        {
            internal override string ToSymPy() => $"sympy.Piecewise({string.Join(", ", Cases.Select(c => $"({c.Expression}, {c.Predicate})"))})";
        }
        
        partial record Matrix
        {
            internal override string ToSymPy()
                => "sympy.ImmutableMatrix([" +
                    string.Join(", ",
                        IsVector switch
                        {
                            true => this.Select(c => c.ToSymPy()),
                            false => this.Select(c => $"[{string.Join(", ", ((Matrix)c).T)}]"),
                        }) +
                   "])";
        }

        partial record Application
        {
            internal override string ToSymPy()
                => Expression is Lambda
                    ? $"({Expression.ToSymPy()})({", ".Join(Arguments.Select(arg => arg.ToSymPy()))})"
                    : throw new NotSufficientlySupportedException("Sympy might not have application of undeclared lambda");
        }

        partial record Lambda
        {
            internal override string ToSymPy()
                => $"sympy.Lambda({Parameter.ToSymPy()}, )";
        }
    }
}
