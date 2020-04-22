
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
                return this.entType == EntType.NUMBER ? this.GetValue().ToString() : this.ToString();
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
            return args[0].Latexise(args[0].Priority < Const.PRIOR_SUM) + "+" + args[1].Latexise(args[1].Priority < Const.PRIOR_SUM);
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
            if (args[1] == 10)
                return @"\log\left(" + args[0].Latexise() + @"\right)";
            else if (args[1] == MathS.e)
                return @"\ln\left(" + args[0].Latexise() + @"\right)";
            else
                return @"\log_{" + args[1].Latexise() + @"}\left(" + args[0].Latexise() + @"\right)";
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
}
