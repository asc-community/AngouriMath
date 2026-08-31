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
                    => $"sympy.FiniteSet({string.Join(", ", Elements.Select(c => c.ToSymPy()))})";
            }

            partial record Interval
            {
                internal override string ToSymPy()
                    => $"sympy.Interval({Left.ToSymPy()}, {Right.ToSymPy()}, left_open={((Boolean)!LeftClosed).ToSymPy()}, right_open={((Boolean)!RightClosed).ToSymPy()})";
            }

            partial record ConditionalSet
            {
                // A set builder's Codomain is Domain.Any, which SpecialSet.Create has no
                // member for and throws on -- so every set builder threw an AngouriBugException
                // out of the exporter.
                // https://github.com/asc-community/AngouriMath/issues/985
                //
                // S.UniversalSet is what SymPy prints a ConditionSet without a third argument
                // over, so it is the right thing to emit for a predicate under no further
                // restriction. It is a choice about the target language and not a claim about
                // this one: SymPy has a universal set and AngouriMath does not, and an exporter
                // to a system without one (or with a typed one, as Lean and SMT have) picks
                // differently. Nothing here may be read back as "Domain.Any is a set".
                // https://github.com/asc-community/AngouriMath/issues/996
                internal override string ToSymPy()
                    => $"sympy.ConditionSet({Var.ToSymPy()}, {Predicate.ToSymPy()}, {CodomainToSymPy()})";

                private string CodomainToSymPy()
                    => Codomain is Domain.Any ? "sympy.S.UniversalSet" : SpecialSet.Create(Codomain).ToSymPy();
            }

            partial record SpecialSet
            {
                partial record Integers
                {
                    internal override string ToSymPy()
                        => "sympy.S.Integers";
                }

                partial record Rationals
                {
                    internal override string ToSymPy()
                        => "sympy.S.Rationals";
                }

                partial record Reals
                {
                    internal override string ToSymPy()
                        => "sympy.S.Reals";
                }

                partial record Complexes
                {
                    internal override string ToSymPy()
                        => "sympy.S.Complexes";
                }

                internal override string ToSymPy()
                        => throw new NotSufficientlySupportedException($"There is no {this} in either SymPy or AM's {nameof(ToSymPy)}");
            }

            partial record Unionf
            {
                internal override string ToSymPy()
                    => $"sympy.Union({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record Intersectionf
            {
                internal override string ToSymPy()
                    => $"sympy.Intersection({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record SetMinusf
            {
                internal override string ToSymPy()
                    => $"sympy.Complement({Left.ToSymPy()}, {Right.ToSymPy()})";
            }

            partial record Inf
            {
                // Python's `in` forces its result to a bool, and a membership that is not
                // decided is not one: `x in sympy.S.Reals` raises "did not evaluate to a
                // bool: (-oo < x) & (x < oo)". `.contains` is the form that answers with the
                // condition instead, and it still answers True or False when it can.
                // https://github.com/asc-community/AngouriMath/issues/985
                internal override string ToSymPy()
                    => $"({SupSet.ToSymPy()}).contains({Element.ToSymPy()})";
            }
        }



        partial record Providedf
        {
            // TODO: is it the right way of using refine?
            internal override string ToSymPy() => $"sympy.refine({Expression.ToSymPy()}, sympy.Q.is_true({Predicate.ToSymPy()}))";
        }

        partial record Piecewise
        {
            internal override string ToSymPy() => $"sympy.Piecewise({string.Join(", ", Cases.Select(c => $"({c.Expression.ToSymPy()}, {c.Predicate.ToSymPy()})"))})";
        }
        
        partial record Matrix
        {
            internal override string ToSymPy()
                => "sympy.ImmutableMatrix([" +
                    string.Join(", ",
                        IsVector switch
                        {
                            true => this.Select(c => c.ToSymPy()),
                            false => this.Select(c => $"[{string.Join(", ", ((Matrix)c).T.Select(e => e.ToSymPy()))}]"),
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
                => $"sympy.Lambda({Parameter.ToSymPy()}, {Body.ToSymPy()})";
        }
    }
}
