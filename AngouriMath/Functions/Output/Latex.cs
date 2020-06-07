
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
using System.Text;
 using AngouriMath.Core.Sys.Interfaces;

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
                return entType switch
                {
                    EntType.VARIABLE => Const.LatexiseConst(Name),
                    EntType.TENSOR => ((Core.Tensor)this).Latexise(),
                    EntType.NUMBER => GetValue().Latexise(parenthesesRequired)
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
            if (args[1] == 0.5)
            {
                return @"\sqrt{" + args[0].Latexise() + "}";
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
    namespace Core
    {
        partial class SetNode : ILatexiseable
        {
            public string Latexise()
            {
                var sb = new StringBuilder();
                switch (Type)
                {
                    case NodeType.SET:
                        var set = this as Set;
                        if (set.IsEmpty())
                        {
                            sb.Append(@"\emptyset");
                            break;
                        }
                        
                        sb.Append(@"\left\{");
                        foreach(var p in set)
                        {
                            switch (p.Type)
                            {
                                case Piece.PieceType.ENTITY:
                                    sb.Append(((OneElementPiece)p).UpperBound().Item1.Latexise());
                                    break;
                                case Piece.PieceType.INTERVAL:
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
                                                (entity.entType, takeReal) switch
                                                {
                                                    (Entity.EntType.NUMBER, true) => entity.GetValue().Real.Latexise(),
                                                    (Entity.EntType.NUMBER, false) => entity.GetValue().Imaginary.Latexise(),
                                                    (_, true) => @"\Re\left(" + entity.Latexise() + @"\right)",
                                                    (_, false) => @"\Im\left(" + entity.Latexise() + @"\right)",
                                                };
                                            sb.Append(@"\left\{z\in\mathbb C:\Re\left(z\right)\in\left")
                                                .Append(lr ? '[' : '(').Append(Extract(lower.Item1, true)).Append(',')
                                                .Append(Extract(upper.Item1, true)).Append(@"\right").Append(ur ? ']' : ')')
                                                .Append(@"\wedge\Im\left(z\right)\in\left")
                                                .Append(li ? '[' : '(').Append(Extract(lower.Item1, false)).Append(',')
                                                .Append(Extract(upper.Item1, false)).Append(@"\right").Append(ur ? ']' : ')')
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
                    case NodeType.OPERATOR:
                        var op = (OperatorSet)this;
                        var connector = op.ConnectionType switch
                        {
                            OperatorSet.OperatorType.UNION => @"\cup",
                            OperatorSet.OperatorType.INTERSECTION => @"\cap",
                            OperatorSet.OperatorType.COMPLEMENT => @"\setminus",
                            OperatorSet.OperatorType.INVERSION => @"^\complement",
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
