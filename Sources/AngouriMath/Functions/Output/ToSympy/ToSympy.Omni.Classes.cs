/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core.Exceptions;
using System.Linq;

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
    }
}
