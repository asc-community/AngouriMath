
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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using PeterO.Numbers;

namespace AngouriMath
{
    using Core;
    using static Entity.Number;
    public abstract partial record Entity
    {
        public partial record Sumf
        {
            public override string Latexise() =>
                Augend.Latexise(Augend.Priority < Priority)
                + (Addend.Latexise(Addend.Priority < Priority) is var addend && addend.StartsWith("-")
                    ? addend : "+" + addend);
        }

        public partial record Minusf
        {
            public override string Latexise() =>
                Subtrahend.Latexise(Subtrahend.Priority < Priority)
                + "-" + Minuend.Latexise(Minuend.Priority <= Priority);
        }

        public partial record Mulf
        {
            public override string Latexise() =>
                (Multiplier == -1 ? "-" : Multiplier.Latexise(Multiplier.Priority < Priority) + @"\times ")
                + Multiplicand.Latexise(Multiplicand.Priority < Priority);
        }

        public partial record Divf
        {
            public override string Latexise() =>
                @"\frac{" + Dividend.Latexise() + "}{" + Divisor.Latexise() + "}";
        }

        public partial record Sinf
        {
            public override string Latexise() =>
                @"\sin\left(" + Argument.Latexise() + @"\right)";
        }
        public partial record Cosf
        {
            public override string Latexise() =>
                @"\cos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Tanf
        {
            public override string Latexise() =>
                @"\tan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Cotanf
        {
            public override string Latexise() =>
                @"\cot\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Logf
        {
            public override string Latexise() =>
                Base == 10
                ? @"\log\left(" + Antilogarithm.Latexise() + @"\right)"
                : Base == MathS.e
                ? @"\ln\left(" + Antilogarithm.Latexise() + @"\right)"
                : @"\log_{" + Base.Latexise() + @"}\left(" + Antilogarithm.Latexise() + @"\right)";
        }

        public partial record Powf
        {
            public override string Latexise()
            {
                if (Exponent is Rational { ERational: { Numerator: var numerator, Denominator: var denominator } }
                    and not Integer)
                {
                    var str =
                        @"\sqrt" + (denominator.Equals(2) ? "" : "[" + denominator + "]")
                        + "{" + Base.Latexise() + "}";
                    var abs = numerator.Abs();
                    if (!abs.Equals(EInteger.One))
                        str += "^{" + abs + "}";
                    if (numerator < 0)
                        str = @"\frac{1}{" + str + "}";
                    return str;
                }
                else
                {
                    return "{" + Base.Latexise(Base.Priority <= Priority.Pow) + "}^{" + Exponent.Latexise() + "}";
                }
            }
        }

        public partial record Arcsinf
        {
            public override string Latexise() =>
                @"\arcsin\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccosf
        {
            public override string Latexise() =>
                @"\arccos\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arctanf
        {
            public override string Latexise() =>
                @"\arctan\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Arccotanf
        {
            public override string Latexise() =>
                @"\arccot\left(" + Argument.Latexise() + @"\right)";
        }

        public partial record Factorialf
        {
            public override string Latexise() =>
                Argument.Latexise(Argument.Priority < Priority.Func) + "!";
        }

        public partial record Derivativef
        {
            public override string Latexise()
            {
                var powerIfNeeded = Iterations == 1 ? "" : "^{" + Iterations + "}";

                var varOverDeriv =
                    Var is Variable { Name: { Length: 1 } name }
                    ? name
                    : @"\left[" + Var.Latexise(false) + @"\right]";

                // TODO: Should we display the d upright using \mathrm?
                // Differentiation is an operation, just like sin, cos, etc.
                return @"\frac{d" + powerIfNeeded +
                @"\left[" + Expression.Latexise(false) + @"\right]}{d" + varOverDeriv + powerIfNeeded + "}";
            }
        }

        public partial record Integralf
        {
            public override string Latexise()
            {
                // Unlike derivatives, integrals do not have "power" that would be equal
                // to sequential applying integration to a function

                if (Iterations < 0)
                    return "Error";

                if (Iterations == 0)
                    return Expression.Latexise(false);

                var sb = new StringBuilder();
                for (int i = 0; i < Iterations; i++)
                    sb.Append(@"\int ");
                sb.Append(@"\left[");
                sb.Append(Expression.Latexise(false));
                sb.Append(@"\right]");

                // TODO: can we write d^2 x or (dx)^2 instead of dx dx?
                // I don't think I have ever seen the same variable being integrated more than one time. -- Happypig375
                for (int i = 0; i < Iterations; i++)
                {
                    sb.Append(" d");
                    if (Var is Variable { Name: { Length: 1 } name })
                        sb.Append(name);
                    else
                    {
                        sb.Append(@"\left[");
                        sb.Append(Var.Latexise(false));
                        sb.Append(@"\right]");
                    }
                }
                return sb.ToString();
            }
        }

        public partial record Limitf
        {
            public override string Latexise()
            {
                var sb = new StringBuilder();
                sb.Append(@"\lim_{").Append(Var.Latexise())
                    .Append(@"\to ").Append(Destination.Latexise());

                switch (ApproachFrom)
                {
                    case ApproachFrom.Left:
                        sb.Append("^-");
                        break;
                    case ApproachFrom.Right:
                        sb.Append("^+");
                        break;
                }

                sb.Append("} ");
                if (Expression.Priority < Priority.Pow)
                    sb.Append(@"\left[");
                sb.Append(Expression.Latexise());
                if (Expression.Priority < Priority.Pow)
                    sb.Append(@"\right]");

                return sb.ToString();
            }
        }

        partial record Signumf
        {
            public override string Latexise()
                => $@"\operatorname{{sgn}}\left({Argument.Latexise()}\right)";
        }

        partial record Absf
        {
            public override string Latexise()
                => $@"\left|{Argument.Latexise()}\right|";
        }

        partial record Boolean
        {
            public override string Latexise() => $@"\operatorname{{{(bool)this}}}";
        }

        partial record Notf
        {
            public override string Latexise()
                => $@"\neg{{{Argument.Latexise(Argument.Priority < Priority)}}}";
        }

        partial record Andf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \land {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Orf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \lor {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Xorf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \oplus {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Impliesf
        {
            public override string Latexise()
                => $@"{Assumption.Latexise(Assumption.Priority < Priority)} \implies {Conclusion.Latexise(Conclusion.Priority < Priority)}";
        }

        partial record Equalsf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} = {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Greaterf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} > {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record GreaterOrEqualf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \geqslant {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Lessf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} < {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record LessOrEqualf
        {
            public override string Latexise()
                => $@"{Left.Latexise(Left.Priority < Priority)} \leqslant {Right.Latexise(Right.Priority < Priority)}";
        }

        partial record Set
        {
            partial record FiniteSet
            {
                public override string Latexise()
                    => $@"\left\{{{Stringize()}}}";
            }

            partial record Interval
            {
                public override string Latexise()
                {
                    var left = LeftClosed ? "[" : "(";
                    var right = RightClosed ? "]" : ")";
                    return @"\left" + left + Left.Stringize() + "; " + Right.Stringize() + @"\right" + right;
                }
            }

            partial record ConditionalSet
            {
                public override string Latexise()
                    => $@"\left\{{ {Var.Latexise()} | {Predicate.Latexise()} \right}}";
            }

            partial record SpecialSet
            {
                public override string Latexise()
                    => $@"\mathbb{{{Stringize()[0]}}}";
            }

            partial record Unionf
            {
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \cup {Right.Latexise(Right.Priority < Priority)}";
            }

            partial record Intersectionf
            {
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \cap {Right.Latexise(Right.Priority < Priority)}";
            }

            partial record SetMinusf
            {
                public override string Latexise()
                    => $@"{Left.Latexise(Left.Priority < Priority)} \setminus {Right.Latexise(Right.Priority < Priority)}";
            }
        }
    }
}