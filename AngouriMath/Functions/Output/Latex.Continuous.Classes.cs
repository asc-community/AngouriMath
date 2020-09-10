
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
        partial record Continuous
        {

            public partial record Sumf
            {
                public override string Latexise() =>
                    Augend.Latexise(Augend.Priority < Priority.Sum)
                    + (Addend.Latexise(Addend.Priority < Priority.Sum) is var addend && addend.StartsWith("-")
                       ? addend : "+" + addend);
            }
            public partial record Minusf
            {
                public override string Latexise() =>
                    Subtrahend.Latexise(Subtrahend.Priority < Priority.Minus)
                    + "-" + Minuend.Latexise(Minuend.Priority <= Priority.Minus);
            }
            public partial record Mulf
            {
                public override string Latexise() =>
                    (Multiplier == -1 ? "-" : Multiplier.Latexise(Multiplier.Priority < Priority.Mul) + @"\times ")
                    + Multiplicand.Latexise(Multiplicand.Priority < Priority.Mul);
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
                    var powerIfNeeded = Iterations == Integer.One ? "" : "^{" + Iterations.Latexise() + "}";

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

                    if (!(Iterations is Integer asInt && asInt >= 0))
                        return "Error";

                    if (asInt == 0)
                        return Expression.Latexise(false);

                    var sb = new StringBuilder();
                    for (int i = 0; i < asInt; i++)
                        sb.Append(@"\int ");
                    sb.Append(@"\left[");
                    sb.Append(Expression.Latexise(false));
                    sb.Append(@"\right]");

                    // TODO: can we write d^2 x or (dx)^2 instead of dx dx?
                    // I don't think I have ever seen the same variable being integrated more than one time. -- Happypig375
                    for (int i = 0; i < asInt; i++)
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

            public partial record Signumf
            {
                public override string Latexise()
                    => $@"\operatorname{{sgn}}\left({Argument.Latexise()}\right)";
            }

            public partial record Absf
            {
                public override string Latexise()
                    => $@"\left|{Argument.Latexise()}\right|";
            }
        }
    }
    namespace Core
    {
        partial record Set
        {
            public override string Latexise()
            {
                if (IsEmpty())
                    return @"\emptyset";

                var sb = new StringBuilder();
                sb.Append(@"\left\{");
                foreach (var p in Pieces)
                {
                    switch (p)
                    {
                        case OneElementPiece oneelem:
                            sb.Append(oneelem.UpperBound().Item1.Latexise());
                            break;
                        case Interval _:
                            var lower = p.LowerBound();
                            var upper = p.UpperBound();
                            var l = lower.Item1.Latexise();
                            var u = upper.Item1.Latexise();
                            switch (lower.Item2, lower.Item3, upper.Item2, upper.Item3)
                            {
                                // Complex part is inclusive for all real intervals which is [0, 0]i
                                case (false, true, false, true):
                                    sb.Append(@"\left(").Append(l).Append(',').Append(u).Append(@"\right)");
                                    break;
                                case (true, true, false, true):
                                    sb.Append(@"\left[").Append(l).Append(',').Append(u).Append(@"\right)");
                                    break;
                                case (false, true, true, true):
                                    sb.Append(@"\left(").Append(l).Append(',').Append(u).Append(@"\right]");
                                    break;
                                case (true, true, true, true):
                                    sb.Append(@"\left[").Append(l).Append(',').Append(u).Append(@"\right]");
                                    break;
                                case var (lr, li, ur, ui):
                                    static string Extract(Entity entity, bool takeReal) =>
                                        (entity, takeReal) switch
                                        {
                                            (Complex num, true) => num.RealPart.Latexise(),
                                            (Complex num, false) => num.ImaginaryPart.Latexise(),
                                            (_, true) => @"\Re\left(" + entity.Latexise() + @"\right)",
                                            (_, false) => @"\Im\left(" + entity.Latexise() + @"\right)",
                                        };
                                    sb.Append(@"\left\{z\in\mathbb C:\Re\left(z\right)\in\left")
                                        .Append(lr ? '[' : '(').Append(Extract(lower.Item1, true)).Append(',')
                                        .Append(Extract(upper.Item1, true)).Append(@"\right").Append(ur ? ']' : ')')
                                        .Append(@"\wedge\Im\left(z\right)\in\left")
                                        .Append(li ? '[' : '(').Append(Extract(lower.Item1, false)).Append(',')
                                        .Append(Extract(upper.Item1, false)).Append(@"\right").Append(ui ? ']' : ')')
                                        .Append(@"\right\}");
                                    break;
                            }
                            break;
                    }
                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1); // Remove extra ,
                sb.Append(@"\right\}");
                return sb.ToString();
            }
        }
        partial record SetNode : ILatexiseable
        {
            public abstract string Latexise();
            partial record Union
            {
                public override string Latexise() => $@"\left({A.Latexise()}\cup{B.Latexise()}\right)";
            }
            partial record Intersection
            {
                public override string Latexise() => $@"\left({A.Latexise()}\cap{B.Latexise()}\right)";
            }
            partial record Complement
            {
                public override string Latexise() => $@"\left({A.Latexise()}\setminus{B.Latexise()}\right)";
            }
            partial record Inversion
            {
                public override string Latexise() => $@"\left({A.Latexise()}^\complement\right)";
            }
        }
    }
}