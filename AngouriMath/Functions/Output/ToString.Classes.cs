/*
 * Copyright (c) 2019-2020 Angourisoft
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using AngouriMath.Core;
using System.Linq;
using System.Text;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public partial record Variable
        {
            /// <inheritdoc/>
            public override string Stringize() => Name;
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Tensor
        {
            /// <inheritdoc/>
            public override string Stringize() => InnerTensor.ToString();
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Sumf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Augend.Stringize(Augend.Priority < Priority) + " + " + Addend.Stringize(Addend.Priority < Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Minusf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Subtrahend.Stringize(Subtrahend.Priority < Priority) + " - " + Minuend.Stringize(Minuend.Priority <= Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Mulf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                (Multiplier is Integer(-1) ? "-"
                    : Multiplier.Stringize(Multiplier.Priority < Priority) + " * ")
                + Multiplicand.Stringize(Multiplicand.Priority < Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Divf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Dividend.Stringize(Dividend.Priority < Priority) + " / " + Divisor.Stringize(Divisor.Priority <= Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Sinf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"sin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cosf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"cos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Tanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"tan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Cotanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"cotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Logf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"log({Base.Stringize()}, {Antilogarithm.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Powf
        {
            /// <inheritdoc/>
            public override string Stringize() =>
                Exponent == 0.5m
                ? "sqrt(" + Base.Stringize() + ")"
                : Base.Stringize(Base.Priority < Priority) + " ^ " + Exponent.Stringize(Exponent.Priority < Priority);
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arcsinf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arcsin({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccosf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arccos({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arctanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arctan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Arccotanf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"arccotan({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Factorialf
        {
            /// <inheritdoc/>
            public override string Stringize() => Argument.Stringize(Argument.Priority < Priority.Leaf) + "!";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Derivativef
        {
            /// <inheritdoc/>
            public override string Stringize()
            {
                if (Iterations == 1)
                    return $"derivative({Expression.Stringize()}, {Var.Stringize()})";
                else
                    return $"derivative({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            }
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Integralf
        {
            /// <inheritdoc/>
            public override string Stringize()
            {
                if (Iterations == 1)
                    return $"integral({Expression.Stringize()}, {Var.Stringize()})";
                else
                    return $"integral({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            }
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Limitf
        {
            /// <inheritdoc/>
            public override string Stringize() =>

                ApproachFrom switch
                {
                    ApproachFrom.Left => "limitleft",
                    ApproachFrom.BothSides => "limit",
                    ApproachFrom.Right => "limitright",
                    _ => throw new System.ComponentModel.InvalidEnumArgumentException
                        (nameof(ApproachFrom), (int)ApproachFrom, typeof(ApproachFrom))
                } + $"({Expression.Stringize()}, {Var.Stringize()}, {Destination.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Signumf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"sgn({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        public partial record Absf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"abs({Argument.Stringize()})";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Boolean
        {
            /// <inheritdoc/>
            public override string Stringize() => ((bool)this).ToString();
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Notf
        {
            /// <inheritdoc/>
            public override string Stringize() => $"not {Argument.Stringize(Argument.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Andf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} and {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Orf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} or {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Xorf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} xor {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Impliesf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Assumption.Stringize(Assumption.Priority < Priority)} implies {Conclusion.Stringize(Conclusion.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Equalsf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} = {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Greaterf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} > {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record GreaterOrEqualf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} >= {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record Lessf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} < {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

        partial record LessOrEqualf
        {
            /// <inheritdoc/>
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} <= {Right.Stringize(Right.Priority < Priority)}";
            /// <inheritdoc/>
            public override string ToString() => Stringize();
        }

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
                /// <inheritdoc/>
                public override string Stringize()
                    => DomainsFunctional.DomainToString(SetType);
                /// <inheritdoc/>
                public override string ToString() => Stringize();
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
    }
}