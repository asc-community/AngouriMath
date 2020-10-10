
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
using System.Linq;
using System.Text;
using static AngouriMath.Entity.Number;

namespace AngouriMath
{
    public abstract partial record Entity
    {
        public partial record Variable
        {
            public override string Stringize() => Name;
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Tensor
        {
            public override string Stringize() => InnerTensor.ToString();
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Sumf
        {
            public override string Stringize() =>
                Augend.Stringize(Augend.Priority < Priority) + " + " + Addend.Stringize(Addend.Priority < Priority);
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Minusf
        {
            public override string Stringize() =>
                Subtrahend.Stringize(Subtrahend.Priority < Priority) + " - " + Minuend.Stringize(Minuend.Priority <= Priority);
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Mulf
        {
            public override string Stringize() =>
                (Multiplier is Integer(-1) ? "-"
                    : Multiplier.Stringize(Multiplier.Priority < Priority) + " * ")
                + Multiplicand.Stringize(Multiplicand.Priority < Priority);
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Divf
        {
            public override string Stringize() =>
                Dividend.Stringize(Dividend.Priority < Priority) + " / " + Divisor.Stringize(Divisor.Priority <= Priority);
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Sinf
        {
            public override string Stringize() => $"sin({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Cosf
        {
            public override string Stringize() => $"cos({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Tanf
        {
            public override string Stringize() => $"tan({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Cotanf
        {
            public override string Stringize() => $"cotan({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Logf
        {
            public override string Stringize() => $"log({Base.Stringize()}, {Antilogarithm.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Powf
        {
            public override string Stringize() =>
                Exponent == 0.5m
                ? "sqrt(" + Base.Stringize() + ")"
                : Base.Stringize(Base.Priority < Priority) + " ^ " + Exponent.Stringize(Exponent.Priority < Priority);
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Arcsinf
        {
            public override string Stringize() => $"arcsin({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Arccosf
        {
            public override string Stringize() => $"arccos({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Arctanf
        {
            public override string Stringize() => $"arctan({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Arccotanf
        {
            public override string Stringize() => $"arccotan({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Factorialf
        {
            public override string Stringize() => Argument.Stringize(Argument.Priority < Priority.Leaf) + "!";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Derivativef
        {
            public override string Stringize() => $"derive({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Integralf
        {
            public override string Stringize() => $"integrate({Expression.Stringize()}, {Var.Stringize()}, {Iterations})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Limitf
        {
            public override string Stringize() =>
                ApproachFrom switch
                {
                    ApproachFrom.Left => "limitleft",
                    ApproachFrom.BothSides => "limit",
                    ApproachFrom.Right => "limitright",
                    _ => throw new System.ComponentModel.InvalidEnumArgumentException
                        (nameof(ApproachFrom), (int)ApproachFrom, typeof(ApproachFrom))
                } + $"({Expression.Stringize()}, {Var.Stringize()}, {Destination.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Signumf
        {
            public override string Stringize() => $"sgn({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        public partial record Absf
        {
            public override string Stringize() => $"abs({Argument.Stringize()})";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Boolean
        {
            public override string Stringize() => ((bool)this).ToString();
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Notf
        {
            public override string Stringize() => $"not {Argument.Stringize(Argument.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Andf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} and {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Orf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} or {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Xorf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} xor {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Impliesf
        {
            public override string Stringize()
                => $"{Assumption.Stringize(Assumption.Priority < Priority)} implies {Conclusion.Stringize(Conclusion.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Equalsf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} = {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Greaterf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} > {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record GreaterOrEqualf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} >= {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Lessf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} < {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record LessOrEqualf
        {
            public override string Stringize()
                => $"{Left.Stringize(Left.Priority < Priority)} <= {Right.Stringize(Right.Priority < Priority)}";
            protected override bool PrintMembers(StringBuilder builder)
            {
                builder.Append(Stringize());
                return false;
            }
            public override string ToString() => Stringize();
        }

        partial record Set
        {
            partial record FiniteSet
            {
                public override string Stringize()
                    => $"{{ {string.Join(", ", Elements.Select(c => c.Stringize()))} }}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record Interval
            {
                public override string Stringize()
                {
                    var left = LeftClosed ? "[" : "(";
                    var right = RightClosed ? "]" : ")";
                    return left + Left.Stringize() + "; " + Right.Stringize() + right;
                }
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record ConditionalSet
            {
                public override string Stringize()
                    => $"{{ {Var.Stringize()} : {Predicate.Stringize()} }}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record SpecialSet
            {
                public override string Stringize()
                    => DomainsFunctional.DomainToString(SetType);
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record Unionf
            {
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \/ {Right.Stringize(Right.Priority < Priority)}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record Intersectionf
            {
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} /\ {Right.Stringize(Right.Priority < Priority)}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record SetMinusf
            {
                public override string Stringize()
                    => $@"{Left.Stringize(Left.Priority < Priority)} \ {Right.Stringize(Right.Priority < Priority)}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }

            partial record Inf
            {
                public override string Stringize()
                    => $@"{Element.Stringize(Element.Priority < Priority)} in {SupSet.Stringize(SupSet.Priority < Priority)}";
                protected override bool PrintMembers(StringBuilder builder)
                {
                    builder.Append(Stringize());
                    return false;
                }
                public override string ToString() => Stringize();
            }
        }
    }
}