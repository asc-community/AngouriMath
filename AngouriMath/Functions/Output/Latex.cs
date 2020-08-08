
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



﻿using System;
using System.Collections.Generic;
 using System.Numerics;
 using System.Text;
using AngouriMath.Core;
using AngouriMath.Core.Numerix;
 using AngouriMath.Core.Sys.Interfaces;
 using PeterO.Numbers;

namespace AngouriMath
{
    using LatexTable = Dictionary<string, Func<List<Entity>, string>>;

    public abstract partial class Entity : ILatexiseable
    {
        /// <summary>
        /// Returns the expression in format of latex (for example, a / b -> \frac{a}{b})
        /// </summary>
        /// <returns></returns>
        public string Latexise() => Latexise(false);
        internal string Latexise(bool parenthesesRequired)
        {
            if (IsLeaf)
                return this switch
                {
                    VariableEntity _ => Const.LatexiseConst(Name),
                    Tensor t => t.Latexise(),
                    // If parentheses are required, they might be only required when complicated numbers are wrapped,
                    // such as fractions and complex but not a single i
                    NumberEntity { Value:var value } => value.Latexise(Priority != Const.PRIOR_NUM && parenthesesRequired),
                    _ => throw new Core.Exceptions.UnknownEntityException()
                };
            else
                return MathFunctions.ParenthesesOnNeed(MathFunctions.InvokeLatex(Name, Children), parenthesesRequired, latex: true);
        }
    }

    internal static partial class MathFunctions
    {
        internal static readonly LatexTable latexTable = new LatexTable();

        /// <summary>
        /// Finds an appropriate function to call latex
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        internal static string InvokeLatex(string typeName, List<Entity> args)
        {
            return latexTable[typeName](args);
        }

        /// <summary>
        /// Wraps a string with parentheses
        /// </summary>
        /// <param name="s"></param>
        /// <param name="need"></param>
        /// <param name="latex"></param>
        /// <returns></returns>
        internal static string ParenthesesOnNeed(string s, bool need, bool latex)
        {
            if (latex)
                return need ? @"\left(" + s + @"\right)" : s;
            else
                return need ? @"(" + s + @")" : s;
        }
    }

    internal static partial class Sumf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            var arg1latex = args[1].Latexise(args[1].Priority < Const.PRIOR_SUM);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_SUM) +
                (arg1latex.StartsWith("-") ? arg1latex : "+" + arg1latex);
        }
    }
    internal static partial class Minusf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_MINUS) + "-" + args[1].Latexise(args[1].Priority <= Const.PRIOR_MINUS);
        }
    }
    internal static partial class Mulf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[0] == -1)
                return "-" + args[1].Latexise(args[1].Priority < Const.PRIOR_MUL);
            else
                return args[0].Latexise(args[0].Priority < Const.PRIOR_MUL) + @"\times " + args[1].Latexise(args[1].Priority < Const.PRIOR_MUL);
        }
    }
    internal static partial class Divf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            return @"\frac{" + args[0].Latexise() + "}{" + args[1].Latexise() + "}";
        }
    }
    internal static partial class Sinf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\sin\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Cosf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\cos\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Tanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\tan\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Cotanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\cot\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Logf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[0] == 10)
                return @"\log\left(" + args[1].Latexise() + @"\right)";
            else if (args[0] == MathS.e)
                return @"\ln\left(" + args[1].Latexise() + @"\right)";
            else
                return @"\log_{" + args[0].Latexise() + @"}\left(" + args[1].Latexise() + @"\right)";
        }
    }
    internal static partial class Powf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 2);
            if (args[1] is NumberEntity { Value:RationalNumber { IsFinite:true } rational }
                && !(rational is IntegerNumber))
            {
                var (numerator, denominator) = (rational.Value.Numerator, rational.Value.Denominator);
                var str = @"\sqrt" + (denominator.Equals(2) ? "" : "[" + denominator + "]") + 
                  "{" + args[0].Latexise() + "}";
                var abs = numerator.Abs();
                if (!abs.Equals(EInteger.One))
                    str += "^{" + abs + "}";
                if (numerator < 0)
                    str = @"\frac{1}{" + str + "}";
                return str;
            }
            else
            {
                return "{" + args[0].Latexise(args[0].Priority <= Const.PRIOR_POW) + "}^{" + args[1].Latexise() + "}";
            }
        }
    }
    internal static partial class Arcsinf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\arcsin\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Arccosf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\arccos\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Arctanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\arctan\left(" + args[0].Latexise() + @"\right)";
        }
    }
    internal static partial class Arccotanf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return @"\arccot\left(" + args[0].Latexise() + @"\right)";
        }
    }

    internal static partial class Factorialf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 1);
            return args[0].Latexise(args[0].Priority < Const.PRIOR_NUM) + "!";
        }
    }

    internal static partial class Derivativef
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            var pow = args[2];
            var powerIfNeeded = pow == IntegerNumber.One ? "" : "^{" + pow.Latexise() + "}";

            var varOverDeriv =
                args[1] is VariableEntity { Name: { Length: 1 } name }
                ? name
                : @"\left[" + args[1].Latexise(false) + @"\right]";

            // TODO: Should we display the d upright using \mathrm?
            // Differentiation is an operation, just like sin, cos, etc.
            return @"\frac{d" + powerIfNeeded +
            @"\left[" + args[0].Latexise(false) + @"\right]}{d" + varOverDeriv + powerIfNeeded + "}";
        }
    }

    internal static partial class Integralf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 3);
            var pow = args[2];

            // Unlike derivatives, integrals do not have "power" that would be equal
            // to sequential applying integration to a function

            if (!(pow is NumberEntity { Value: IntegerNumber asInt } && asInt >= 0))
                return "Error";

            if (asInt == 0)
                return args[0].Latexise(false);

            var sb = new StringBuilder();
            for (int i = 0; i < asInt; i++)
                sb.Append(@"\int ");
            sb.Append(@"\left[");
            sb.Append(args[0].Latexise(false));
            sb.Append(@"\right]");

            // TODO: can we write d^2 x or (dx)^2 instead of dx dx?
            // I don't think I have ever seen the same variable being integrated more than one time. -- Happypig375
            for (int i = 0; i < asInt; i++)
            {
                sb.Append(" d");
                if (args[1] is VariableEntity { Name: { Length: 1 } name })
                    sb.Append(name);
                else
                {
                    sb.Append(@"\left[");
                    sb.Append(args[1].Latexise(false));
                    sb.Append(@"\right]");
                }
            }
            return sb.ToString();
        }
    }

    internal static partial class Limitf
    {
        internal static string Latex(List<Entity> args)
        {
            MathFunctions.AssertArgs(args.Count, 4);

            var sb = new StringBuilder();
            sb.Append(@"\lim_{");

            sb.Append(args[1].Latexise(false));

            sb.Append(@"\to ");

            sb.Append(args[2].Latexise(false));

            if (args[3] == IntegerNumber.MinusOne)
                sb.Append("^-");
            else if (args[3] == IntegerNumber.One)
                sb.Append("^+");
            else if (args[3] != IntegerNumber.Zero)
                return "Error";

            sb.Append("} ");
            if (args[0].Priority < Const.PRIOR_POW)
                sb.Append(@"\left[");
            sb.Append(args[0].Latexise(false));
            if (args[0].Priority < Const.PRIOR_POW)
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
                        foreach(var p in set)
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
                        for(int i = 0; i < op.Children.Length; i++)
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
