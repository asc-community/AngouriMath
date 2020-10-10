
/* Copyright (c) 2019-2020 Angourisoft
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation
 * files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software
 * is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 * OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using AngouriMath.Core;
using AngouriMath.Core.Exceptions;

namespace AngouriMath
{
    public abstract partial record Entity
    {   
        public partial record Variable
        {
            internal override string ToSymPy() => Name;
        }

        public partial record Tensor
        {
            internal override string ToSymPy() => InnerTensor.ToString();
        }

        public partial record Number
        {
            internal override string ToSymPy() => Stringize().Replace("i", "sympy.I");
        }


        public partial record Sumf
        {
            internal override string ToSymPy() =>
                Augend.ToSymPy(Augend.Priority < Priority.Sum) + " + " + Addend.ToSymPy(Addend.Priority < Priority.Sum);
        }

        public partial record Minusf
        {
            internal override string ToSymPy() =>
                Subtrahend.ToSymPy(Subtrahend.Priority < Priority.Minus) + " - " + Minuend.ToSymPy(Minuend.Priority <= Priority.Minus);
        }

        public partial record Mulf
        {
            internal override string ToSymPy() =>
                Multiplier.ToSymPy(Multiplier.Priority < Priority.Mul) + " * " + Multiplicand.ToSymPy(Multiplicand.Priority < Priority.Mul);
        }

        public partial record Divf
        {
            internal override string ToSymPy() =>
                Dividend.ToSymPy(Dividend.Priority < Priority.Div) + " / " + Divisor.ToSymPy(Divisor.Priority <= Priority.Div);
        }

        public partial record Sinf
        {
            internal override string ToSymPy() => "sympy.sin(" + Argument.ToSymPy() + ")";
        }

        public partial record Cosf
        {
            internal override string ToSymPy() => "sympy.cos(" + Argument.ToSymPy() + ")";
        }

        public partial record Tanf
        {
            internal override string ToSymPy() => "sympy.tan(" + Argument.ToSymPy() + ")";
        }

        public partial record Cotanf
        {
            internal override string ToSymPy() => "sympy.cot(" + Argument.ToSymPy() + ")";
        }

        public partial record Logf
        {
            internal override string ToSymPy() => "sympy.log(" + Antilogarithm.ToSymPy() + ", " + Base.ToSymPy() + ")";
        }

        public partial record Powf
        {
            internal override string ToSymPy() =>
                Exponent == 0.5m
                ? "sympy.sqrt(" + Base.ToSymPy() + ")"
                : Base.ToSymPy(Base.Priority < Priority.Pow) + " ** " + Exponent.ToSymPy(Exponent.Priority < Priority.Pow);
        }

        public partial record Arcsinf
        {
            internal override string ToSymPy() => "sympy.asin(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccosf
        {
            internal override string ToSymPy() => "sympy.acos(" + Argument.ToSymPy() + ")";
        }

        public partial record Arctanf
        {
            internal override string ToSymPy() => "sympy.atan(" + Argument.ToSymPy() + ")";
        }

        public partial record Arccotanf
        {
            internal override string ToSymPy() => "sympy.acot(" + Argument.ToSymPy() + ")";
        }

        public partial record Factorialf
        {
            internal override string ToSymPy() => "sympy.factorial(" + Argument.ToSymPy() + ")";
        }

        public partial record Derivativef
        {
            internal override string ToSymPy() => $"sympy.diff({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Integralf
        {
            // TODO: The 3rd parameter of sympy.integrate is not interpreted as iterations, unlike sympy.diff
            // which allows both sympy.diff(expr, var, iterations) and sympy.diff(expr, var1, var2, var3...)
            internal override string ToSymPy() => $"sympy.integrate({Expression.ToSymPy()}, {Var.ToSymPy()}, {Iterations})";
        }

        public partial record Limitf
        {
            internal override string ToSymPy() =>
                @$"sympy.limit({Expression.ToSymPy()}, {Var.ToSymPy()}, {Destination.ToSymPy()}{ApproachFrom switch
                {
                    ApproachFrom.Left => ", '-'",
                    ApproachFrom.BothSides => "",
                    ApproachFrom.Right => ", '+'",
                    _ => throw new System.ComponentModel.InvalidEnumArgumentException
                        (nameof(ApproachFrom), (int)ApproachFrom, typeof(ApproachFrom))
                }})";
        }

        public partial record Signumf
        {
            internal override string ToSymPy()
                => $@"sympy.sign({Argument.ToSymPy()})";
        }

        public partial record Absf
        {
            internal override string ToSymPy()
                => $@"sympy.Abs({Argument.ToSymPy()})";
        }

        partial record Boolean
        {
            internal override string ToSymPy()
                => this ? "True" : "False";
        }

        partial record Notf
        {
            internal override string ToSymPy()
                => $"not {Argument.ToSymPy(Argument.Priority < Priority)}";
        }

        partial record Andf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} and {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} or {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} ^ {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            internal override string ToSymPy()
                => $"sympy.Implies({Assumption.ToSymPy()}, {Conclusion.ToSymPy()})";
        }

        partial record Equalsf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} == {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} > {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} >= {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} < {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            internal override string ToSymPy()
                => $"{Left.ToSymPy(Left.Priority < Priority)} <= {Right.ToSymPy(Right.Priority < Priority)}";
        }

        partial record Set
        {
            partial record FiniteSet
            {
                internal override string ToSymPy()
                    => $"FiniteSet({string.Join(", ", elements)})";
            }

            partial record Interval
            {
                internal override string ToSymPy()
                    => $"Interval({Left}, {Right}, left_open={(!(Boolean)LeftClosed).ToSymPy()}, right_open={(!(Boolean)RightClosed).ToSymPy()})";
            }

            partial record ConditionalSet
            {
                internal override string ToSymPy()
                    => $"ConditionSet({Var.ToSymPy()}, {Predicate.ToSymPy()}, {((SpecialSet)Var.Codomain).ToSymPy()})";
            }

            partial record SpecialSet
            {
                internal override string ToSymPy()
                    => "S." + (SetType switch
                    {
                        Domain.Integer => "Integers",
                        Domain.Rational => "Rationals",
                        Domain.Real => "Reals",
                        Domain.Complex => "Complexes",
                        _ => throw new MathSException($"There is no {SetType} in either SymPy or AM's {nameof(ToSymPy)}")
                    });
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
    }
}