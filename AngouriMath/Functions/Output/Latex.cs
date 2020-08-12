
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



using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
using AngouriMath.Core.Sys.Interfaces;
using PeterO.Numbers;

namespace AngouriMath
{
    public abstract partial record Entity : ILatexiseable
    {
        /// <summary>Returns the expression in LaTeX (for example, a / b -> \frac{a}{b})</summary>
        public abstract string Latexise();
        protected internal string Latexise(bool parenthesesRequired) =>
            parenthesesRequired ? @$"\left({Latexise()}\right)" : Latexise();
    }
    public partial record NumberEntity
    {
        public override string Latexise() => Value.Latexise();
    }

    public partial record VariableEntity
    {
        /// <summary>
        /// List of constants LaTeX will correctly display
        /// Yet to be extended
        /// Case does matter, not all letters have both displays in LaTeX
        /// </summary>
        private static readonly HashSet<string> LatexisableConstants = new HashSet<string>
        {
            "alpha", "beta", "gamma", "delta", "epsilon", "varepsilon", "zeta", "eta", "theta", "vartheta",
            "iota", "kappa", "varkappa", "lambda", "mu", "nu", "xi", "omicron", "pi", "varpi", "rho",
            "varrho", "sigma", "varsigma", "tau", "upsilon", "phi", "varphi", "chi", "psi", "omega",

            "Gamma", "Delta", "Theta", "Lambda", "Xi", "Pi", "Sigma", "Upsilon", "Phi", "Psi", "Omega",
        };

        /// <summary>
        /// Returns latexised const if it is possible to latexise it,
        /// or its original name otherwise
        /// </summary>
        public override string Latexise()
        {
            var constName = Name;
            var index = Functions.Utils.ParseIndex(constName);
            constName = index.prefix ?? constName;
            constName = LatexisableConstants.Contains(constName) ? @"\" + constName : constName;
            return index.prefix is null ? constName : (constName + "_{" + index.index + "}");
        }
    }

    public partial record Tensor
    {
        public override string Latexise()
        {
            if (IsMatrix)
            {
                var sb = new StringBuilder();
                sb.Append(@"\begin{pmatrix}");
                var lines = new List<string>();
                for (int x = 0; x < Shape[0]; x++)
                {
                    var items = new List<string>();

                    for (int y = 0; y < Shape[1]; y++)
                        items.Add(this[x, y].Latexise());

                    var line = string.Join(" & ", items);
                    lines.Add(line);
                }
                sb.Append(string.Join(@"\\", lines));
                sb.Append(@"\end{pmatrix}");
                return sb.ToString();
            }
            else if (IsVector)
            {
                var sb = new StringBuilder();
                sb.Append(@"\begin{bmatrix}");
                sb.Append(string.Join(" & ", innerTensor.Iterate().Select(k => k.Value.Latexise())));
                sb.Append(@"\end{bmatrix}");
                return sb.ToString();
            }
            else
            {
                return this.ToString();
            }
        }
    }

    public partial record Sumf
    {
        public override string Latexise() =>
            Augend.Latexise(Augend.Priority < Const.Priority.Sum)
            + (Addend.Latexise(Addend.Priority < Const.Priority.Sum) is var addend && addend.StartsWith("-")
               ? addend : "+" + addend);
    }
    public partial record Minusf
    {
        public override string Latexise() =>
            Subtrahend.Latexise(Subtrahend.Priority < Const.Priority.Minus)
            + "-" + Minuend.Latexise(Minuend.Priority <= Const.Priority.Minus);
    }
    public partial record Mulf
    {
        public override string Latexise() =>
            (Multiplier == -1 ? "-" : Multiplier.Latexise(Multiplier.Priority < Const.Priority.Mul) + @"\times ")
            + Multiplicand.Latexise(Multiplicand.Priority < Const.Priority.Mul);
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
            if (Exponent is NumberEntity { Value: RationalNumber {
                Value: { Numerator:var numerator, Denominator:var denominator }
            } and not IntegerNumber })
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
                return "{" + Base.Latexise(Base.Priority <= Const.Priority.Pow) + "}^{" + Exponent.Latexise() + "}";
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
            Argument.Latexise(Argument.Priority < Const.Priority.Num) + "!";
    }

    public partial record Derivativef
    {
        public override string Latexise()
        {
            var powerIfNeeded = Iterations == IntegerNumber.One ? "" : "^{" + Iterations.Latexise() + "}";

            var varOverDeriv =
                Variable is VariableEntity { Name: { Length: 1 } name }
                ? name
                : @"\left[" + Variable.Latexise(false) + @"\right]";

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

            if (!(Iterations is NumberEntity { Value: IntegerNumber asInt } && asInt >= 0))
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
                if (Variable is VariableEntity { Name: { Length: 1 } name })
                    sb.Append(name);
                else
                {
                    sb.Append(@"\left[");
                    sb.Append(Variable.Latexise(false));
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
            sb.Append(@"\lim_{").Append(Variable.Latexise())
              .Append(@"\to ").Append(Destination.Latexise());

            switch (ApproachFrom)
            {
                case Limits.ApproachFrom.Left:
                    sb.Append("^-");
                    break;
                case Limits.ApproachFrom.Right:
                    sb.Append("^+");
                    break;
            }

            sb.Append("} ");
            if (Expression.Priority < Const.Priority.Pow)
                sb.Append(@"\left[");
            sb.Append(Expression.Latexise());
            if (Expression.Priority < Const.Priority.Pow)
                sb.Append(@"\right]");

            return sb.ToString();
        }
    }

    namespace Core
    {
        partial class SetNode : ILatexiseable
        {
            public string Latexise()
            {
                var sb = new StringBuilder();
                switch (this)
                {
                    case Set set:
                        if (set.IsEmpty())
                        {
                            sb.Append(@"\emptyset");
                            break;
                        }

                        sb.Append(@"\left\{");
                        foreach (var p in set)
                        {
                            switch (p)
                            {
                                case OneElementPiece oneelem:
                                    sb.Append(oneelem.UpperBound().Item1.Latexise());
                                    break;
                                case IntervalPiece _:
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
                                                    (NumberEntity num, true) => num.Value.Real.Latexise(),
                                                    (NumberEntity num, false) => num.Value.Imaginary.Latexise(),
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
                        break;
                    case OperatorSet op:
                        var connector = op.ConnectionType switch
                        {
                            OperatorSet.OperatorType.UNION => @"\cup",
                            OperatorSet.OperatorType.INTERSECTION => @"\cap",
                            OperatorSet.OperatorType.COMPLEMENT => @"\setminus",
                            OperatorSet.OperatorType.INVERSION => @"^\complement",
                            _ => throw new Exceptions.UnknownOperatorException()
                        };
                        for (int i = 0; i < op.Children.Length; i++)
                        {
                            sb.Append(@"\left(").Append(op.Children[i].Latexise());
                            if (op.Children.Length == 1 || i < op.Children.Length - 1)
                                sb.Append(connector);
                            sb.Append(@"\right)");
                        }
                        break;
                }
                return sb.ToString();
            }
        }
    }
}
